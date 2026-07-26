Describe 'Transferetto storage endpoints' {
    It 'creates S3-compatible endpoints without exposing credentials' {
        $Credential = [pscredential]::new(
            'access-key',
            (ConvertTo-SecureString 'secret-key' -AsPlainText -Force))

        $Endpoint = New-TransferS3Endpoint `
            -BucketName 'evidence' `
            -ServiceUrl 'http://127.0.0.1:9000' `
            -Credential $Credential `
            -ForcePathStyle

        $Endpoint.Scheme | Should -Be 's3'
        $Endpoint.DisplayName | Should -Be 's3://evidence/'
        $Endpoint.DisplayName | Should -Not -Match 'access-key|secret-key'
        Close-TransferEndpoint -Endpoint $Endpoint
    }

    It 'creates Azure Blob endpoints without exposing a SAS token' {
        $ContainerUri = [uri]'https://account.blob.core.windows.net/evidence'
        $SasToken = ConvertTo-SecureString 'sv=test&sig=secret' -AsPlainText -Force

        $Endpoint = New-TransferAzureBlobEndpoint `
            -ContainerUri $ContainerUri `
            -SasToken $SasToken

        $Endpoint.Scheme | Should -Be 'azureblob'
        $Endpoint.DisplayName | Should -Be 'https://account.blob.core.windows.net/evidence/'
        $Endpoint.DisplayName | Should -Not -Match 'sig=|secret'
    }

    It 'offers connection string, SAS, shared key, embedded URI, and default credential parameter sets' {
        $ParameterSets = (Get-Command New-TransferAzureBlobEndpoint).ParameterSets.Name

        $ParameterSets | Should -Contain 'ConnectionString'
        $ParameterSets | Should -Contain 'ContainerUri'
        $ParameterSets | Should -Contain 'SasToken'
        $ParameterSets | Should -Contain 'SharedKey'
        $ParameterSets | Should -Contain 'DefaultCredential'
    }

    It 'separates item inspection from child-item listing' {
        $GetItem = Get-Command Get-TransferItem
        $GetChildren = Get-Command Get-TransferChildItem

        ($GetItem.ParameterSets[0].Parameters | Where-Object Name -EQ 'Path').IsMandatory |
            Should -BeTrue
        $GetItem.Parameters.Keys | Should -Not -Contain 'List'
        ($GetChildren.ParameterSets[0].Parameters | Where-Object Name -EQ 'Path').IsMandatory |
            Should -BeFalse
        $GetChildren.Parameters.Keys | Should -Contain 'Recurse'
    }

    It 'rejects metadata names that cannot cross providers' {
        if ($PSVersionTable.PSEdition -eq 'Core') {
            Set-ItResult -Skipped -Because 'module implementation types are intentionally isolated in the PowerShell Core AssemblyLoadContext'
            return
        }

        { [Transferetto.Core.TransferMetadata]::ValidateName('evidence-id') } |
            Should -Throw '*letters, digits, and underscores*'
        { [Transferetto.Core.TransferMetadata]::ValidateName('evidence_id') } |
            Should -Not -Throw
    }
}
