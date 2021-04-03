function Remove-FTPDirectory {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpListOption] $FtpListOption
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        if (-not $FtpListOption) {
            $Client.DeleteDirectory($RemotePath)
        } else {
            $Client.DeleteDirectory($RemotePath, $FtpListOption)
        }
    }
}