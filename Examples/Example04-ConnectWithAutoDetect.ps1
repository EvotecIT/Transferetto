Import-Module .\Transferetto.psd1 -Force

# Login via UserName/Password via autodetect - keep in mind using Connect-FTP directly will be faster...
$ProfileFtp1 = Request-FTPConfiguration -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'
$ProfileFtp1 | Format-Table

# use first profile
$Client = Connect-FTP -FtpProfile $ProfileFtp1[0]
Get-FTPList -Client $Client | Format-Table
Disconnect-FTP -Client $Client