---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Export-SSHShellTranscript
## SYNOPSIS
Exports the interactive SSH shell transcript to a file.

Writes the shell transcript to disk for troubleshooting or audit purposes, with optional append mode and support for exporting only the newest entries from a long-running shell session.

## SYNTAX
### __AllParameterSets
```powershell
Export-SSHShellTranscript -ShellSession <TransferettoSshShellSession> -Path <string> [-Last <int>] [-Append] [<CommonParameters>]
```

## DESCRIPTION
Exports the interactive SSH shell transcript to a file.

Writes the shell transcript to disk for troubleshooting or audit purposes, with optional append mode and support for exporting only the newest entries from a long-running shell session.

## EXAMPLES

### EXAMPLE 1
```powershell
Export-SSHShellTranscript -ShellSession 'Value' -Path 'C:\Path'
```


## PARAMETERS

### -Append
Gets or sets the append.

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

### -Path
Gets or sets the path.

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
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
