Import-Module .\Transferetto.psd1 -Force

# Login via UserName/Password via autoconnect - this will try all options and connect, while displaying Verbose what settings were used to achieve connection
# Useful for trying out every possible combination
$Client = Connect-FTP -Server 'test.rebex.net' -Username 'demo' -Password 'password' -AutoConnect -Verbose
Get-FTPList -Client $Client | Format-Table
Disconnect-FTP -Client $Client