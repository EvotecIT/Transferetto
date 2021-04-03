function Move-FTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $RemoteSource,
        [Parameter(Mandatory)][string] $RemoteDestination,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.MoveFile($RemoteSource, $RemoteDestination, $RemoteExists)
    }
}