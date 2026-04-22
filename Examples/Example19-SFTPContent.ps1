Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username 'demo' -Password 'password'

Set-SFTPContent -SftpClient $SftpClient -Path '/pub/example/temp/config.txt' -Value "first line`n"
Set-SFTPContent -SftpClient $SftpClient -Path '/pub/example/temp/config.txt' -Value 'second line' -Append

Get-SFTPContent -SftpClient $SftpClient -Path '/pub/example/temp/config.txt'

Disconnect-SFTP -SftpClient $SftpClient
