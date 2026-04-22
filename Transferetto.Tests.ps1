$ModuleName = (Get-ChildItem $PSScriptRoot\*.psd1).BaseName
$PrimaryModule = Get-ChildItem -Path $PSScriptRoot -Filter '*.psd1' -Recurse -ErrorAction SilentlyContinue -Depth 1
if (-not $PrimaryModule) {
    throw "Path $PSScriptRoot doesn't contain PSD1 files. Failing tests."
}
if ($PrimaryModule.Count -ne 1) {
    throw 'More than one PSD1 files detected. Failing tests.'
}
$PSDInformation = Import-PowerShellDataFile -Path $PrimaryModule.FullName
$RequiredModules = @(
    'Pester'
    'PSWriteColor'
    if ($PSDInformation.RequiredModules) {
        $PSDInformation.RequiredModules
    }
)
foreach ($Module in $RequiredModules) {
    if ($Module -is [System.Collections.IDictionary]) {
        $Exists = Get-Module -ListAvailable -Name $Module.ModuleName
        if (-not $Exists) {
            Write-Warning "$ModuleName - Downloading $($Module.ModuleName) from PSGallery"
            Install-Module -Name $Module.ModuleName -Force -SkipPublisherCheck -AllowClobber
        }
    } else {
        $Exists = Get-Module -ListAvailable $Module -ErrorAction SilentlyContinue
        if (-not $Exists) {
            Install-Module -Name $Module -Force -SkipPublisherCheck -AllowClobber
        }
    }
}

Write-Color 'ModuleName: ', $ModuleName, ' Version: ', $PSDInformation.ModuleVersion -Color Yellow, Green, Yellow, Green -LinesBefore 2
Write-Color 'PowerShell Version: ', $PSVersionTable.PSVersion -Color Yellow, Green
Write-Color 'PowerShell Edition: ', $PSVersionTable.PSEdition -Color Yellow, Green
Write-Color 'Required modules: ' -Color Yellow
foreach ($Module in $PSDInformation.RequiredModules) {
    if ($Module -is [System.Collections.IDictionary]) {
        Write-Color '   [>] ', $Module.ModuleName, ' Version: ', $Module.ModuleVersion -Color Yellow, Green, Yellow, Green
    } else {
        Write-Color '   [>] ', $Module -Color Yellow, Green
    }
}
Write-Color

Import-Module ([IO.Path]::Combine($PSScriptRoot, '*.psd1')) -Force

$PesterCommand = Get-Command -Name Invoke-Pester
$InvokePesterParameters = @{
    Verbose = $true
    PassThru = $true
}
if ($PesterCommand.Parameters.ContainsKey('Path')) {
    $InvokePesterParameters.Path = "$PSScriptRoot\Tests"
} else {
    $InvokePesterParameters.Script = "$PSScriptRoot\Tests"
}
$ExcludedTags = [System.Collections.Generic.List[string]]::new()
if ($env:TRANSFERETTO_RUN_LIVE_FTP_TESTS -ne '1') {
    Write-Color 'Skipping public live FTP tests. Set TRANSFERETTO_RUN_LIVE_FTP_TESTS=1 to include them.' -Color Yellow
    $null = $ExcludedTags.Add('LiveFTP')
}
if ($env:TRANSFERETTO_RUN_LIVE_SSH_TESTS -ne '1') {
    Write-Color 'Skipping public live SSH/SFTP/SCP tests. Set TRANSFERETTO_RUN_LIVE_SSH_TESTS=1 to include them.' -Color Yellow
    $null = $ExcludedTags.Add('LiveSSH')
}
if ($env:TRANSFERETTO_RUN_LOCAL_FTPS_TESTS -ne '1') {
    Write-Color 'Skipping local FTPS tests. Set TRANSFERETTO_RUN_LOCAL_FTPS_TESTS=1 to include them.' -Color Yellow
    $null = $ExcludedTags.Add('LocalFTPS')
}
if ($ExcludedTags.Count -gt 0) {
    if ($PesterCommand.Parameters.ContainsKey('ExcludeTagFilter')) {
        $InvokePesterParameters.ExcludeTagFilter = $ExcludedTags.ToArray()
    } elseif ($PesterCommand.Parameters.ContainsKey('ExcludeTag')) {
        $InvokePesterParameters.ExcludeTag = $ExcludedTags.ToArray()
    }
}

$result = Invoke-Pester @InvokePesterParameters

if ($result.FailedCount -gt 0) {
    throw "$($result.FailedCount) tests failed."
}
