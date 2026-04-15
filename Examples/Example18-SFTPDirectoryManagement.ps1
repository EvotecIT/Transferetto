Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username 'demo' -Password 'password'

Get-SFTPWorkingDirectory -SftpClient $SftpClient

New-SFTPDirectory -SftpClient $SftpClient -Path '/pub/example/temp'
Test-SFTPDirectory -SftpClient $SftpClient -Path '/pub/example/temp'

Get-SFTPItem -SftpClient $SftpClient -Path '/pub/example'

Set-SFTPWorkingDirectory -SftpClient $SftpClient -Path '/pub/example' -PassThru | Out-Null
Get-SFTPWorkingDirectory -SftpClient $SftpClient

Remove-SFTPDirectory -SftpClient $SftpClient -Path '/pub/example/temp'
Disconnect-SFTP -SftpClient $SftpClient
