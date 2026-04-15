Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'
$Shell = New-SSHShell -SshClient $SshClient -PromptPattern '(?m)^[^@\r\n]+@[^:\r\n]+:.*[$#]\s?$'

Invoke-SSHShellCommand -ShellSession $Shell -Command 'cd /var/www/html && ls -la'
Invoke-SSHShellCommand -ShellSession $Shell -Command 'sudo systemctl status nginx --no-pager'

Get-SSHShellTranscript -ShellSession $Shell -AsText
Export-SSHShellTranscript -ShellSession $Shell -Path "$PSScriptRoot\Logs\ssh-session.log"
Clear-SSHShellTranscript -ShellSession $Shell -Suppress

Close-SSHShell -ShellSession $Shell
Disconnect-SSH -SshClient $SshClient
