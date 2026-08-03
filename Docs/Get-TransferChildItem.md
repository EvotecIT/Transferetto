---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-TransferChildItem
## SYNOPSIS
Lists items beneath a path on any Transferetto endpoint.

## SYNTAX
### __AllParameterSets
```powershell
Get-TransferChildItem [-Endpoint] <ITransferEndpoint> [[-Path] <string>] [-Recurse] [<CommonParameters>]
```

## DESCRIPTION
Lists items beneath a path on any Transferetto endpoint.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-TransferChildItem -Path 'C:\Path'
```


## PARAMETERS

### -Endpoint
Gets or sets the source endpoint.

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

### -Path
Gets or sets the listing prefix. The endpoint root is used by default.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Recurse
Gets or sets whether listing descends recursively.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `Transferetto.Core.TransferItem`

## RELATED LINKS

- None
