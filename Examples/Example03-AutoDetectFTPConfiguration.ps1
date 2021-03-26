Import-Module .\Transferetto.psd1 -Force

# Login via UserName/Password
$ProfileFtp1 = Request-FTPConfiguration -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'
$ProfileFtp1 | Format-Table

# Anonymous login
$ProfileFtp2 = Request-FTPConfiguration -Server 'speedtest.tele2.net' -Verbose
$ProfileFtp2 | Format-Table