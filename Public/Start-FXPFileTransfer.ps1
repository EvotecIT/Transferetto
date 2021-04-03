function Start-FXPFileTransfer {
    [alias('Start-FXPFile')]
    [cmdletBinding()]
    param(
        [alias('SourceClient')][Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $SourcePath,
        [Parameter(Mandatory)][FluentFTP.FtpClient] $DestinationClient,
        [Parameter(Mandatory)][string] $DestinationPath,
        [switch] $CreateRemoteDirectory,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify[]] $VerifyOptions = [FluentFTP.FtpVerify]::None
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        # FluentFTP.FtpStatus TransferFile(string sourcePath, FluentFTP.FtpClient remoteClient, string remotePath, bool createRemoteDir, FluentFTP.FtpRemoteExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Action[FluentFTP.FtpProgress] progress, FluentFTP.FtpProgress metaProgress)
        $Client.TransferFile($SourcePath, $DestinationClient, $DestinationPath, $CreateRemoteDirectory.IsPresent, $RemoteExists, $VerifyOptions)
    }
}