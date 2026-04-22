# Remaining PowerShell script assets, if any, are intentionally ignored at runtime.
$Classes = @(Get-ChildItem -Path ([IO.Path]::Combine($PSScriptRoot, 'Classes', '*.ps1')) -ErrorAction SilentlyContinue -Recurse)
$Enums = @(Get-ChildItem -Path ([IO.Path]::Combine($PSScriptRoot, 'Enums', '*.ps1')) -ErrorAction SilentlyContinue -Recurse)

# Get all assemblies
$LibRoot = [IO.Path]::Combine($PSScriptRoot, 'Lib')
$AssemblyFolders = Get-ChildItem -Path $LibRoot -Directory -ErrorAction SilentlyContinue

# Lets find which libraries we need to load
$Default = $false
$Core = $false
$Standard = $false
foreach ($A in $AssemblyFolders.Name) {
    if ($A -eq 'Default') {
        $Default = $true
    } elseif ($A -eq 'Core') {
        $Core = $true
    } elseif ($A -eq 'Standard') {
        $Standard = $true
    }
}
if ($Standard -and $Core -and $Default) {
    $FrameworkNet = 'Default'
    $Framework = 'Standard'
} elseif ($Standard -and $Core) {
    $Framework = 'Standard'
    $FrameworkNet = 'Standard'
} elseif ($Core -and $Default) {
    $Framework = 'Core'
    $FrameworkNet = 'Default'
} elseif ($Standard -and $Default) {
    $Framework = 'Standard'
    $FrameworkNet = 'Default'
} elseif ($Standard) {
    $Framework = 'Standard'
    $FrameworkNet = 'Standard'
} elseif ($Core) {
    $Framework = 'Core'
    $FrameworkNet = ''
} elseif ($Default) {
    $Framework = ''
    $FrameworkNet = 'Default'
} else {
    $Framework = ''
    $FrameworkNet = ''
}
if ($PSEdition -eq 'Core') {
    $LibFolder = $Framework
} else {
    $LibFolder = $FrameworkNet
}

$BinaryModules = @(
    'Transferetto.PowerShell.dll'
)
$IgnoreLibraryFiles = @(
    'libgcc_s_seh-1.dll'
    'libgmp-10.dll'
    'libgnutls-30.dll'
    'libhogweed-6.dll'
    'libnettle-8.dll'
    'libwinpthread-1.dll'
    'Transferetto.PowerShell.dll'
)
$Assembly = @(
    if ($Framework -and $PSEdition -eq 'Core') {
        Get-ChildItem -Path ([IO.Path]::Combine($LibRoot, $Framework, '*.dll')) -ErrorAction SilentlyContinue -Recurse
    }
    if ($FrameworkNet -and $PSEdition -ne 'Core') {
        Get-ChildItem -Path ([IO.Path]::Combine($LibRoot, $FrameworkNet, '*.dll')) -ErrorAction SilentlyContinue -Recurse
    }
)
$Development = $false
$DevelopmentPath = [IO.Path]::Combine($PSScriptRoot, 'Sources', 'Transferetto.PowerShell', 'bin', 'Debug')
$DevelopmentFolderDefault = 'net472'
$DevelopmentFolderCore = 'net8.0'
$BinaryModulePaths = @(
    foreach ($BinaryModule in $BinaryModules) {
        $ModulePath = [IO.Path]::Combine($PSScriptRoot, 'Lib', $LibFolder, $BinaryModule)
        if (Test-Path -LiteralPath $ModulePath) {
            $ModulePath
        } else {
            $Development = $true
            if ($PSEdition -eq 'Core') {
                [IO.Path]::Combine($DevelopmentPath, $DevelopmentFolderCore, $BinaryModule)
            } else {
                [IO.Path]::Combine($DevelopmentPath, $DevelopmentFolderDefault, $BinaryModule)
            }
        }
    }
)
if ($Development) {
    $Assembly = @(
        if ($PSEdition -eq 'Core') {
            Get-ChildItem -Path ([IO.Path]::Combine($DevelopmentPath, $DevelopmentFolderCore, '*.dll')) -ErrorAction SilentlyContinue -Recurse
        } else {
            Get-ChildItem -Path ([IO.Path]::Combine($DevelopmentPath, $DevelopmentFolderDefault, '*.dll')) -ErrorAction SilentlyContinue -Recurse
        }
    )
}

$FoundErrors = @(
    $ImportModule = Get-Command -Name Import-Module -Module Microsoft.PowerShell.Core
    foreach ($BinaryModule in $BinaryModulePaths) {
        try {
            if ($Development) {
                Write-Warning "Development mode: Using binary module from $BinaryModule"
            }
            & $ImportModule $BinaryModule -ErrorAction Stop
        } catch {
            Write-Warning -Message "Importing module $BinaryModule failed. Fix errors before continuing. Error: $($_.Exception.Message)"
            $true
        }
    }

    foreach ($Import in @($Assembly)) {
        if ($IgnoreLibraryFiles -contains $Import.Name) {
            continue
        }
        try {
            Add-Type -Path $Import.Fullname -ErrorAction Stop
        } catch [System.Reflection.ReflectionTypeLoadException] {
            Write-Warning "Processing $($Import.Name) Exception: $($_.Exception.Message)"
            $LoaderExceptions = $($_.Exception.LoaderExceptions) | Sort-Object -Unique
            foreach ($E in $LoaderExceptions) {
                Write-Warning "Processing $($Import.Name) LoaderExceptions: $($E.Message)"
            }
            $true
        } catch {
            Write-Warning "Processing $($Import.Name) Exception: $($_.Exception.Message)"
            $LoaderExceptions = $($_.Exception.LoaderExceptions) | Sort-Object -Unique
            foreach ($E in $LoaderExceptions) {
                Write-Warning "Processing $($Import.Name) LoaderExceptions: $($E.Message)"
            }
            $true
        }
    }

    # Dot source the files
    foreach ($Import in @($Classes + $Enums)) {
        try {
            . $Import.Fullname
        } catch {
            Write-Warning -Message "Failed to import functions from $($import.Fullname).Error: $($_.Exception.Message)"
            $true
        }
    }
)

if ($FoundErrors.Count -gt 0) {
    $ModuleName = (Get-ChildItem $PSScriptRoot\*.psd1).BaseName
    Write-Warning "Importing module $ModuleName failed. Fix errors before continuing."
    break
}

Export-ModuleMember -Function '*' -Alias '*' -Cmdlet '*'
