Import-Module .\Transferetto.psd1 -Force

$ScpClient = Connect-SCP -Server 'linux.example.com' -Credential (Get-Credential)

$UploadFile = Send-SCPFile -ScpClient $ScpClient `
    -LocalPath "$PSScriptRoot\Upload\large-file.zip" `
    -RemotePath '/tmp/large-file.zip' `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$UploadFile | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$DownloadFile = Receive-SCPFile -ScpClient $ScpClient `
    -RemotePath '/tmp/large-file.zip' `
    -LocalPath "$PSScriptRoot\Download\large-file.zip" `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$DownloadFile | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$UploadDirectory = Send-SCPDirectory -ScpClient $ScpClient `
    -LocalPath "$PSScriptRoot\Upload" `
    -RemotePath '/tmp/upload-demo' `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$UploadDirectory | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

$DownloadDirectory = Receive-SCPDirectory -ScpClient $ScpClient `
    -RemotePath '/tmp/upload-demo' `
    -LocalPath "$PSScriptRoot\Download\scp-demo" `
    -ShowProgress `
    -ProgressIntervalBytes 262144

$DownloadDirectory | Format-List Action, Status, LocalPath, RemotePath, BytesTransferred, TotalBytes, Elapsed

Disconnect-SCP -ScpClient $ScpClient
