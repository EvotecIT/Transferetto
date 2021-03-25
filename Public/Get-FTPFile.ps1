function Get-FTPFile {
    [cmdletBinding(DefaultParameterSetName = 'Text')]
    param(
        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,

        [Parameter(ParameterSetName = 'Native')]
        [FtpListItem] $RemoteFile,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [string] $RemotePath,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [string] $LocalPath,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpLocalExists] $LocalExists = [FluentFTP.FtpLocalExists]::Skip,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpError] $FtpError = [FluentFTP.FtpError]::Stop,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [switch] $Suppress
    )


    if ($RemoteFile) {
        if ($RemoteFile.Type -ne 'Directory') {
            $FileToDownload = $RemoteFile.FullName
        } else {
            if (-not $Suppress) {
                [PSCustomObject] @{
                    Action     = 'DownloadFile'
                    Status     = $false
                    LocalPath  = $LocalPath
                    RemotePath = $RemoteFile.FullName
                    Message    = 'Get-FTPFile - Given path $($RemoteFile.FullName) is a directory. Skipping.'
                }
            } else {
                Write-Warning "Get-FTPFile - Given path $($RemoteFile.FullName) is a directory. Skipping."
                return
            }
        }
    } else {
        $FileToDownload = $RemotePath
    }
    if ($Client -and $Client.IsConnected) {
        try {
            $Message = $Client.DownloadFile($LocalPath, $FileToDownload, $LocalExists, $VerifyOptions)
            if ($Message -eq 'success') {
                $State = $true
            } else {
                $State = $false
            }
            $Status = [PSCustomObject] @{
                Action     = 'DownloadFile'
                Status     = $State
                LocalPath  = $LocalPath
                RemotePath = $FileToDownload
                Message    = $Message
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action     = 'DownloadFile'
                Status     = $false
                LocalPath  = $LocalPath
                RemotePath = $FileToDownload
                Message    = "Error: $($_.Exception.Message)"
            }
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-FTPFile - Error: $($_.Exception.Message)"
            }
        }
    } else {
        $Status = [PSCustomObject] @{
            Action     = 'DownloadFile'
            Status     = $false
            LocalPath  = $LocalPath
            RemotePath = $FileToDownload
            Message    = "Not connected."
        }
    }
    if (-not $Suppress) {
        $Status
    }
}