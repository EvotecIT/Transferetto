Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'

Get-SFTPItem -SftpClient $SftpClient -Path '/home/przemek/www/index.html'
Get-SFTPChmod -SftpClient $SftpClient -Path '/home/przemek/www/index.html'

Set-SFTPChmod -SftpClient $SftpClient -Path '/home/przemek/www/index.html' -Permissions '644' -PassThru
Set-SFTPTimestamp -SftpClient $SftpClient -Path '/home/przemek/www/index.html' -LastWriteTime (Get-Date).AddMinutes(-5) -PassThru

Disconnect-SFTP -SftpClient $SftpClient
