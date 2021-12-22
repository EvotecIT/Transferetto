@{
    AliasesToExport      = @('Get-FTPDirectory', 'Get-FTPFile', 'Get-SFTPFile', 'Add-FTPDirectory', 'Add-FTPFile', 'Add-SFTPFile', 'Start-FXPDirectory', 'Start-FXPFile')
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @()
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2021 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Module which allows ftp, ftps, sftp file transfers with advanced features. It also allows to transfer files and directorires between servers using fxp protocol. As a side feature it allows to conenct to SSH and executes commands on it. '
    FunctionsToExport    = @('Compare-FTPFile', 'Connect-FTP', 'Connect-SFTP', 'Connect-SSH', 'Disconnect-FTP', 'Disconnect-SFTP', 'Get-FTPChecksum', 'Get-FTPChmod', 'Get-FTPList', 'Get-SFTPList', 'Move-FTPDirectory', 'Move-FTPFile', 'Receive-FTPDirectory', 'Receive-FTPFile', 'Receive-SFTPFile', 'Remove-FTPDirectory', 'Remove-FTPFile', 'Remove-SFTPFile', 'Rename-FTPFile', 'Rename-SFTPFile', 'Request-FTPConfiguration', 'Send-FTPDirectory', 'Send-FTPFile', 'Send-SFTPFile', 'Send-SSHCommand', 'Set-FTPChmod', 'Set-FTPOption', 'Set-FTPTracing', 'Start-FXPDirectoryTransfer', 'Start-FXPFileTransfer', 'Test-FTPDirectory', 'Test-FTPFile')
    GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
    ModuleVersion        = '0.0.10'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            Tags       = @('Windows', 'Linux', 'MacOs', 'ftp', 'sftp', 'ftps', 'scp', 'winscp', 'ssh')
            ProjectUri = 'https://github.com/EvotecIT/Transferetto'
            IconUri    = 'https://evotec.xyz/wp-content/uploads/2021/03/Transferetto.png'
        }
    }
    RequiredModules      = @(@{
            ModuleVersion = '0.0.215'
            ModuleName    = 'PSSharedGoods'
            Guid          = 'ee272aa8-baaa-4edf-9f45-b6d6f7d844fe'
        })
    RootModule           = 'Transferetto.psm1'
}