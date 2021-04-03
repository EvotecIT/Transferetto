function Send-FTPFile {
    [alias('Add-FTPFile')]
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath,
        [System.IO.FileInfo[]] $LocalFile,
        [string[]] $LocalPath,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [FluentFTP.FtpError] $ErrorHandling = [FluentFTP.FtpError]::None,
        [switch] $CreateRemoteDirectory
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        if ($LocalPath.Count -gt 1 -or $LocalFile.Count -gt 1) {
            $Splat = @{
                Client                = $Client
                RemoteExists          = $RemoteExists
                VerifyOptions         = $VerifyOptions
                LocalPath             = $LocalPath
                LocalFile             = $LocalFile
                RemotePath            = $RemotePath
                CreateRemoteDirectory = $CreateRemoteDirectory.IsPresent
                ErrorHandling         = $ErrorHandling
            }
            Remove-EmptyValue -Hashtable $Splat
            $Status = Add-PrivateFTPFiles @Splat
            $Status
        } else {
            foreach ($Path in $LocalPath) {
                $Splat = @{
                    Client                = $Client
                    RemoteExists          = $RemoteExists
                    VerifyOptions         = $VerifyOptions
                    LocalPath             = $Path
                    RemotePath            = $RemotePath
                    CreateRemoteDirectory = $CreateRemoteDirectory.IsPresent
                }
                $Status = Add-PrivateFTPFile @Splat
                $Status
            }
        }
    }
}