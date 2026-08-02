---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Disconnect-SCP
## SYNOPSIS
Disconnects an SCP session.

Closes the reusable SCP session created by Connect-SCP so the underlying SSH transport is released cleanly after copy operations complete.

## SYNTAX
### __AllParameterSets
```powershell
Disconnect-SCP -ScpClient <TransferettoScpSession> [<CommonParameters>]
```

## DESCRIPTION
Disconnects an SCP session.

Closes the reusable SCP session created by Connect-SCP so the underlying SSH transport is released cleanly after copy operations complete.

## EXAMPLES

### EXAMPLE 1
```powershell
Disconnect-SCP -ScpClient 'Value'
```


## PARAMETERS

### -ScpClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoScpSession
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

- `Transferetto.TransferettoScpSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
