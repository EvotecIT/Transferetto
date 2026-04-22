Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A'
Get-SFTPList -SftpClient $SftpClient | Format-Table
Get-SFTPList -SftpClient $SftpClient -Path '/Temporary' | Format-Table *

Send-SFTPDirectory -SftpClient $SftpClient -LocalPath "$PSScriptRoot\Upload" -RemotePath '/Temporary' -AllowOverride

Disconnect-SFTP -SftpClient $SftpClient
