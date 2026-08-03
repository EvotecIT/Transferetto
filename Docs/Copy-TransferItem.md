---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Copy-TransferItem
## SYNOPSIS
Streams an item between any two Transferetto endpoints.

The content is relayed without loading the full item into memory. The returned receipt contains a provider-independent SHA-256 digest.

## SYNTAX
### __AllParameterSets
```powershell
Copy-TransferItem -SourceEndpoint <ITransferEndpoint> -SourcePath <string> -DestinationEndpoint <ITransferEndpoint> -DestinationPath <string> [-WriteMode <TransferWriteMode>] [-ShowProgress] [<CommonParameters>]
```

## DESCRIPTION
Streams an item between any two Transferetto endpoints.

The content is relayed without loading the full item into memory. The returned receipt contains a provider-independent SHA-256 digest.

## EXAMPLES

### EXAMPLE 1
```powershell
Copy-TransferItem -SourceEndpoint 'Value' -SourcePath 'C:\Path' -DestinationEndpoint 'Value' -DestinationPath 'C:\Path'
```


## PARAMETERS

### -DestinationEndpoint
Gets or sets the destination endpoint.

```yaml
Type: ITransferEndpoint
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DestinationPath
Gets or sets the destination item path.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowProgress
Gets or sets whether transfer progress is displayed.

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

### -SourceEndpoint
Gets or sets the source endpoint.

```yaml
Type: ITransferEndpoint
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SourcePath
Gets or sets the source item path.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WriteMode
Gets or sets destination collision behavior.

```yaml
Type: TransferWriteMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: SkipIfExists, FailIfExists, Overwrite

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

- `Transferetto.Core.TransferReceipt`

## RELATED LINKS

- None
