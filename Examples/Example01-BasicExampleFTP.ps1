Import-Module .\Transferetto.psd1 -Force

# Login via UserName/Password
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -EncryptionMode Auto
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client


# Login via UserName/Password
$Client = Connect-FTP -Server 'ftp.dlptest.com' -Verbose -Username 'dlpuser' -Password 'rNrKYTX9g7z3RgJRmxWuGHbeu'
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client

# Anonymous login
$Client = Connect-FTP -Server 'speedtest.tele2.net' -Verbose
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client

# Login via credentials
$Credential = Get-Credential -UserName 'demo' -Message 'Please enter password' # password is password
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Credential $Credential
$List = Get-FTPList -Client $Client
$List | Format-Table
Disconnect-FTP -Client $Client