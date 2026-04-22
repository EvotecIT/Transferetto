Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential
$FtpClient = Connect-FTP -Server 'ftp.example.com' -Credential $Credential -EncryptionMode Explicit

$Upload = Send-FTPFile -Client $FtpClient `
    -LocalPath "$PSScriptRoot\Upload\large-file.zip" `
    -RemotePath '/incoming/large-file.zip' `
    -RemoteExists Overwrite `
    -CreateRemoteDirectory `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Upload | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$Download = Receive-FTPFile -Client $FtpClient `
    -RemotePath '/incoming/large-file.zip' `
    -LocalPath "$PSScriptRoot\Download\large-file.zip" `
    -LocalExists Overwrite `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$Download | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

Disconnect-FTP -Client $FtpClient
