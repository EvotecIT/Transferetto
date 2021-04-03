Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A'
Get-SFTPList -SftpClient $SftpClient | Format-Table
Get-SFTPList -SftpClient $SftpClient -Path "/Temporary" | Format-Table *

$ListFiles = Get-ChildItem -LiteralPath $PSScriptRoot\Upload
foreach ($File in $ListFiles) {
    Send-SFTPFile -SftpClient $SftpClient -LocalPath $File.FullName -RemotePath "/Temporary/$($File.Name)" -AllowOverride
}

Disconnect-SFTP -SftpClient $SftpClient