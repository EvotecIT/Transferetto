---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Send-TransferItem
## SYNOPSIS
Uploads a local file to any writable Transferetto endpoint.

## SYNTAX
### __AllParameterSets
```powershell
Send-TransferItem [-Endpoint] <ITransferEndpoint> [-LocalPath] <string> [[-Path] <string>] [-WriteMode <TransferWriteMode>] [-ContentType <string>] [-Metadata <hashtable>] [-ShowProgress] [<CommonParameters>]
```

## DESCRIPTION
Uploads a local file to any writable Transferetto endpoint.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-TransferItem -Path 'C:\Path'
```


## PARAMETERS

### -ContentType
Gets or sets the destination content type.

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

### -Endpoint
Gets or sets the destination endpoint.

```yaml
Type: ITransferEndpoint
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalPath
Gets or sets the local source file.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Metadata
Gets or sets provider metadata.

```yaml
Type: Hashtable
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Gets or sets the destination item path. The source filename is used by default.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 2
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
