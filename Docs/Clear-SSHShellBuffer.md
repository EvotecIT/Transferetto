---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Clear-SSHShellBuffer
## SYNOPSIS
Clears buffered unread output from an interactive SSH shell session.

Returns the buffered text by default so callers can inspect or discard it explicitly, or suppress output when they simply want a clean shell buffer before continuing an automation flow.

## SYNTAX
### __AllParameterSets
```powershell
Clear-SSHShellBuffer -ShellSession <TransferettoSshShellSession> [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Clears buffered unread output from an interactive SSH shell session.

Returns the buffered text by default so callers can inspect or discard it explicitly, or suppress output when they simply want a clean shell buffer before continuing an automation flow.

## EXAMPLES

### EXAMPLE 1
```powershell
Clear-SSHShellBuffer -ShellSession 'Value'
```


## PARAMETERS

### -ShellSession
Gets or sets the shell Session.

```yaml
Type: TransferettoSshShellSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Suppress
Gets or sets the suppress.

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

- `Transferetto.TransferettoSshShellSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
