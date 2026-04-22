Import-Module .\Transferetto.psd1 -Force

$FtpClient = Connect-FTP -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'

Get-FTPWorkingDirectory -Client $FtpClient
Set-FTPWorkingDirectory -Client $FtpClient -Path '/public_html'
Get-FTPItem -Client $FtpClient -RemotePath '/public_html/index.html'
Get-FTPFileSize -Client $FtpClient -RemotePath '/public_html/index.html'
Get-FTPModifiedTime -Client $FtpClient -RemotePath '/public_html/index.html'

New-FTPDirectory -Client $FtpClient -RemotePath '/public_html/releases' -Force
Set-FTPModifiedTime -Client $FtpClient -RemotePath '/public_html/index.html' -ModifiedTime (Get-Date).AddMinutes(-10) -PassThru

Disconnect-FTP -Client $FtpClient
