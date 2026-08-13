Describe 'Packaged AssemblyLoadContext isolation' {
    It 'loads binary cmdlets and library assemblies from the module ALC without type accelerators' {
        if ($PSVersionTable.PSEdition -ne 'Core') {
            Set-ItResult -Skipped -Because 'module-scoped AssemblyLoadContext is PowerShell Core-only'
            return
        }

        $packagedModuleRoot = Join-Path $PSScriptRoot '..\Artefacts\Unpacked\Modules'
        $packagedModule = Join-Path $packagedModuleRoot 'Transferetto'
        $packagedLoader = Join-Path $packagedModule 'Lib\Core\Transferetto.ModuleLoadContext.dll'
        if (-not (Test-Path -LiteralPath $packagedModule)) {
            Set-ItResult -Skipped -Because 'packaged module artifacts are not created by source-only test runs'
            return
        }
        if (-not (Test-Path -LiteralPath $packagedLoader)) {
            Set-ItResult -Skipped -Because 'packaged module artifacts were created without the ALC loader'
            return
        }

        $moduleRootLiteral = $packagedModuleRoot.Replace("'", "''")
        $script = @"
`$ErrorActionPreference = 'Stop'
`$WarningPreference = 'SilentlyContinue'
`$moduleRoot = '$moduleRootLiteral'
`$env:PSModulePath = `$moduleRoot + [IO.Path]::PathSeparator + `$env:PSModulePath

Import-Module Transferetto -Force

`$command = Get-Command Connect-FTP -Module Transferetto -ErrorAction Stop
`$credential = [pscredential]::new(
    'access-key',
    (ConvertTo-SecureString 'secret-key' -AsPlainText -Force))
`$s3 = New-TransferS3Endpoint -BucketName 'evidence' -ServiceUrl 'http://127.0.0.1:9000' -Credential `$credential -ForcePathStyle
`$commandAssembly = `$command.ImplementingType.Assembly
`$commandAlc = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext(`$commandAssembly)
`$s3Assembly = `$s3.GetType().Assembly
`$s3Alc = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext(`$s3Assembly)
`$transferettoAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { `$_.GetName().Name -eq 'Transferetto.Protocols' } |
    Select-Object -First 1
`$transferettoAlc = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext(`$transferettoAssembly)
`$fluentFtpAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { `$_.GetName().Name -eq 'FluentFTP' } |
    Select-Object -First 1
`$fluentFtpAlc = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext(`$fluentFtpAssembly)
`$acceleratorType = [psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')
`$accelerators = `$acceleratorType.GetProperty('Get', [System.Reflection.BindingFlags]'Static,Public,NonPublic').GetValue(`$null)
`$transferettoAccelerators = @(
    foreach (`$entry in `$accelerators.GetEnumerator()) {
        if (`$entry.Value.Assembly.GetName().Name -like 'Transferetto*') {
            `$entry.Key
        }
    }
)

[pscustomobject]@{
    CommandName = `$command.Name
    CommandAssembly = `$commandAssembly.GetName().Name
    CommandAssemblyPath = `$commandAssembly.Location
    CommandALC = `$commandAlc.Name
    CommandALCIsDefault = [object]::ReferenceEquals(`$commandAlc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
    S3Assembly = `$s3Assembly.GetName().Name
    S3ALC = `$s3Alc.Name
    S3ALCIsDefault = [object]::ReferenceEquals(`$s3Alc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
    TransferettoAssembly = `$transferettoAssembly.GetName().Name
    TransferettoAssemblyPath = `$transferettoAssembly.Location
    TransferettoALC = `$transferettoAlc.Name
    TransferettoALCIsDefault = [object]::ReferenceEquals(`$transferettoAlc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
    FluentFtpAssembly = `$fluentFtpAssembly.GetName().Name
    FluentFtpALC = `$fluentFtpAlc.Name
    FluentFtpALCIsDefault = [object]::ReferenceEquals(`$fluentFtpAlc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
    TransferettoTypeAccelerators = `$transferettoAccelerators
} | ConvertTo-Json -Compress
"@
        $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($script))
        $output = pwsh -NoProfile -ExecutionPolicy Bypass -EncodedCommand $encoded 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($output -join [Environment]::NewLine)

        $json = $output | Where-Object { $_ -is [string] -and $_.TrimStart().StartsWith('{') } | Select-Object -Last 1
        $json | Should -Not -BeNullOrEmpty -Because ($output -join [Environment]::NewLine)
        $result = $json | ConvertFrom-Json

        $result.CommandName | Should -Be 'Connect-FTP'
        $result.CommandAssembly | Should -Be 'Transferetto.PowerShell'
        ($result.CommandAssemblyPath -replace '\\', '/') | Should -BeLike '*/Artefacts/Unpacked/Modules/Transferetto/Lib/Core/Transferetto.PowerShell.dll'
        $result.CommandALC | Should -Be 'Transferetto'
        $result.CommandALCIsDefault | Should -BeFalse
        $result.S3Assembly | Should -Be 'Transferetto.S3'
        $result.S3ALC | Should -Be 'Transferetto'
        $result.S3ALCIsDefault | Should -BeFalse
        $result.TransferettoAssembly | Should -Be 'Transferetto.Protocols'
        ($result.TransferettoAssemblyPath -replace '\\', '/') | Should -BeLike '*/Artefacts/Unpacked/Modules/Transferetto/Lib/Core/Transferetto.Protocols.dll'
        $result.TransferettoALC | Should -Be 'Transferetto'
        $result.TransferettoALCIsDefault | Should -BeFalse
        $result.FluentFtpAssembly | Should -Be 'FluentFTP'
        $result.FluentFtpALC | Should -Be 'Transferetto'
        $result.FluentFtpALCIsDefault | Should -BeFalse
        @($result.TransferettoTypeAccelerators).Count | Should -Be 0
    }
}
