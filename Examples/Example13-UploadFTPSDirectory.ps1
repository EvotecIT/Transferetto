Import-Module .\Transferetto.psd1 -Force

Set-FTPTracing -Enable

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
# List files
$List = Get-FTPList -Client $Client
#$List | Format-Table
# List files within Temporary directory
$List = Get-FTPList -Client $Client -Path '/Temporary'
#$List | Format-Table

# Get local files
$ListFiles = Get-ChildItem -LiteralPath $PSScriptRoot\Upload

$Upload = Send-FTPDirectory -Client $Client -LocalPath $PSScriptRoot\Upload -RemotePath '/Temporary' -Verbose -FolderSyncMode Update
$Upload | Format-Table *

Disconnect-FTP -Client $Client

Set-FTPTracing -Disable