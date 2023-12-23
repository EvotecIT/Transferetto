Import-Module .\Transferetto.psd1 -Force

Set-FTPTracing -Enable
# # Login via UserName/Password via proxy
# $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -ProxyHost '192.252.216.81' -ProxyPort 4145 -ProxyType FtpClientSocks5Proxy
# $List = Get-FTPList -Client $Client
# $List | Format-Table
# Disconnect-FTP -Client $Client


# $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'
# $List = Get-FTPList -Client $Client
# $List | Format-Table
# Receive-FTPFile -Client $Client -RemotePath '/readme.txt' -LocalPath 'C:\Temp\readme.txt'
# Disconnect-FTP -Client $Client


# Login via UserName/Password with Proxy
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -ProxyHost '104.37.135.145' -ProxyPort 4145 -ProxyType FtpClientSocks4aProxy
$List = Get-FTPList -Client $Client
$List | Format-Table
#Remove-Item -LiteralPath 'C:\Temp\readme.txt'
#Receive-FTPFile -Client $Client -RemotePath '/readme.txt' -LocalPath 'C:\Temp\readme.txt'
Disconnect-FTP -Client $Client


# Set-FTPTracing -Enable
# # Login via UserName/Password
# $Client = Connect-FTP -Server 'ftp.dlptest.com' -Verbose -Username 'dlpuser' -Password 'rNrKYTX9g7z3RgJRmxWuGHbeu'
# $List = Get-FTPList -Client $Client
# $List | Format-Table
# $List.Count
# Disconnect-FTP -Client $Client

# # Login via UserName/Password
# $Client = Connect-FTP -Server 'ftp.dlptest.com' -Verbose -Username 'dlpuser' -Password 'rNrKYTX9g7z3RgJRmxWuGHbeu' -ProxyHost '104.37.135.145' -ProxyPort 4145 -ProxyType FtpClientSocks4aProxy
# $List = Get-FTPList -Client $Client
# $List | Format-Table
# $List.Count
# Disconnect-FTP -Client $Client