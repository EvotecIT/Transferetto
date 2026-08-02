---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Stop-SSHShellCommand
## SYNOPSIS
Stops a running interactive SSH shell command and waits for the prompt to return.

Uses the shell stop lane to interrupt the active command, optionally waiting for a resolved prompt pattern or preset before returning the captured stop result.

## SYNTAX
### __AllParameterSets
```powershell
Stop-SSHShellCommand -ShellSession <TransferettoSshShellSession> [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-Lookback <int>] [-TimeoutSeconds <double>] [<CommonParameters>]
```

## DESCRIPTION
Stops a running interactive SSH shell command and waits for the prompt to return.

Uses the shell stop lane to interrupt the active command, optionally waiting for a resolved prompt pattern or preset before returning the captured stop result.

## EXAMPLES

### EXAMPLE 1
```powershell
Stop-SSHShellCommand -ShellSession 'Value'
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
