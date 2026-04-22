Import-Module .\Transferetto.psd1 -Force

$ScpClient = Connect-SCP -Server '192.168.241.29' -Username 'przemek' -Password 'YourPassword'
Receive-SCPFile -ScpClient $ScpClient -RemotePath '/home/przemek/test1.txt' -LocalPath "$PSScriptRoot\Downloads\scp-test1.txt"
Send-SCPFile -ScpClient $ScpClient -LocalPath "$PSScriptRoot\Downloads\scp-test1.txt" -RemotePath '/home/przemek/scp-test1.txt'
Send-SCPDirectory -ScpClient $ScpClient -LocalPath "$PSScriptRoot\Downloads" -RemotePath '/home/przemek/scp-downloads'
Receive-SCPDirectory -ScpClient $ScpClient -RemotePath '/home/przemek/scp-downloads' -LocalPath "$PSScriptRoot\Downloads-SCP"
Disconnect-SCP -ScpClient $ScpClient
