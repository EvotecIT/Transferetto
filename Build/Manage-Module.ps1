Clear-Host

Import-Module 'C:\Support\GitHub\PSPublishModule\PSPublishModule.psd1' -Force

Build-Module -ModuleName 'Transferetto' {
    # Usual defaults as per standard module
    $Manifest = [ordered] @{
        # Version number of this module.
        ModuleVersion          = '0.0.X'
        # Supported PSEditions
        CompatiblePSEditions   = @('Desktop', 'Core')
        # ID used to uniquely identify this module
        GUID                   = '7d61db15-9efe-41d1-a1c0-81d738975dec'
        # Author of this module
        Author                 = 'Przemyslaw Klys'
        # Company or vendor of this module
        CompanyName            = 'Evotec'
        # Copyright statement for this module
        Copyright              = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        # Description of the functionality provided by this module
        Description            = 'Module which allows ftp, ftps, sftp file transfers with advanced features. It also allows to transfer files and directorires between servers using fxp protocol. As a side feature it allows to conenct to SSH and executes commands on it. '
        # Minimum version of the Windows PowerShell engine required by this module
        PowerShellVersion      = '5.1'
        # Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
        # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
        Tags                   = @('Windows', 'Linux', 'MacOs', 'ftp', 'sftp', 'ftps', 'scp', 'winscp', 'ssh')

        IconUri                = 'https://evotec.xyz/wp-content/uploads/2021/03/Transferetto.png'

        ProjectUri             = 'https://github.com/EvotecIT/Transferetto'

       # DotNetFrameworkVersion = '4.7.2'
    }
    New-ConfigurationManifest @Manifest
    # Add external module dependencies, using loop for simplicity
    New-ConfigurationModule -Type ExternalModule -Name 'Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility'

    # Add approved modules, that can be used as a dependency, but only when specific function from those modules is used
    # And on that time only that function and dependant functions will be copied over
    # Keep in mind it has it's limits when "copying" functions such as it should not depend on DLLs or other external files
    New-ConfigurationModule -Type ApprovedModule -Name 'PSSharedGoods', 'PSWriteColor', 'Connectimo', 'PSUnifi', 'PSWebToolbox', 'PSMyPassword'

    $ConfigurationFormat = [ordered] @{
        RemoveComments                              = $false

        PlaceOpenBraceEnable                        = $true
        PlaceOpenBraceOnSameLine                    = $true
        PlaceOpenBraceNewLineAfter                  = $true
        PlaceOpenBraceIgnoreOneLineBlock            = $false

        PlaceCloseBraceEnable                       = $true
        PlaceCloseBraceNewLineAfter                 = $true
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
    # format PSD1 and PSM1 files when merging into a single file
    # enable formatting is not required as Configuration is provided
    New-ConfigurationFormat -ApplyTo 'OnMergePSM1', 'OnMergePSD1' -Sort None @ConfigurationFormat
    # format PSD1 and PSM1 files within the module
    # enable formatting is required to make sure that formatting is applied (with default settings)
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'DefaultPSM1' -EnableFormatting -Sort None
    # when creating PSD1 use special style without comments and with only required parameters
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'OnMergePSD1' -PSD1Style 'Minimal'

    # configuration for documentation, at the same time it enables documentation processing
    New-ConfigurationDocumentation -Enable:$false -StartClean -UpdateWhenNew -PathReadme 'Docs\Readme.md' -Path 'Docs'

    New-ConfigurationImportModule -ImportSelf #-ImportRequiredModules

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $true
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        CertificateThumbprint             = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
        # require for FluentFTP to work in PS 5.1 in VSCode, works fine outside
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'Transferetto'
        NETProjectName                    = 'Transferetto'
        NETConfiguration                  = 'Release'
        NETFramework                      = 'netstandard2.0'
        #NETExcludeMainLibrary             = $true
        DotSourceLibraries                = $true
        DotSourceClasses                  = $true
        #SeparateFileLibraries             = $true
        DeleteTargetModuleBeforeBuild     = $true
    }

    New-ConfigurationBuild @newConfigurationBuildSplat  #-DotSourceLibraries -DotSourceClasses -MergeModuleOnBuild -Enable -SignModule -DeleteTargetModuleBeforeBuild -CertificateThumbprint '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703' -MergeFunctionsFromApprovedModules

    $newConfigurationArtefactSplat = @{
        Type                = 'Unpacked'
        Enable              = $true
        Path                = "$PSScriptRoot\..\Artefacts\Unpacked"
        ModulesPath         = "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
        RequiredModulesPath = "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
        AddRequiredModules  = $true
        CopyFiles           = @{
            #"Examples\PublishingExample\Example-ExchangeEssentials.ps1" = "RunMe.ps1"
        }
    }
    New-ConfigurationArtefact @newConfigurationArtefactSplat -CopyFilesRelative
    $newConfigurationArtefactSplat = @{
        Type                = 'Packed'
        Enable              = $true
        Path                = "$PSScriptRoot\..\Artefacts\Packed"
        ModulesPath         = "$PSScriptRoot\..\Artefacts\Packed\Modules"
        RequiredModulesPath = "$PSScriptRoot\..\Artefacts\Packed\Modules"
        AddRequiredModules  = $true
        CopyFiles           = @{
            #"Examples\PublishingExample\Example-ExchangeEssentials.ps1" = "RunMe.ps1"
        }
        ArtefactName        = '<ModuleName>.v<ModuleVersion>.zip'
    }
    New-ConfigurationArtefact @newConfigurationArtefactSplat

    #New-ConfigurationTest -TestsPath "$PSScriptRoot\..\Tests" -Enable

    # global options for publishing to github/psgallery
    #New-ConfigurationPublish -Type PowerShellGallery -FilePath 'C:\Support\Important\PowerShellGalleryAPI.txt' -Enabled:$true
    #New-ConfigurationPublish -Type GitHub -FilePath 'C:\Support\Important\GitHubAPI.txt' -UserName 'EvotecIT' -Enabled:$true
} -ExitCode