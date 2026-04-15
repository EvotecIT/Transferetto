Import-Module .\Transferetto.psd1 -Force

$ExpectedFingerprint = 'SHA256:REPLACE_WITH_SERVER_FINGERPRINT'
$KnownHostsPath = Join-Path $env:LOCALAPPDATA 'Transferetto\ssh-known-hosts.tsv'

# Strictest option: pin the server fingerprint explicitly.
$SshClient = Connect-SSH -Server 'server.example.com' `
    -Credential (Get-Credential) `
    -ExpectedHostKeyFingerprint $ExpectedFingerprint `
    -ConnectionTimeoutSeconds 15 `
    -KeepAliveIntervalSeconds 30 `
    -RetryAttempts 2

$SshClient.HostKeyInfo | Format-List

# Practical default: Trust On First Use and persist the server key locally.
# $SshClient = Connect-SSH -Server 'server.example.com' `
#     -Credential (Get-Credential) `
#     -HostKeyPolicy TrustOnFirstUse `
#     -KnownHostsPath $KnownHostsPath

# Require the key to already exist in the known-hosts file.
# $SshClient = Connect-SSH -Server 'server.example.com' `
#     -Credential (Get-Credential) `
#     -HostKeyPolicy KnownHosts `
#     -KnownHostsPath $KnownHostsPath

# Use this only in disposable/test environments where strict validation is not needed.
# $SshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential) -AcceptAnyHostKey

# Proxy example for bastion or corporate networks:
# $ProxyCredential = Get-Credential
# $SshClient = Connect-SSH -Server 'server.example.com' `
#     -Credential (Get-Credential) `
#     -ExpectedHostKeyFingerprint $ExpectedFingerprint `
#     -ProxyType Socks5 `
#     -ProxyHost 'proxy.example.com' `
#     -ProxyPort 1080 `
#     -ProxyCredential $ProxyCredential

Disconnect-SSH -SshClient $SshClient
