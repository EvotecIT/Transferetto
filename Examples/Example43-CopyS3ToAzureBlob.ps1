$s3 = Connect-TransferettoS3 `
    -BucketName 'evidence' `
    -Region 'eu-central-1' `
    -Prefix 'incoming'

$blob = Connect-TransferettoAzureBlob `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix 'archive'

Copy-TransferettoItem `
    -SourceEndpoint $s3 `
    -SourcePath 'server01/latest.txevidence.json' `
    -DestinationEndpoint $blob `
    -DestinationPath 'server01/latest.txevidence.json' `
    -WriteMode FailIfExists `
    -ShowProgress
