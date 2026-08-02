---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Disconnect-SSH
## SYNOPSIS
Disconnects an SSH session.

Closes the reusable SSH session created by Connect-SSH, ending command, shell, and tunnel activity tied to that session when shutdown is complete.

## SYNTAX
### __AllParameterSets
```powershell
Disconnect-SSH -SshClient <TransferettoSshSession> [<CommonParameters>]
```

## DESCRIPTION
Disconnects an SSH session.

Closes the reusable SSH session created by Connect-SSH, ending command, shell, and tunnel activity tied to that session when shutdown is complete.

## EXAMPLES

### EXAMPLE 1
```powershell
Disconnect-SSH -SshClient 'Value'
```


## PARAMETERS

### -SshClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoSshSession
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

- `Transferetto.TransferettoSshSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
