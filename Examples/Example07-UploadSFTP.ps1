Import-Module .\Transferetto.psd1 -Force

$SftpClient = Connect-SFTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A'
Get-SFTPList -SftpClient $SftpClient | Format-Table
Get-SFTPList -SftpClient $SftpClient -Path "/Temporary" | Format-Table *

$ListFiles = Get-ChildItem -LiteralPath $PSScriptRoot\Upload -Recurse -File
foreach ($File in $ListFiles) {
    $Directory = [io.path]::GetDirectoryName($File.FullName)
    if ($Directory -eq "$PSScriptRoot\Upload") {
        Send-SFTPFile -SftpClient $SftpClient -LocalPath $File.FullName -RemotePath "/Temporary/$($File.Name)" -AllowOverride
    } else {
        #$RemotePath = "/Temporary/$($Directory.Split('\')[-1])/$($File.Name)"
        $RemoteFolder = "/Temporary/$($Directory.Split('\')[-1])"
        $List = Get-SFTPList -SftpClient $SftpClient -Path $RemoteFolder -WarningAction SilentlyContinue
        if (-not $List) {
            $SftpClient.CreateDirectory($RemoteFolder)
        }
        Send-SFTPFile -SftpClient $SftpClient -LocalPath $File.FullName -RemotePath "$RemoteFolder/$($File.Name)" -AllowOverride
    }
}

Disconnect-SFTP -SftpClient $SftpClient