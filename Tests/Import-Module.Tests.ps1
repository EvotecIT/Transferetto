Describe 'Transferetto module import' {
    It 'exports the binary Connect-FTP cmdlet with advanced connection parameters' {
        $Command = Get-Command -Name Connect-FTP -ErrorAction Stop
        $Command.CommandType | Should -Be 'Cmdlet'

        $ExpectedParameters = @(
            'ConnectTimeout'
            'ReadTimeout'
            'DataConnectionConnectTimeout'
            'DataConnectionReadTimeout'
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
}
