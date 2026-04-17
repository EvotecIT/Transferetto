Import-Module .\Transferetto.psd1 -Force

$ExpectedFingerprint = 'SHA256:REPLACE_WITH_SERVER_FINGERPRINT'
$KnownHostsPath = Join-Path $env:LOCALAPPDATA 'Transferetto\ssh-known-hosts.tsv'

# Strictest option: pin the server fingerprint explicitly.
$SftpClient = Connect-SFTP -Server 'sftp.example.com' `
    -Credential (Get-Credential) `
    -ExpectedHostKeyFingerprint $ExpectedFingerprint `
    -ConnectionTimeoutSeconds 15 `
    -KeepAliveIntervalSeconds 30 `
    -RetryAttempts 2

$SftpClient.HostKeyInfo | Format-List
Get-SFTPWorkingDirectory -SftpClient $SftpClient
Disconnect-SFTP -SftpClient $SftpClient

# Practical default: Trust On First Use and persist the server key locally.
# $SftpClient = Connect-SFTP -Server 'sftp.example.com' `
#     -Credential (Get-Credential) `
#     -HostKeyPolicy TrustOnFirstUse `
#     -KnownHostsPath $KnownHostsPath

# Require the key to already exist in the known-hosts file.
# $SftpClient = Connect-SFTP -Server 'sftp.example.com' `
#     -Credential (Get-Credential) `
#     -HostKeyPolicy KnownHosts `
#     -KnownHostsPath $KnownHostsPath

# Use this only in disposable/test environments where strict validation is not needed.
# $SftpClient = Connect-SFTP -Server 'sftp.example.com' -Credential (Get-Credential) -AcceptAnyHostKey

# Proxy example for bastion or corporate networks:
# $ProxyCredential = Get-Credential
# $SftpClient = Connect-SFTP -Server 'sftp.example.com' `
#     -Credential (Get-Credential) `
#     -ExpectedHostKeyFingerprint $ExpectedFingerprint `
#     -ProxyType Socks5 `
#     -ProxyHost 'proxy.example.com' `
#     -ProxyPort 1080 `
#     -ProxyCredential $ProxyCredential
