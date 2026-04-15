Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username 'demo' -Password 'password'

Send-SFTPDirectory -SftpClient $SftpClient -LocalPath "$PSScriptRoot\Upload" -RemotePath '/pub/example/upload-demo' -AllowOverride

Receive-SFTPDirectory -SftpClient $SftpClient -RemotePath '/pub/example' -LocalPath "$PSScriptRoot\Download\SftpExample" -AllowOverride

Disconnect-SFTP -SftpClient $SftpClient
