---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-SSHShellTranscript
## SYNOPSIS
Retrieves the in-memory transcript captured for an interactive SSH shell session.

Returns either a structured transcript snapshot or plain text entries, with optional trimming to the newest entries so long-running interactive sessions can be inspected without exporting everything.

## SYNTAX
### __AllParameterSets
```powershell
Get-SSHShellTranscript -ShellSession <TransferettoSshShellSession> [-Last <int>] [-AsText] [<CommonParameters>]
```

## DESCRIPTION
Retrieves the in-memory transcript captured for an interactive SSH shell session.

Returns either a structured transcript snapshot or plain text entries, with optional trimming to the newest entries so long-running interactive sessions can be inspected without exporting everything.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SSHShellTranscript -ShellSession 'Value'
```


## PARAMETERS

### -AsText
Gets or sets the as Text.

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

### -Last
Gets or sets the last.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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
