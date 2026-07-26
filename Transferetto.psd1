@{
    AliasesToExport      = @('Add-FTPDirectory', 'Add-FTPFile', 'Add-SCPDirectory', 'Add-SCPFile', 'Add-SFTPDirectory', 'Add-SFTPFile', 'Get-FTPDirectory', 'Get-FTPFile', 'Get-SCPDirectory', 'Get-SCPFile', 'Get-SFTPDirectory', 'Get-SFTPFile', 'Receive-SSHShell', 'Resize-SSHShell', 'Send-SSHShell', 'Start-FXPDirectory', 'Start-FXPFile')
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @('Clear-SSHShellBuffer', 'Clear-SSHShellTranscript', 'Close-FTPStream', 'Close-SFTPStream', 'Close-SSHShell', 'Close-TransferEndpoint', 'Compare-FTPFile', 'Connect-FTP', 'Connect-SCP', 'Connect-SFTP', 'Connect-SSH', 'Copy-TransferItem', 'Disconnect-FTP', 'Disconnect-SCP', 'Disconnect-SFTP', 'Disconnect-SSH', 'Export-SSHShellTranscript', 'Get-FTPChecksum', 'Get-FTPChmod', 'Get-FTPFileSize', 'Get-FTPItem', 'Get-FTPList', 'Get-FTPModifiedTime', 'Get-FTPWorkingDirectory', 'Get-SFTPChmod', 'Get-SFTPContent', 'Get-SFTPItem', 'Get-SFTPList', 'Get-SFTPWorkingDirectory', 'Get-SSHShellTranscript', 'Get-TransferChildItem', 'Get-TransferItem', 'Invoke-SSHShellCommand', 'Invoke-SSHShellExpect', 'Invoke-SSHShellRecipe', 'Move-FTPDirectory', 'Move-FTPFile', 'Move-SFTPDirectory', 'Move-SFTPFile', 'New-FTPDirectory', 'New-SFTPDirectory', 'New-SFTPSymbolicLink', 'New-SSHShell', 'New-TransferAzureBlobEndpoint', 'New-TransferS3Endpoint', 'Open-FTPStream', 'Open-SFTPStream', 'Read-FTPStream', 'Read-SFTPStream', 'Read-SSHShell', 'Receive-FTPDirectory', 'Receive-FTPFile', 'Receive-SCPDirectory', 'Receive-SCPFile', 'Receive-SFTPDirectory', 'Receive-SFTPFile', 'Receive-TransferItem', 'Remove-FTPDirectory', 'Remove-FTPFile', 'Remove-SFTPDirectory', 'Remove-SFTPFile', 'Remove-TransferItem', 'Rename-FTPFile', 'Rename-SFTPFile', 'Request-FTPConfiguration', 'Send-FTPDirectory', 'Send-FTPFile', 'Send-SCPDirectory', 'Send-SCPFile', 'Send-SFTPDirectory', 'Send-SFTPFile', 'Send-SSHCommand', 'Send-SSHShellControl', 'Send-TransferItem', 'Set-FTPChmod', 'Set-FTPModifiedTime', 'Set-FTPOption', 'Set-FTPStreamPosition', 'Set-FTPTracing', 'Set-FTPWorkingDirectory', 'Set-SFTPChmod', 'Set-SFTPContent', 'Set-SFTPStreamPosition', 'Set-SFTPTimestamp', 'Set-SFTPWorkingDirectory', 'Set-SSHShellPrompt', 'Set-SSHShellSize', 'Start-FXPDirectoryTransfer', 'Start-FXPFileTransfer', 'Start-SSHLocalTunnel', 'Start-SSHRemoteTunnel', 'Stop-SSHShellCommand', 'Stop-SSHTunnel', 'Sync-FTPDirectory', 'Sync-FTPStream', 'Sync-SFTPDirectory', 'Sync-SFTPStream', 'Test-FTPDirectory', 'Test-FTPFile', 'Test-FXPTransfer', 'Test-SFTPDirectory', 'Test-SFTPFile', 'Test-SFTPPath', 'Test-SFTPSymbolicLink', 'Test-TransferItem', 'Wait-SSHShellPrompt', 'Write-FTPStream', 'Write-SFTPStream', 'Write-SSHShell')
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Transferetto provides reusable .NET and PowerShell data transfer across filesystems, FTP, FTPS, SFTP, SCP, FXP, Amazon S3 and S3-compatible storage, Azure Blob Storage, and SSH operations.'
    FunctionsToExport    = @()
    GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
    HelpInfoURI          = 'https://github.com/EvotecIT/Transferetto/blob/master/README.md'
    ModuleVersion        = '2.0.1'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            ExternalModuleDependencies = @()
            IconUri                    = 'https://evotec.xyz/wp-content/uploads/2021/03/Transferetto.png'
            ProjectUri                 = 'https://github.com/EvotecIT/Transferetto'
            Tags                       = @('Windows', 'MacOS', 'Linux', 'transfer', 'storage', 'ftp', 'ftps', 'sftp', 'scp', 'fxp', 'ssh', 's3', 'azure', 'blob')
            RequireLicenseAcceptance   = $false
        }
    }
    RequiredModules      = @()
    RootModule           = 'Transferetto.psm1'
    ScriptsToProcess     = @()
}
