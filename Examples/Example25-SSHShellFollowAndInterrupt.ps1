Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'
$Shell = New-SSHShell -SshClient $SshClient -PromptPattern '(?m)^[^@\r\n]+@[^:\r\n]+:.*[$#]\s?$'

Clear-SSHShellBuffer -ShellSession $Shell -Suppress
Write-SSHShell -ShellSession $Shell -Text 'journalctl -fu nginx.service'

1..3 | ForEach-Object {
    Read-SSHShell -ShellSession $Shell -ReadUntilIdle -IdleTimeoutSeconds 0.5 -TimeoutSeconds 5
}

Stop-SSHShellCommand -ShellSession $Shell -TimeoutSeconds 5
Close-SSHShell -ShellSession $Shell
Disconnect-SSH -SshClient $SshClient
