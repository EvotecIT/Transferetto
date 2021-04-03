function Send-FTPDirectory {
    [alias('Add-FTPDirectory')]
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $LocalPath,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpFolderSyncMode] $FolderSyncMode = [FluentFTP.FtpFolderSyncMode]::Update,
        [FluentFTP.FtpRemoteExists] $RemoteExists,
        [FluentFTP.FtpVerify] $VerifyOptions,
        [FluentFTP.Rules.FtpRule[]] $Rules
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        # System.Collections.Generic.List[FluentFTP.FtpResult] UploadDirectory(string localFolder, string remoteFolder, FluentFTP.FtpFolderSyncMode mode, FluentFTP.FtpRemoteExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Collections.Generic.List[FluentFTP.Rules.FtpRule] rules, System.Action[FluentFTP.FtpProgress] progress)
        # System.Collections.Generic.List[FluentFTP.FtpResult] IFtpClient.UploadDirectory(string localFolder, string remoteFolder, FluentFTP.FtpFolderSyncMode mode, FluentFTP.FtpRemoteExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Collections.Generic.List[FluentFTP.Rules.FtpRule] rules, System.Action[FluentFTP.FtpProgress] progress)

        $Client.UploadDirectory($LocalPath, $RemotePath, $FolderSyncMode) #, $RemoteExists, $VerifyOptions, @($Rules))

    }
}
