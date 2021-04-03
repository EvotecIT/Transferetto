function Get-PrivateFTPFiles {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $LocalPath,
        [FluentFTP.FtpListItem[]]  $RemoteFile,
        [string[]] $RemotePath,
        [FluentFTP.FtpLocalExists] $LocalExists,
        [FluentFTP.FtpVerify[]] $VerifyOptions,
        [FluentFTP.FtpError] $FtpError
    )

    if ($RemoteFile) {
        $FileToDownload = foreach ($File in $RemoteFile) {
            if ($File.Type -eq 'File') {
                $File.FullName
            } else {
                if (-not $Suppress) {
                    [PSCustomObject] @{
                        Action     = 'DownloadFile'
                        Status     = $false
                        LocalPath  = $LocalPath
                        RemotePath = $File.FullName
                        Message    = "Receive-FTPFile - Given path $($RemoteFile.FullName) is $($RemoteFile.Type). Skipping."
                    }
                } else {
                    Write-Warning "Receive-FTPFile - Given path $($RemoteFile.FullName) is a directory. Skipping."
                }
            }
        }
    } else {
        $FileToDownload = $RemotePath
    }
    try {
        $Message = $Client.DownloadFiles($LocalPath, ([string[]] $FileToDownload), $LocalExists, $VerifyOptions, $FtpError)
        if ($Message -gt 0) {
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
            Write-Warning "Receive-FTPFile - Error: $($_.Exception.Message)"
        }
    }
    $Status
}