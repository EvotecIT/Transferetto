function Rename-FTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $Path,
        [Parameter(Mandatory)][string] $DestinationPath
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.Rename($Path, $DestinationPath)
    }
}