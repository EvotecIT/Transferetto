---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-TransferAzureBlobEndpoint
## SYNOPSIS
Creates an Azure Blob Transferetto endpoint.

Connects through a protected connection string or a container URI, including a container SAS URI. The endpoint performs blob data-plane operations and does not create or administer storage resources.

## SYNTAX
### ConnectionString (Default)
```powershell
New-TransferAzureBlobEndpoint -ConnectionString <securestring> -ContainerName <string> [-Prefix <string>] [<CommonParameters>]
```

### ContainerUri
```powershell
New-TransferAzureBlobEndpoint -ContainerUri <uri> [-Prefix <string>] [<CommonParameters>]
```

### SasToken
```powershell
New-TransferAzureBlobEndpoint -ContainerUri <uri> -SasToken <securestring> [-Prefix <string>] [<CommonParameters>]
```

### SharedKey
```powershell
New-TransferAzureBlobEndpoint -ContainerUri <uri> -SharedKeyCredential <pscredential> [-Prefix <string>] [<CommonParameters>]
```

### DefaultCredential
```powershell
New-TransferAzureBlobEndpoint -ContainerUri <uri> -UseDefaultCredential [-Prefix <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates an Azure Blob Transferetto endpoint.

Connects through a protected connection string or a container URI, including a container SAS URI. The endpoint performs blob data-plane operations and does not create or administer storage resources.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TransferAzureBlobEndpoint -ConnectionString (Read-Host -AsSecureString) -ContainerName 'Name'
```


### EXAMPLE 2
```powershell
New-TransferAzureBlobEndpoint -ContainerUri 'Value'
```


### EXAMPLE 3
```powershell
New-TransferAzureBlobEndpoint -ContainerUri 'Value' -UseDefaultCredential
```


## PARAMETERS

### -ConnectionString
Gets or sets a protected Azure Storage connection string.

```yaml
Type: SecureString
Parameter Sets: ConnectionString
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ContainerName
Gets or sets the blob container name used with a connection string.

```yaml
Type: String
Parameter Sets: ConnectionString
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ContainerUri
Gets or sets a container URI, optionally containing a SAS token.

```yaml
Type: Uri
Parameter Sets: ContainerUri, SasToken, SharedKey, DefaultCredential
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Prefix
Gets or sets the endpoint-relative blob prefix.

```yaml
Type: String
Parameter Sets: ConnectionString, ContainerUri, SasToken, SharedKey, DefaultCredential
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SasToken
Gets or sets a separately protected SAS token.

```yaml
Type: SecureString
Parameter Sets: SasToken
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SharedKeyCredential
Gets or sets an account credential whose username is the account name and password is the shared key.

```yaml
Type: PSCredential
Parameter Sets: SharedKey
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseDefaultCredential
Gets or sets whether the Azure default credential chain is used.

```yaml
Type: SwitchParameter
Parameter Sets: DefaultCredential
Aliases: None
Possible values:

Required: True
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

- `Transferetto.AzureBlob.AzureBlobTransferEndpoint`

## RELATED LINKS

- None
