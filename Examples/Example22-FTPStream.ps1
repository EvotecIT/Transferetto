Import-Module .\Transferetto.psd1 -Force

$FtpClient = Connect-FTP -Server 'test.rebex.net' -Username 'demo' -Password 'password'

$WriteStream = Open-FTPStream -Client $FtpClient -RemotePath '/pub/example/ftp-stream-demo.txt' -Mode Write
Write-FTPStream -StreamSession $WriteStream -Text "first line`n"
Write-FTPStream -StreamSession $WriteStream -Text "second line`n" -Flush
Close-FTPStream -StreamSession $WriteStream

$ReadStream = Open-FTPStream -Client $FtpClient -RemotePath '/pub/example/ftp-stream-demo.txt' -Mode Read
$Chunk = Read-FTPStream -StreamSession $ReadStream -Count 32
$Chunk | Format-List

if ($ReadStream.CanSeek) {
    Set-FTPStreamPosition -StreamSession $ReadStream -Offset 0 | Out-Null
    Read-FTPStream -StreamSession $ReadStream -AsText -Count 128
}

Close-FTPStream -StreamSession $ReadStream
Disconnect-FTP -Client $FtpClient
