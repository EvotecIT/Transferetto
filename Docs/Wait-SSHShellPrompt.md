---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Wait-SSHShellPrompt
## SYNOPSIS
Waits until an expected interactive SSH shell prompt is observed.

Supports explicit prompt regexes or reusable prompt presets, progressive streaming while waiting, and cancellation-aware polling so shell automation can synchronize reliably before the next interactive step.

## SYNTAX
### __AllParameterSets
```powershell
Wait-SSHShellPrompt -ShellSession <TransferettoSshShellSession> [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-Lookback <int>] [-TimeoutSeconds <double>] [-StreamOutput] [-PollIntervalMilliseconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Waits until an expected interactive SSH shell prompt is observed.

Supports explicit prompt regexes or reusable prompt presets, progressive streaming while waiting, and cancellation-aware polling so shell automation can synchronize reliably before the next interactive step.

## EXAMPLES

### EXAMPLE 1
```powershell
Wait-SSHShellPrompt -ShellSession 'Value'
```


## PARAMETERS

### -Lookback
Gets or sets the lookback.

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

### -PromptPattern
Gets or sets the prompt Pattern.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PromptPreset
Gets or sets the reusable prompt preset applied when no explicit prompt pattern is supplied.

```yaml
Type: TransferettoSshShellPromptPreset
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Linux, LinuxUser, LinuxRoot, PowerShell, Cmd

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

### -StreamOutput
Gets or sets a value indicating whether progressive output chunks are written to the pipeline while waiting for the prompt.

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

### -TimeoutSeconds
Gets or sets the timeout Seconds.

```yaml
Type: Double
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
