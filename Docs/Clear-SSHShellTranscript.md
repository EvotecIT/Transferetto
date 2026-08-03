---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Clear-SSHShellTranscript
## SYNOPSIS
Clears the in-memory transcript stored for an interactive SSH shell session.

Returns the removed transcript snapshot by default so callers can archive it before clearing, or suppress output when they simply want to reset transcript state before the next automation step.

## SYNTAX
### __AllParameterSets
```powershell
Clear-SSHShellTranscript -ShellSession <TransferettoSshShellSession> [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Clears the in-memory transcript stored for an interactive SSH shell session.

Returns the removed transcript snapshot by default so callers can archive it before clearing, or suppress output when they simply want to reset transcript state before the next automation step.

## EXAMPLES

### EXAMPLE 1
```powershell
Clear-SSHShellTranscript -ShellSession 'Value'
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
