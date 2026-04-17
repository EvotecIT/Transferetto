Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential
$FtpClient = Connect-FTP -Server 'ftp.example.com' -Credential $Credential -EncryptionMode Explicit

$Upload = Send-FTPDirectory -Client $FtpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/progress-demo' `
    -FolderSyncMode Update `
    -RemoteExists Overwrite `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Upload | Format-Table Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes

$Download = Receive-FTPDirectory -Client $FtpClient `
    -RemotePath '/incoming/progress-demo' `
    -LocalPath "$PSScriptRoot\Download\progress-demo" `
    -FolderSyncMode Update `
    -LocalExists Overwrite `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Download | Format-Table Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes

Disconnect-FTP -Client $FtpClient
