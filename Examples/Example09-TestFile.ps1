Import-Module .\Transferetto.psd1 -Force

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
# List files
$List = Get-FTPList -Client $Client
$List | Format-Table
# List files within Temporary directory
$List = Get-FTPList -Client $Client -Path '/Temporary'
$List | Format-Table


Test-FTPFile -Client $Client -RemotePath '/Temporar'