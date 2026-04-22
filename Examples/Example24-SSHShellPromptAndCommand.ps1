Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server 'server.example.com' -Credential (Get-Credential)

# Linux / POSIX shell prompt example.
$Shell = New-SSHShell -SshClient $SshClient -PromptPattern '(?m)^[^@\r\n]+@[^:\r\n]+:[^\r\n]*[$#]\s?$'

Wait-SSHShellPrompt -ShellSession $Shell -TimeoutSeconds 5 | Out-Null

$Result = Invoke-SSHShellCommand -ShellSession $Shell -Command 'pwd && whoami'
$Result | Format-List

Set-SSHShellPrompt -ShellSession $Shell -PromptPattern '(?m)^[>#]\s?$' -PassThru | Out-Null
Read-SSHShell -ShellSession $Shell -ReadUntilIdle -IdleTimeoutSeconds 1

Close-SSHShell -ShellSession $Shell
Disconnect-SSH -SshClient $SshClient
