@{
    AliasesToExport      = @()
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @()
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2021 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Module with transfer functionality'
    FunctionsToExport    = @('Add-FTPFile', 'Add-SFTPFile', 'Connect-FTP', 'Connect-SFTP', 'Disconnect-FTP', 'Disconnect-SFTP', 'Get-FTPFile', 'Get-FTPList', 'Get-SFTPFile', 'Get-SFTPList', 'Remove-FTPFile', 'Remove-SFTPFile', 'Rename-FTPFile', 'Rename-SFTPFile', 'Request-FTPConfiguration', 'Set-FTPTracing', 'Test-FTPFile')
    GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
    ModuleVersion        = '0.0.2'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            Tags       = @('Windows', 'Linux', 'MacOs')
            ProjectUri = 'https://github.com/EvotecIT/Transferetto'
        }
    }
    RequiredModules      = @(@{
            ModuleVersion = '0.0.198'
            ModuleName    = 'PSSharedGoods'
            Guid          = 'ee272aa8-baaa-4edf-9f45-b6d6f7d844fe'
        })
    RootModule           = 'Transferetto.psm1'
}