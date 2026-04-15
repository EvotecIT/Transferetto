@{
    AliasesToExport        = @('Get-FTPDirectory', 'Get-FTPFile', 'Get-SCPDirectory', 'Get-SCPFile', 'Get-SFTPDirectory', 'Get-SFTPFile', 'Add-FTPDirectory', 'Add-FTPFile', 'Add-SCPDirectory', 'Add-SCPFile', 'Add-SFTPDirectory', 'Add-SFTPFile', 'Start-FXPDirectory', 'Start-FXPFile', 'Receive-SSHShell', 'Send-SSHShell', 'Resize-SSHShell')
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @('Clear-SSHShellBuffer', 'Clear-SSHShellTranscript', 'Close-FTPStream', 'Close-SFTPStream', 'Close-SSHShell', 'Compare-FTPFile', 'Connect-FTP', 'Connect-SCP', 'Connect-SFTP', 'Connect-SSH', 'Disconnect-FTP', 'Disconnect-SCP', 'Disconnect-SFTP', 'Disconnect-SSH', 'Export-SSHShellTranscript', 'Get-FTPChecksum', 'Get-FTPChmod', 'Get-FTPFileSize', 'Get-FTPItem', 'Get-FTPList', 'Get-FTPModifiedTime', 'Get-FTPWorkingDirectory', 'Get-SFTPChmod', 'Get-SFTPContent', 'Get-SFTPItem', 'Get-SFTPList', 'Get-SFTPWorkingDirectory', 'Get-SSHShellTranscript', 'Invoke-SSHShellCommand', 'Move-FTPDirectory', 'Move-FTPFile', 'Move-SFTPDirectory', 'Move-SFTPFile', 'New-FTPDirectory', 'New-SFTPDirectory', 'New-SFTPSymbolicLink', 'New-SSHShell', 'Open-FTPStream', 'Open-SFTPStream', 'Read-FTPStream', 'Read-SFTPStream', 'Read-SSHShell', 'Receive-FTPDirectory', 'Receive-FTPFile', 'Receive-SCPDirectory', 'Receive-SCPFile', 'Receive-SFTPDirectory', 'Receive-SFTPFile', 'Remove-FTPDirectory', 'Remove-FTPFile', 'Remove-SFTPDirectory', 'Remove-SFTPFile', 'Rename-FTPFile', 'Rename-SFTPFile', 'Request-FTPConfiguration', 'Send-FTPDirectory', 'Send-FTPFile', 'Send-SCPDirectory', 'Send-SCPFile', 'Send-SFTPDirectory', 'Send-SFTPFile', 'Send-SSHCommand', 'Send-SSHShellControl', 'Set-FTPChmod', 'Set-FTPModifiedTime', 'Set-FTPOption', 'Set-FTPStreamPosition', 'Set-FTPTracing', 'Set-FTPWorkingDirectory', 'Set-SFTPChmod', 'Set-SFTPContent', 'Set-SFTPStreamPosition', 'Set-SFTPTimestamp', 'Set-SFTPWorkingDirectory', 'Set-SSHShellPrompt', 'Set-SSHShellSize', 'Start-FXPDirectoryTransfer', 'Start-FXPFileTransfer', 'Start-SSHLocalTunnel', 'Start-SSHRemoteTunnel', 'Stop-SSHShellCommand', 'Stop-SSHTunnel', 'Sync-FTPStream', 'Sync-SFTPStream', 'Test-FTPDirectory', 'Test-FTPFile', 'Test-SFTPDirectory', 'Test-SFTPFile', 'Test-SFTPPath', 'Test-SFTPSymbolicLink', 'Wait-SSHShellPrompt', 'Write-FTPStream', 'Write-SFTPStream', 'Write-SSHShell')
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2024 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'Module which allows ftp, ftps, sftp file transfers with advanced features. It also allows to transfer files and directorires between servers using fxp protocol. As a side feature it allows to conenct to SSH and executes commands on it. '
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = @()
    GUID                   = '7d61db15-9efe-41d1-a1c0-81d738975dec'
    ModuleVersion          = '1.0.0'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            ExternalModuleDependencies = @('Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility')
            IconUri                    = 'https://evotec.xyz/wp-content/uploads/2021/03/Transferetto.png'
            ProjectUri                 = 'https://github.com/EvotecIT/Transferetto'
            Tags                       = @('Windows', 'Linux', 'MacOs', 'ftp', 'sftp', 'ftps', 'scp', 'winscp', 'ssh')
        }
    }
    RequiredModules        = @('Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility')
    RootModule             = 'Transferetto.psm1'
}
