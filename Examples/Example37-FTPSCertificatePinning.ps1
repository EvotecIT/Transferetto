Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential

# Use the server certificate SHA1 or SHA256 thumbprint from a trusted source.
# Separators and casing do not matter, for example:
#   SHA256:AA:BB:CC:DD...
#   aa bb cc dd ...
$ExpectedThumbprint = 'SHA256:AA:BB:CC:DD:EE:FF'

$FtpClient = Connect-FTP -Server 'ftps.example.com' `
    -Credential $Credential `
    -EncryptionMode Explicit `
    -CertificatePolicy PolicyChain `
    -ExpectedCertificateThumbprint $ExpectedThumbprint

$FtpClient.CertificateInfo | Format-List Subject, Issuer, ThumbprintSHA1, ThumbprintSHA256, CanTrust, TrustSource, PolicyErrors

Get-FTPItem -Client $FtpClient -RemotePath '/'

Disconnect-FTP -Client $FtpClient
