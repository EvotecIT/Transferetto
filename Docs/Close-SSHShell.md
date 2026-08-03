---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Close-SSHShell
## SYNOPSIS
Closes an interactive SSH shell session.

Stops the shell stream created by New-SSHShell while leaving the parent SSH connection available for other commands or for creating a replacement shell later.

## SYNTAX
### __AllParameterSets
```powershell
Close-SSHShell -ShellSession <TransferettoSshShellSession> [<CommonParameters>]
```

## DESCRIPTION
Closes an interactive SSH shell session.

Stops the shell stream created by New-SSHShell while leaving the parent SSH connection available for other commands or for creating a replacement shell later.

## EXAMPLES

### EXAMPLE 1
```powershell
Close-SSHShell -ShellSession 'Value'
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSshShellSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
