---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Invoke-SSHShellExpect
## SYNOPSIS
Executes an ordered expect-style workflow against an interactive SSH shell.

Supports send-text, control-key, prompt, text, regex, line, idle, and follow-mode steps through reusable step objects or PSCustomObject input, making it possible to script interactive shell flows without dropping into raw shell polling logic.

## SYNTAX
### __AllParameterSets
```powershell
Invoke-SSHShellExpect -ShellSession <TransferettoSshShellSession> -Step <psobject[]> [-StreamOutput] [-PollIntervalMilliseconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Executes an ordered expect-style workflow against an interactive SSH shell.

Supports send-text, control-key, prompt, text, regex, line, idle, and follow-mode steps through reusable step objects or PSCustomObject input, making it possible to script interactive shell flows without dropping into raw shell polling logic.

## EXAMPLES

### EXAMPLE 1
```powershell
Invoke-SSHShellExpect -ShellSession 'Value' -Step @('Value')
```


## PARAMETERS

### -PollIntervalMilliseconds
Gets or sets the poll interval, in milliseconds, used while waiting for shell output.

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
Gets or sets the shell session.

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

### -Step
Gets or sets the ordered expect steps.

```yaml
Type: PSObject[]
Parameter Sets: __AllParameterSets
Aliases: Steps
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StreamOutput
Gets or sets a value indicating whether progressive shell output chunks are written to the pipeline while steps execute.

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

- `None`

## RELATED LINKS

- None
