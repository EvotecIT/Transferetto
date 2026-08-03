---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Close-TransferEndpoint
## SYNOPSIS
Disposes a Transferetto endpoint and its owned provider client.

## SYNTAX
### __AllParameterSets
```powershell
Close-TransferEndpoint -Endpoint <ITransferEndpoint> [<CommonParameters>]
```

## DESCRIPTION
Disposes a Transferetto endpoint and its owned provider client.

## EXAMPLES

### EXAMPLE 1
```powershell
Close-TransferEndpoint -Endpoint 'Value'
```


## PARAMETERS

### -Endpoint
Gets or sets the endpoint to dispose.

```yaml
Type: ITransferEndpoint
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.Core.ITransferEndpoint`

## OUTPUTS

- `None`

## RELATED LINKS

- None
