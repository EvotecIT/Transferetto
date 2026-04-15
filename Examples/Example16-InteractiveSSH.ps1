Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server '192.168.240.29' -Username 'przemek' -Password 'YourPassword'

$Shell = New-SSHShell -SshClient $SshClient -TerminalName 'xterm-256color' -Columns 120 -Rows 40

# Read the initial login banner or prompt.
Read-SSHShell -ShellSession $Shell -TimeoutSeconds 2

# Run a command inside the interactive shell and wait for the prompt to return.
Write-SSHShell -ShellSession $Shell -Text 'pwd'
Read-SSHShell -ShellSession $Shell -ExpectText '$' -TimeoutSeconds 5

# Resize the pseudo-terminal if you need more room.
Set-SSHShellSize -ShellSession $Shell -Columns 160 -Rows 50

Close-SSHShell -ShellSession $Shell
Disconnect-SSH -SshClient $SshClient
