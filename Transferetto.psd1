@{
    AliasesToExport      = @()
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @()
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2021 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'Module with transfer functionality'
    FunctionsToExport    = @('Add-FTPItem', 'Compare-FTPItem', 'Connect-FTP', 'Disconnect-FTP', 'Get-FTPItem', 'Get-FTPList', 'Remove-FTPItem', 'Rename-FTPItem', 'Set-FTPTracing', 'Test-FTPItem')
    GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
    ModuleVersion        = '0.0.1'
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