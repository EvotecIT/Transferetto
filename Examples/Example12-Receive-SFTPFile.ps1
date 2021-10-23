Import-Module .\Transferetto.psd1 -Force

# Connect to SFTP Server
$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username demo -Password password

# Get All Files in '/test' for Export
$Export_Files = Get-SFTPList -SftpClient $SftpClient -Path '/pub/example' | Where-Object {$_.IsDirectory -eq $false} 

# Set Export Directory
$ExportPathLocal = "C:\Temp\sftp_demo"

# Download Each File
ForEach ($RemoteFile in $Export_Files) {

    Receive-SFTPFile -SftpClient $SftpClient -RemotePath $RemoteFile.FullName -LocalPath "$ExportPathLocal\$($RemoteFile.Name)"

}

# Disconnect
Disconnect-FTP -Client $Client
