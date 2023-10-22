Clear-Host
Import-Module .\Transferetto.psd1 -Force

# Connect to SFTP Server
$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username demo -Password password

# Get All Files in '/test' for Export
$Export_Files = Get-SFTPList -SftpClient $SftpClient -Path '/pub/example' | Where-Object { $_.IsDirectory -eq $false }

# Set Export Directory
$ExportPathLocal = "C:\Temp"

# Download Each File
$Output = ForEach ($RemoteFile in $Export_Files) {
    Receive-SFTPFile -SftpClient $SftpClient -RemotePath $RemoteFile.FullName -LocalPath "$ExportPathLocal\$($RemoteFile.Name)"
}
$Output | Format-Table

# Disconnect
Disconnect-FTP -Client $Client
