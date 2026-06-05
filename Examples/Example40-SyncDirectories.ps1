Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential
$SftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential $Credential

$Preview = Sync-SFTPDirectory -SftpClient $SftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/releases' `
    -Mode Mirror `
    -Include '*.zip', '*.json' `
    -Exclude 'archive/*' `
    -DryRun

$Preview | Format-Table Action, IsDryRun, RelativePath, LocalPath, RemotePath, Message

$Result = Sync-SFTPDirectory -SftpClient $SftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/releases' `
    -Mode Mirror `
    -Include '*.zip', '*.json' `
    -Exclude 'archive/*' `
    -ShowProgress

$Result | Format-Table Action, Status, RelativePath, BytesTransferred, Message

Disconnect-SFTP -SftpClient $SftpClient

$FtpClient = Connect-FTP -Server 'ftp.example.com' -Credential $Credential -EncryptionMode Explicit

Sync-FTPDirectory -Client $FtpClient `
    -Direction Download `
    -RemotePath '/exports/nightly' `
    -LocalPath "$PSScriptRoot\Download\Nightly" `
    -Mode Update `
    -Comparison SizeOrLastWriteTime `
    -NoPreserveTimestamps

Disconnect-FTP -Client $FtpClient
