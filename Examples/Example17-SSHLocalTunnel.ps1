Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server 'server.example.com' -Username 'przemek' -Password 'YourPassword'

# Forward local port 15432 to PostgreSQL on the remote server.
$Tunnel = Start-SSHLocalTunnel -SshClient $SshClient -BoundHost '127.0.0.1' -BoundPort 15432 -RemoteHost '127.0.0.1' -RemotePort 5432

$Tunnel | Format-List

# Use 127.0.0.1:15432 locally while the tunnel is open.
Read-Host 'Press Enter to stop the tunnel'

Stop-SSHTunnel -TunnelSession $Tunnel
Disconnect-SSH -SshClient $SshClient
