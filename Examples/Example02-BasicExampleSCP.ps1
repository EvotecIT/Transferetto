Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.240.29' -Username 'przemek' -Password 'YourPassword'
Get-SFTPList -SftpClient $SftpClient | Format-Table
Get-SFTPList -SftpClient $SftpClient -Path "/home" | Format-Table
Get-SFTPItem -SftpClient $SftpClient -RemotePath '/home/przemek/test1.txt' -LocalPath "$PSScriptRoot\Downloads\mmm.txt"
Add-SFTPItem -SftpClient $SftpClient -LocalPath "$PSScriptRoot\Downloads\mmm.txt" -RemotePath '/home/przemek/mmm.txt' -AllowOverride