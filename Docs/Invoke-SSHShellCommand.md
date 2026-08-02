---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Invoke-SSHShellCommand
## SYNOPSIS
Runs a command inside an interactive SSH shell and captures output plus exit code.

Uses the reusable shell marker and prompt-handling lane to execute a command in a live shell session, optionally stream output while it runs, trim command echo, and return either structured results or raw shell output.

## SYNTAX
### __AllParameterSets
```powershell
Invoke-SSHShellCommand -ShellSession <TransferettoSshShellSession> -Command <string> [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-Lookback <int>] [-TimeoutSeconds <double>] [-RawOutput] [-KeepCommandEcho] [-StreamOutput] [-PollIntervalMilliseconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Runs a command inside an interactive SSH shell and captures output plus exit code.

Uses the reusable shell marker and prompt-handling lane to execute a command in a live shell session, optionally stream output while it runs, trim command echo, and return either structured results or raw shell output.

## EXAMPLES

### EXAMPLE 1
```powershell
Invoke-SSHShellCommand -ShellSession 'Value' -Command 'Value'
```


## PARAMETERS

### -Command
Gets or sets the command.

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

### -KeepCommandEcho
Gets or sets the keep Command Echo.

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

### -RawOutput
Gets or sets the raw Output.

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

### -StreamOutput
Gets or sets a value indicating whether progressive shell output chunks are written to the pipeline while the command runs.

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

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
