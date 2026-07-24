$s3 = Connect-TransferettoS3 `
    -BucketName 'evidence' `
    -Region 'eu-central-1' `
    -Prefix 'servers'

$receipt = Send-TransferettoItem `
    -Endpoint $s3 `
    -LocalPath '.\server01.txevidence.json' `
    -Path 'server01/latest.txevidence.json' `
    -ContentType 'application/json' `
    -Metadata @{ schema = 'testimo_evidence_v3' }

$receipt
Get-TransferettoItem -Endpoint $s3 -Path 'server01/' -List -Recurse

Disconnect-TransferettoEndpoint -Endpoint $s3
