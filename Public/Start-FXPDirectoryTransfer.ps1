function Start-FXPDirectoryTransfer {
    [alias('Start-FXPDirectory')]
    [cmdletBinding()]
    param(
        [alias('SourceClient')][Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $SourcePath,
        [Parameter(Mandatory)][FluentFTP.FtpClient] $DestinationClient,
        [Parameter(Mandatory)][string] $DestinationPath,
        [FluentFTP.FtpFolderSyncMode] $FolderSyncMode = [FluentFTP.FtpFolderSyncMode]::Update,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify[]] $VerifyOptions = [FluentFTP.FtpVerify]::None
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        # System.Collections.Generic.List[FluentFTP.FtpResult] TransferDirectory(string sourceFolder, FluentFTP.FtpClient remoteClient, string remoteFolder, FluentFTP.FtpFolderSyncMode mode, FluentFTP.FtpRemoteExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Collections.Generic.List[FluentFTP.Rules.FtpRule] rules, System.Action[FluentFTP.FtpProgress] progress)
        $Client.TransferDirectory($SourcePath, $DestinationClient, $DestinationPath, $FolderSyncMode, $RemoteExists, $VerifyOptions)
    }
}