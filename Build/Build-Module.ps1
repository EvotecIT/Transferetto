param(
    [ValidateSet('Manifest', 'Documentation', 'Build', 'Publish')]
    [string] $ConfigurationGateMode = 'Build',

    [bool] $SignModule = $true,

    [string] $ProjectBuildConfigPath = 'Build\project.build.json',

    [string] $PowerShellGalleryApiKeyPath = 'C:\Support\Important\PowerShellGalleryAPI.txt',

    [string] $GitHubApiKeyPath = 'C:\Support\Important\GitHubAPI.txt'
)

Import-Module PSPublishModule -Force -ErrorAction Stop

Build-Module -ModuleName 'Transferetto' {
    $Manifest = [ordered] @{
        ModuleVersion        = '2.0.X'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = '7d61db15-9efe-41d1-a1c0-81d738975dec'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'Transferetto provides reusable .NET and PowerShell data transfer across filesystems, FTP, FTPS, SFTP, SCP, FXP, Amazon S3 and S3-compatible storage, Azure Blob Storage, and SSH operations.'
        Tags                 = @('Windows', 'MacOS', 'Linux', 'transfer', 'storage', 'ftp', 'ftps', 'sftp', 'scp', 'fxp', 'ssh', 's3', 'azure', 'blob')
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

    New-ConfigurationDocumentation -Enable -PathReadme 'Docs\Readme.md' -Path 'Docs' -SyncExternalHelpToProjectRoot
    New-ConfigurationImportModule -ImportSelf -ImportRequiredModules

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $SignModule
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        CertificateThumbprint             = '92e95fb58effa6a4a75e77a33cdd6bfe6dd30f1a'
        NETProjectPath                    = 'Sources\Transferetto.PowerShell\Transferetto.PowerShell.csproj'
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
    }
    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationProjectBuild -Name 'Transferetto' -ConfigPath $ProjectBuildConfigPath -Enabled -BuildBeforeModule -UseAsReleaseVersionSource -ProvideLocalNuGetFeed -PublishNuget
    New-ConfigurationRelease -StageRoot 'Artefacts\UploadReady' -VersionSource ProjectBuild -PrimaryProject 'Transferetto' -SynchronizeModuleVersion -PublishOrder 'NuGet', 'PowerShellGallery', 'GitHub'

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot\..\Artefacts\Unpacked" -ModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot\..\Artefacts\Packed" -ModulesPath "$PSScriptRoot\..\Artefacts\Packed\Modules" -IncludeTagName -ArtefactName 'Transferetto-PowerShellModule.<TagModuleVersionWithPreRelease>.zip' -ID 'ToGitHub'

    New-ConfigurationPublish -Type PowerShellGallery -FilePath $PowerShellGalleryApiKeyPath -Enabled:$false
    New-ConfigurationPublish -Type GitHub -FilePath $GitHubApiKeyPath -UserName 'EvotecIT' -RepositoryName 'Transferetto' -Enabled:$false -GenerateReleaseNotes -OverwriteTagName '{ModuleName}-v{ModuleVersionWithPreRelease}'

    New-ConfigurationGate -Mode $ConfigurationGateMode
} -ExitCode
