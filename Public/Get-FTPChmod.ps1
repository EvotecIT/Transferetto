function Get-FTPChmod {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.GetChmod($RemotePath)
    }
}