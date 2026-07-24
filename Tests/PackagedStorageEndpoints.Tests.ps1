Describe 'Packaged Transferetto storage endpoints' {
    It 'imports the exact artifact and exposes storage cmdlets in a clean process' {
        $PackagedModuleRoot = Join-Path $PSScriptRoot '..\Artefacts\Unpacked\Modules'
        $PackagedModule = Join-Path $PackagedModuleRoot 'Transferetto\Transferetto.psd1'
        if (-not (Test-Path -LiteralPath $PackagedModule)) {
            Set-ItResult -Skipped -Because 'packaged module artifacts are not created by source-only test runs'
            return
        }

        $ModuleRootLiteral = $PackagedModuleRoot.Replace("'", "''")
        $Script = @"
`$ErrorActionPreference = 'Stop'
`$WarningPreference = 'Stop'
`$env:PSModulePath = '$ModuleRootLiteral' + [IO.Path]::PathSeparator + `$env:PSModulePath
Import-Module Transferetto -Force
`$credential = [pscredential]::new(
    'access-key',
    (ConvertTo-SecureString 'secret-key' -AsPlainText -Force))
`$s3 = Connect-TransferettoS3 -BucketName 'evidence' -ServiceUrl 'http://127.0.0.1:9000' -Credential `$credential -ForcePathStyle
`$sas = ConvertTo-SecureString 'sv=test&sig=secret' -AsPlainText -Force
`$blob = Connect-TransferettoAzureBlob -ContainerUri 'https://account.blob.core.windows.net/evidence' -SasToken `$sas
[pscustomobject]@{
    Commands = @((Get-Command -Module Transferetto -Name '*-Transferetto*').Name | Sort-Object)
    S3Scheme = `$s3.Scheme
    S3DisplayName = `$s3.DisplayName
    BlobScheme = `$blob.Scheme
    BlobDisplayName = `$blob.DisplayName
    ModuleVersion = (Get-Module Transferetto).Version.ToString()
} | ConvertTo-Json -Compress
"@
        $Encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($Script))
        $Executable = if ($PSVersionTable.PSEdition -eq 'Core') { 'pwsh' } else { 'powershell.exe' }
        $Output = & $Executable -NoProfile -ExecutionPolicy Bypass -EncodedCommand $Encoded 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($Output -join [Environment]::NewLine)

        $Json = $Output | Where-Object {
            $_ -is [string] -and $_.TrimStart().StartsWith('{')
        } | Select-Object -Last 1
        $Json | Should -Not -BeNullOrEmpty -Because ($Output -join [Environment]::NewLine)
        $Result = $Json | ConvertFrom-Json

        (@($Result.Commands) -join ',') | Should -Be (@(
            'Connect-TransferettoAzureBlob'
            'Connect-TransferettoS3'
            'Copy-TransferettoItem'
            'Disconnect-TransferettoEndpoint'
            'Get-TransferettoItem'
            'Receive-TransferettoItem'
            'Remove-TransferettoItem'
            'Send-TransferettoItem'
            'Test-TransferettoItem'
        ) -join ',')
        $Result.S3Scheme | Should -Be 's3'
        $Result.S3DisplayName | Should -Be 's3://evidence/'
        $Result.BlobScheme | Should -Be 'azureblob'
        $Result.BlobDisplayName | Should -Be 'https://account.blob.core.windows.net/evidence/'
        $Result.ModuleVersion | Should -Be '2.0.1'
    }
}
