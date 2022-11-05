Import-Module .\Transferetto.psd1 -Force

# Login via UserName/Password via proxy
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -ProxyHost '192.252.216.81' -ProxyPort 4145 -ProxyType FtpClientSocks5Proxy
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client


# Login via UserName/Password with Proxy
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -ProxyHost '104.37.135.145' -ProxyPort 4145 -ProxyType FtpClientSocks4aProxy
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client


# # Login via UserName/Password via proxy
# $Client = Connect-FTP -Server 'ftp.gnu.org' -Verbose -ProxyHost '192.252.216.81' -ProxyPort 4145 -ProxyType FtpClientSocks5Proxy
# $List = Get-FTPList -Client $Client
# $List | Format-Table
# Disconnect-FTP -Client $Client