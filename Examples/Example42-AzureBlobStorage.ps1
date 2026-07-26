$blob = New-TransferAzureBlobEndpoint `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix 'servers'

$receipt = Send-TransferItem `
    -Endpoint $blob `
    -LocalPath '.\server01.txevidence.json' `
    -Path 'server01/latest.txevidence.json' `
    -ContentType 'application/json'

$receipt
Receive-TransferItem `
    -Endpoint $blob `
    -Path 'server01/latest.txevidence.json' `
    -LocalPath '.\downloaded.txevidence.json'
