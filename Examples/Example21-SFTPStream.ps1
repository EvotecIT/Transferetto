Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server 'test.rebex.net' -Username 'demo' -Password 'password'

$WriteStream = Open-SFTPStream -SftpClient $SftpClient -Path '/pub/example/stream-demo.txt' -Mode Write
Write-SFTPStream -StreamSession $WriteStream -Text "first line`n"
Write-SFTPStream -StreamSession $WriteStream -Text "second line`n" -Flush
Close-SFTPStream -StreamSession $WriteStream

$ReadStream = Open-SFTPStream -SftpClient $SftpClient -Path '/pub/example/stream-demo.txt' -Mode Read
$Chunk = Read-SFTPStream -StreamSession $ReadStream -Count 32
$Chunk | Format-List

Set-SFTPStreamPosition -StreamSession $ReadStream -Offset 0 | Out-Null
Read-SFTPStream -StreamSession $ReadStream -AsText -Count 128

Close-SFTPStream -StreamSession $ReadStream
Disconnect-SFTP -SftpClient $SftpClient
