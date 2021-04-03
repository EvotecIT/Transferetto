function Receive-FTPDirectory {
    [alias('Get-FTPDirectory')]
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $LocalPath,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpFolderSyncMode] $FolderSyncMode = [FluentFTP.FtpFolderSyncMode]::Update,
        [FluentFTP.FtpLocalExists] $LocalExists,
        [FluentFTP.FtpVerify] $VerifyOptions,
        [FluentFTP.Rules.FtpRule[]] $Rules
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        #System.Collections.Generic.List[FluentFTP.FtpResult] DownloadDirectory(string localFolder, string remoteFolder, FluentFTP.FtpFolderSyncMode mode, FluentFTP.FtpLocalExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Collections.Generic.List[FluentFTP.Rules.FtpRule] rules, System.Action[FluentFTP.FtpProgress] progress)
        #System.Collections.Generic.List[FluentFTP.FtpResult] IFtpClient.DownloadDirectory(string localFolder, string remoteFolder, FluentFTP.FtpFolderSyncMode mode, FluentFTP.FtpLocalExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Collections.Generic.List[FluentFTP.Rules.FtpRule] rules, System.Action[FluentFTP.FtpProgress] progress)
        $Client.DownloadDirectory($LocalPath, $RemotePath, $FolderSyncMode)
    }
}
