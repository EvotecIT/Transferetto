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
    ModuleVersion        = '0.0.1'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            Tags       = @('Windows', 'Linux', 'MacOs')
            ProjectUri = 'https://github.com/EvotecIT/Transferetto'
        }
    }
    RootModule           = 'Transferetto.psm1'
}