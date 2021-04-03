function Test-FTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $RemotePath
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.FileExists($RemotePath)
    }
}