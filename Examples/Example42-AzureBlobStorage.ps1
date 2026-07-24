$blob = Connect-TransferettoAzureBlob `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix 'servers'

$receipt = Send-TransferettoItem `
    -Endpoint $blob `
    -LocalPath '.\server01.txevidence.json' `
    -Path 'server01/latest.txevidence.json' `
    -ContentType 'application/json'

$receipt
Receive-TransferettoItem `
    -Endpoint $blob `
    -Path 'server01/latest.txevidence.json' `
    -LocalPath '.\downloaded.txevidence.json'
