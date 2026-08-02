---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-TransferS3Endpoint
## SYNOPSIS
Creates an Amazon S3 or S3-compatible Transferetto endpoint.

Uses the AWS default credential chain unless an explicit access-key credential is supplied. Custom endpoints support S3-compatible services such as MinIO, Cloudflare R2, and Backblaze B2.

## SYNTAX
### __AllParameterSets
```powershell
New-TransferS3Endpoint [-BucketName] <string> [-Prefix <string>] [-Region <string>] [-ServiceUrl <uri>] [-Credential <pscredential>] [-SessionToken <securestring>] [-ForcePathStyle] [<CommonParameters>]
```

## DESCRIPTION
Creates an Amazon S3 or S3-compatible Transferetto endpoint.

Uses the AWS default credential chain unless an explicit access-key credential is supplied. Custom endpoints support S3-compatible services such as MinIO, Cloudflare R2, and Backblaze B2.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TransferS3Endpoint -BucketName 'Name'
```


## PARAMETERS

### -BucketName
Gets or sets the bucket name.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Credential
Gets or sets an access-key credential whose username is the access key identifier.

```yaml
Type: PSCredential
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ForcePathStyle
Gets or sets whether path-style bucket addressing is required.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Prefix
Gets or sets the endpoint-relative key prefix.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Region
Gets or sets the AWS or signing region.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ServiceUrl
Gets or sets a custom S3-compatible service URL.

```yaml
Type: Uri
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SessionToken
Gets or sets a session token for temporary credentials.

```yaml
Type: SecureString
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `Transferetto.S3.S3TransferEndpoint`

## RELATED LINKS

- None
