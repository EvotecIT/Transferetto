function Receive-FTPFile {
    [alias('Get-FTPFile')]
    [cmdletBinding(DefaultParameterSetName = 'Text')]
    param(
        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,

        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpListItem[]] $RemoteFile,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [string[]] $RemotePath,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [string] $LocalPath,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpLocalExists] $LocalExists = [FluentFTP.FtpLocalExists]::Skip,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpVerify[]] $VerifyOptions = [FluentFTP.FtpVerify]::None,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [FluentFTP.FtpError] $FtpError = [FluentFTP.FtpError]::Stop,

        [Parameter(ParameterSetName = 'Text')]
        [Parameter(ParameterSetName = 'Native')]
        [switch] $Suppress
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Path = Get-Item -LiteralPath $LocalPath -ErrorAction SilentlyContinue
        if ($Path -is [System.IO.DirectoryInfo]) {
            Get-PrivateFTPFiles -Client $Client -LocalPath $LocalPath -RemoteFile $RemoteFile -RemotePath $RemotePath -LocalExists $LocalExists -VerifyOptions $VerifyOptions -FtpError $FtpError
        } else {
            if ($RemoteFile.Count -gt 1 -or $RemotePath.Count -gt 1) {
                Write-Warning "Receive-FTPFile - Multiple files detected, but $LocalPath is not a directory or directory doesn't exists. "
                if ($RemoteFile) {
                    $FileToDownload = $RemoteFile.FullName
                } else {
                    $FileToDownload = $RemotePath
                }
                $Status = [PSCustomObject] @{
                    Action     = 'DownloadFile'
                    Status     = $false
                    LocalPath  = $LocalPath
                    RemotePath = $FileToDownload
                    Message    = "Multiple files detected, but $LocalPath is not a directory or directory doesn't exists."
                }
            } else {
                $Splat = @{
                    Client        = $Client
                    LocalExists   = $LocalExists
                    VerifyOptions = $VerifyOptions
                    FtpError      = $FtpError
                    LocalPath     = $LocalPath
                }
                if ($RemoteFile) {
                    $Splat.RemoteFile = $RemoteFile[0]
                } else {
                    $Splat.RemotePath = $RemotePath[0]
                }
                Get-PrivateFTPFile @Splat
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