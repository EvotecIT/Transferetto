$s3 = New-TransferS3Endpoint `
    -BucketName 'evidence' `
    -Region 'eu-central-1' `
    -Prefix 'incoming'

$blob = New-TransferAzureBlobEndpoint `
    -ContainerUri 'https://account.blob.core.windows.net/evidence' `
    -UseDefaultCredential `
    -Prefix 'archive'

Copy-TransferItem `
    -SourceEndpoint $s3 `
    -SourcePath 'server01/latest.txevidence.json' `
    -DestinationEndpoint $blob `
    -DestinationPath 'server01/latest.txevidence.json' `
    -WriteMode FailIfExists `
    -ShowProgress
