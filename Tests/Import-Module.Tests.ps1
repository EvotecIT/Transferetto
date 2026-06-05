Describe 'Transferetto module import' {
    It 'has modern manifest metadata for discovery and help' {
        $ManifestPath = Join-Path $PSScriptRoot '..\Transferetto.psd1'
        $Manifest = Import-PowerShellDataFile -Path $ManifestPath

        $Manifest.HelpInfoURI | Should -Be 'https://github.com/EvotecIT/Transferetto/blob/master/README.md'
        $Manifest.Description | Should -Match 'FTP'
        $Manifest.Description | Should -Match 'SFTP'
        $Manifest.Description | Should -Match 'SCP'
        $Manifest.Description | Should -Match 'FXP'
        $Manifest.Description | Should -Match 'SSH'
        $Manifest.PrivateData.PSData.ProjectUri | Should -Be 'https://github.com/EvotecIT/Transferetto'
        $Manifest.PrivateData.PSData.Tags | Should -Contain 'fxp'
        $Manifest.PrivateData.PSData.Tags | Should -Contain 'scp'
        $Manifest.PrivateData.PSData.Tags | Should -Contain 'ssh'
    }

    It 'exports the binary Connect-FTP cmdlet with advanced connection parameters' {
        $Command = Get-Command -Name Connect-FTP -ErrorAction Stop
        $Command.CommandType | Should -Be 'Cmdlet'

        $ExpectedParameters = @(
            'ConnectTimeout'
            'ReadTimeout'
            'DataConnectionConnectTimeout'
            'DataConnectionReadTimeout'
            'NoopInterval'
            'SslSessionLength'
            'EncryptAuthenticationOnly'
            'SelfConnectMode'
            'RetryAttempts'
            'TransferChunkSize'
            'LocalFileBufferSize'
            'InternetProtocolVersions'
            'UploadRateLimit'
            'DownloadRateLimit'
            'UploadDataType'
            'DownloadDataType'
            'ListingDataType'
            'FXPDataType'
            'FXPProgressInterval'
            'ActivePorts'
            'PassiveBlockedPorts'
            'PassiveMaxAttempts'
            'EncodingName'
            'UseGnuTls'
        )

        foreach ($ParameterName in $ExpectedParameters) {
            $Command.Parameters.Keys | Should -Contain $ParameterName
        }
    }

    It 'exports the FXP preflight cmdlet' {
        $Command = Get-Command -Name Test-FXPTransfer -ErrorAction Stop
        $Command.CommandType | Should -Be 'Cmdlet'
        $Command.Parameters.Keys | Should -Contain 'TransferKind'
        $Command.Parameters.Keys | Should -Contain 'CreateRemoteDirectory'
    }

    It 'exports FTP and SFTP synchronization cmdlets with planning parameters' {
        foreach ($CommandName in @('Sync-FTPDirectory', 'Sync-SFTPDirectory')) {
            $Command = Get-Command -Name $CommandName -ErrorAction Stop
            $Command.CommandType | Should -Be 'Cmdlet'
            $Command.Parameters.Keys | Should -Contain 'Direction'
            $Command.Parameters.Keys | Should -Contain 'Mode'
            $Command.Parameters.Keys | Should -Contain 'Comparison'
            $Command.Parameters.Keys | Should -Contain 'Include'
            $Command.Parameters.Keys | Should -Contain 'Exclude'
            $Command.Parameters.Keys | Should -Contain 'DryRun'
            $Command.Parameters.Keys | Should -Contain 'NoOverwrite'
        }
    }

    It 'exports the SSH shell recipe cmdlet with administration parameters' {
        $Command = Get-Command -Name Invoke-SSHShellRecipe -ErrorAction Stop
        $Command.CommandType | Should -Be 'Cmdlet'
        $Command.Parameters.Keys | Should -Contain 'Recipe'
        $Command.Parameters.Keys | Should -Contain 'Command'
        $Command.Parameters.Keys | Should -Contain 'RemotePath'
        $Command.Parameters.Keys | Should -Contain 'ServiceName'
        $Command.Parameters.Keys | Should -Contain 'Password'
        $Command.Parameters.Keys | Should -Contain 'PromptPreset'
    }

    It 'shows examples for every exported cmdlet' {
        $ExampleCommands = Get-Command -Module Transferetto -CommandType Cmdlet | Sort-Object Name | Select-Object -ExpandProperty Name
        $ExampleCommands.Count | Should -BeGreaterThan 0

        foreach ($CommandName in $ExampleCommands) {
            $Examples = Get-Help -Name $CommandName -Examples | Out-String -Width 200
            $Examples | Should -Match '(?i)Example 1'
        }
    }

    It 'does not leave placeholder XML doc summaries in PowerShell source' {
        $SourceRoot = Join-Path $PSScriptRoot '..\Sources\Transferetto.PowerShell'
        $PlaceholderMatches = Get-ChildItem -Path $SourceRoot -Recurse -Filter '*.cs' |
            Select-String -Pattern 'Implements the .* cmdlet\.'

        @($PlaceholderMatches).Count | Should -Be 0
    }

    It 'rejects conflicting SSH host key trust settings before connecting' {
        {
            Connect-SSH -Server 'ssh.example.com' `
                -Username 'user' `
                -Password 'password' `
                -AcceptAnyHostKey `
                -ExpectedHostKeyFingerprint 'SHA256:abc123=' `
                -ErrorAction Stop
        } | Should -Throw '*AcceptAnyHostKey*'
    }

    It 'rejects multi-file FTP downloads when LocalPath is an existing file' {
        $client = [FluentFTP.FtpClient]::new('ftp.example.com')
        $constructor = [Transferetto.TransferettoFtpSession].GetConstructors([System.Reflection.BindingFlags]'Instance, NonPublic') | Select-Object -First 1
        $session = $constructor.Invoke(@($client, $null, $null))
        $localFile = Join-Path $TestDrive 'existing.txt'
        Set-Content -LiteralPath $localFile -Value 'existing'

        {
            Receive-FTPFile -Client $session -RemotePath @('/pub/example/a.txt', '/pub/example/b.txt') -LocalPath $localFile -ErrorAction Stop
        } | Should -Throw '*directory*'

        $session.Dispose()
    }

    It 'rejects SSH proxy configuration without a proxy port before connecting' {
        {
            Connect-SSH -Server 'ssh.example.com' `
                -Username 'user' `
                -Password 'password' `
                -ProxyType Socks5 `
                -ProxyHost 'proxy.example.com' `
                -ErrorAction Stop
        } | Should -Throw '*ProxyPort*'
    }

    It 'rejects SSH proxy settings when ProxyType is omitted' {
        {
            Connect-SSH -Server 'ssh.example.com' `
                -Username 'user' `
                -Password 'password' `
                -ProxyHost 'proxy.example.com' `
                -ProxyPort 1080 `
                -ErrorAction Stop
        } | Should -Throw '*ProxyType*'
    }
}
