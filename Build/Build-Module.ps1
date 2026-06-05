Build-Module -ModuleName 'Transferetto' {
    $Manifest = [ordered] @{
        ModuleVersion        = '1.0.X'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'Transferetto is a PowerShell module and reusable .NET library for FTP, FTPS, SFTP, SCP, FXP, SSH commands, SSH shells, and SSH tunnels.'
        Tags                 = @('Windows', 'MacOS', 'Linux', 'ftp', 'ftps', 'sftp', 'scp', 'fxp', 'ssh')
        IconUri              = 'https://evotec.xyz/wp-content/uploads/2021/03/Transferetto.png'
        ProjectUri           = 'https://github.com/EvotecIT/Transferetto'
        PowerShellVersion    = '5.1'
    }
    New-ConfigurationManifest @Manifest

    $ConfigurationFormat = [ordered] @{
        RemoveComments                              = $false
        PlaceOpenBraceEnable                        = $true
        PlaceOpenBraceOnSameLine                    = $true
        PlaceOpenBraceNewLineAfter                  = $true
        PlaceOpenBraceIgnoreOneLineBlock            = $false
        PlaceCloseBraceEnable                       = $true
        PlaceCloseBraceNewLineAfter                 = $false
        PlaceCloseBraceIgnoreOneLineBlock           = $false
        PlaceCloseBraceNoEmptyLineBefore            = $true
        UseConsistentIndentationEnable              = $true
        UseConsistentIndentationKind                = 'space'
        UseConsistentIndentationPipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
        UseConsistentIndentationIndentationSize     = 4
        UseConsistentWhitespaceEnable               = $true
        UseConsistentWhitespaceCheckInnerBrace      = $true
        UseConsistentWhitespaceCheckOpenBrace       = $true
        UseConsistentWhitespaceCheckOpenParen       = $true
        UseConsistentWhitespaceCheckOperator        = $true
        UseConsistentWhitespaceCheckPipe            = $true
        UseConsistentWhitespaceCheckSeparator       = $true
        AlignAssignmentStatementEnable              = $true
        AlignAssignmentStatementCheckHashtable      = $true
        UseCorrectCasingEnable                      = $true
    }
    New-ConfigurationFormat -ApplyTo 'OnMergePSM1', 'OnMergePSD1' -Sort None @ConfigurationFormat
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'DefaultPSM1' -EnableFormatting -Sort None
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'OnMergePSD1' -PSD1Style 'Minimal'

    New-ConfigurationDocumentation -Enable:$false -PathReadme 'Docs\Readme.md' -Path 'Docs'
    $ImportSelf = if ([string]::IsNullOrWhiteSpace($Env:ImportSelf)) { $true } else { [bool]::Parse($Env:ImportSelf) }
    New-ConfigurationImportModule -ImportSelf:$ImportSelf -ImportRequiredModules

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = if ([string]::IsNullOrWhiteSpace($Env:SignModule)) { $true } else { [bool]::Parse($Env:SignModule) }
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        CertificateThumbprint             = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
        NETProjectPath                    = "$PSScriptRoot\..\Sources\Transferetto.PowerShell"
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'Transferetto.PowerShell'
        NETProjectName                    = 'Transferetto.PowerShell'
        NETBinaryModule                   = 'Transferetto.PowerShell.dll'
        NETConfiguration                  = 'Release'
        NETFramework                      = 'net8.0', 'net472'
        NETHandleAssemblyWithSameName     = $true
        NETAssemblyLoadContext            = $true
        NETAssemblyTypeAcceleratorMode    = 'None'
        NETIgnoreLibraryOnLoad            = @(
            'libgcc_s_seh-1.dll'
            'libgmp-10.dll'
            'libgnutls-30.dll'
            'libhogweed-6.dll'
            'libnettle-8.dll'
            'libwinpthread-1.dll'
        )
        DotSourceLibraries                = $true
        NETSearchClass                    = 'Transferetto.PowerShell.CmdletConnectFtp'
        NETBinaryModuleDocumentation      = $true
        DeleteTargetModuleBeforeBuild     = $true
        RefreshPSD1Only                   = if ([string]::IsNullOrWhiteSpace($Env:RefreshPSD1Only)) { $true } else { [bool]::Parse($Env:RefreshPSD1Only) }
    }
    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot\..\Artefacts\Unpacked" -RequiredModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot\..\Artefacts\Packed" -IncludeTagName -ArtefactName "Transferetto-PowerShellModule.<TagModuleVersionWithPreRelease>.zip" -ID 'ToGitHub'
}
