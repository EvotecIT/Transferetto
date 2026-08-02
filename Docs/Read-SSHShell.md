---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Read-SSHShell
## SYNOPSIS
Reads output from an interactive SSH shell session.

Supports simple reads, line reads, read-until-idle, text and regex expectations, prompt waits, follow-mode output capture, prompt presets, progressive streaming, and cancellation-aware polling for interactive shell automation.

## SYNTAX
### __AllParameterSets
```powershell
Read-SSHShell -ShellSession <TransferettoSshShellSession> [-ReadLine] [-ExpectText <string>] [-RegexPattern <string>] [-Lookback <int>] [-TimeoutSeconds <double>] [-ReadUntilIdle] [-IdleTimeoutSeconds <double>] [-ExpectPrompt] [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-Follow] [-StreamOutput] [-PollIntervalMilliseconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Reads output from an interactive SSH shell session.

Supports simple reads, line reads, read-until-idle, text and regex expectations, prompt waits, follow-mode output capture, prompt presets, progressive streaming, and cancellation-aware polling for interactive shell automation.

## EXAMPLES

### EXAMPLE 1
```powershell
Read-SSHShell -ShellSession 'Value'
```


## PARAMETERS

### -ExpectPrompt
Gets or sets the expect Prompt.

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

### -ExpectText
Gets or sets the expect Text.

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

### -Follow
Gets or sets a value indicating whether output should be followed until cancellation, timeout, or an optional stop pattern.

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

### -IdleTimeoutSeconds
Gets or sets the idle Timeout Seconds.

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

### -ReadLine
Gets or sets the read Line.

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

### -ReadUntilIdle
Gets or sets the read Until Idle.

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

### -RegexPattern
Gets or sets the regex Pattern.

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
Gets or sets a value indicating whether progressive output chunks are written to the pipeline while waiting.

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
