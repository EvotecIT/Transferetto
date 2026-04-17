Import-Module .\Transferetto.psd1 -Force

$SourceCredential = Get-Credential -Message 'Source FTP credentials'
$DestinationCredential = Get-Credential -Message 'Destination FTP credentials'

$SourceClient = Connect-FTP -Server 'source-ftp.example.com' -Credential $SourceCredential -EncryptionMode Explicit
$DestinationClient = Connect-FTP -Server 'destination-ftp.example.com' -Credential $DestinationCredential -EncryptionMode Explicit

$Preflight = Test-FXPTransfer -Client $SourceClient `
    -SourcePath '/exports/large-file.zip' `
    -DestinationClient $DestinationClient `
    -DestinationPath '/imports/large-file.zip' `
    -TransferKind File `
    -CreateRemoteDirectory

$Preflight | Format-List Status, SourceConnected, DestinationConnected, SourcePathExists, DestinationParentExists, Messages
if (-not $Preflight.Status) {
    throw 'FXP preflight failed. Review the preflight messages before starting the transfer.'
}

$FileTransfer = Start-FXPFileTransfer -Client $SourceClient `
    -SourcePath '/exports/large-file.zip' `
    -DestinationClient $DestinationClient `
    -DestinationPath '/imports/large-file.zip' `
    -CreateRemoteDirectory `
    -RemoteExists Overwrite `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$FileTransfer | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$DirectoryTransfer = Start-FXPDirectoryTransfer -Client $SourceClient `
    -SourcePath '/exports/site' `
    -DestinationClient $DestinationClient `
    -DestinationPath '/imports/site' `
    -FolderSyncMode Update `
    -RemoteExists Overwrite `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$DirectoryTransfer | Format-Table Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes

Disconnect-FTP -Client $SourceClient
Disconnect-FTP -Client $DestinationClient
