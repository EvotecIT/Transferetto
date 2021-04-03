Import-Module .\Transferetto.psd1 -Force

$SshClient = Connect-SSH -Server '192.168.240.29' -Username 'przemek' -Password 'sdsd'

$Command = {
    'cd /'
    'ls -la'
    'cd /etc'
    'ls -la'
}
$SshCommand = Send-SSHCommand -SSHClient $SshClient -Command $Command -Verbose
$SshCommand


$Command = {
    'cat /etc/hosts.allow'
}
$SshCommand = Send-SSHCommand -SSHClient $SshClient -Command $Command -Verbose
$SshCommand