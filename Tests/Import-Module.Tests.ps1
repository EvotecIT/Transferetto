Describe 'Transferetto module import' {
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
