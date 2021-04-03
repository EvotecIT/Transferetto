Import-Module .\Transferetto.psd1 -Force

# If you want to track responses from FTP
Set-FTPTracing -Enable -DisplayConsole

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
# List files
Get-FTPList -Client $Client -Path '/Temporary' | Format-Table *
Get-FTPChmod -Client $Client -RemotePath '/Temporary'

# Set / Read Chmod - you need to have permissions for this to work properly
Set-FTPChmod -Client $Client -RemotePath '/Temporary/OrgChart (1).pdf' -Permissions 666
Set-FTPChmod -Client $Client -RemotePath '/Temporary/CreateDir' -Permissions 666

# Set / Read Chmod - you need to have permissions for this to work properly
Get-FTPChmod -Client $Client -RemotePath '/Temporary/OrgChart (1).pdf'
Get-FTPChmod -Client $Client -RemotePath '/Temporary/CreateDir'

Set-FTPTracing -Disable