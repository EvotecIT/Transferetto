Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential)

$Upload = Send-SFTPFile -SftpClient $SftpClient `
    -LocalPath "$PSScriptRoot\Upload\large-file.zip" `
    -RemotePath '/incoming/large-file.zip' `
    -AllowOverride `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Upload | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$Download = Receive-SFTPFile -SftpClient $SftpClient `
    -RemotePath '/incoming/large-file.zip' `
    -LocalPath "$PSScriptRoot\Download\large-file.zip" `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Download | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

Disconnect-SFTP -SftpClient $SftpClient
