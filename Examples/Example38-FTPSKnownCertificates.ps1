Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential
$KnownCertificatesPath = Join-Path $env:LOCALAPPDATA 'Transferetto\ftps-known-certificates.tsv'

# First connection with TrustOnFirstUse stores the presented certificate for this host and port.
# Future connections can switch to KnownCertificates to reject unexpected certificate changes.
$FtpClient = Connect-FTP -Server 'ftps.example.com' `
    -Credential $Credential `
    -EncryptionMode Explicit `
    -CertificatePolicy TrustOnFirstUse `
    -KnownCertificatesPath $KnownCertificatesPath

$FtpClient.CertificateInfo | Format-List Subject, Issuer, ThumbprintSHA1, ThumbprintSHA256, CanTrust, TrustSource, KnownCertificatesPath, WasPersisted

Disconnect-FTP -Client $FtpClient

$PinnedClient = Connect-FTP -Server 'ftps.example.com' `
    -Credential $Credential `
    -EncryptionMode Explicit `
    -CertificatePolicy KnownCertificates `
    -KnownCertificatesPath $KnownCertificatesPath

Get-FTPList -Client $PinnedClient -Path '/'

Disconnect-FTP -Client $PinnedClient
