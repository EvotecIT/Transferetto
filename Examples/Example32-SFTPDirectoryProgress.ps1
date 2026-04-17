Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)

$Upload = Send-SFTPDirectory -SftpClient $SftpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/incoming/progress-demo' `
    -AllowOverride `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Upload | Format-Table Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes

$Download = Receive-SFTPDirectory -SftpClient $SftpClient `
    -RemotePath '/incoming/progress-demo' `
    -LocalPath "$PSScriptRoot\Download\progress-demo" `
    -AllowOverride `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Download | Format-Table Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes

Disconnect-SFTP -SftpClient $SftpClient
