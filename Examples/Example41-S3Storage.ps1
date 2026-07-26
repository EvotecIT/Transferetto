$s3 = New-TransferS3Endpoint `
    -BucketName 'evidence' `
    -Region 'eu-central-1' `
    -Prefix 'servers'

$receipt = Send-TransferItem `
    -Endpoint $s3 `
    -LocalPath '.\server01.txevidence.json' `
    -Path 'server01/latest.txevidence.json' `
    -ContentType 'application/json' `
    -Metadata @{ schema = 'testimo_evidence_v3' }

$receipt
Get-TransferChildItem -Endpoint $s3 -Path 'server01/' -Recurse

Close-TransferEndpoint -Endpoint $s3
