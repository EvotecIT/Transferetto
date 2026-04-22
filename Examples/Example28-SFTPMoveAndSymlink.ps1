Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'

Move-SFTPFile -SftpClient $SftpClient -SourcePath '/home/przemek/www/index.html' -DestinationPath '/home/przemek/www/index.previous.html'
Move-SFTPDirectory -SftpClient $SftpClient -SourcePath '/home/przemek/www/releases/current' -DestinationPath '/home/przemek/www/releases/backup-current' -PosixRename

New-SFTPSymbolicLink -SftpClient $SftpClient -TargetPath '/home/przemek/www/releases/2026-04-15' -LinkPath '/home/przemek/www/current'
Test-SFTPSymbolicLink -SftpClient $SftpClient -Path '/home/przemek/www/current'
Get-SFTPList -SftpClient $SftpClient -Path '/home/przemek/www' | Format-Table Name, FullName, IsDirectory, IsRegularFile, IsSymbolicLink

Disconnect-SFTP -SftpClient $SftpClient
