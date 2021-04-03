Import-Module .\Transferetto.psd1 -Force

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
# List files
Test-FTPFile -Client $Client -RemotePath '/Temporary'