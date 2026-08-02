---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Send-SSHCommand
## SYNOPSIS
Runs one or more non-interactive SSH commands and captures their output.

Supports multi-line command blocks, structured status results, progressive stdout and stderr streaming, and per-command timeouts on top of the reusable SSH command execution layer.

## SYNTAX
### __AllParameterSets
```powershell
Send-SSHCommand -SshClient <TransferettoSshSession> [-Command <scriptblock>] [-Status] [-StreamOutput] [-CommandTimeoutSeconds <int>] [<CommonParameters>]
```

## DESCRIPTION
Runs one or more non-interactive SSH commands and captures their output.

Supports multi-line command blocks, structured status results, progressive stdout and stderr streaming, and per-command timeouts on top of the reusable SSH command execution layer.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-SSHCommand -SshClient 'Value'
```


## PARAMETERS

### -Command
Gets or sets the command.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CommandTimeoutSeconds
Gets or sets the timeout, in seconds, applied to the remote command.

```yaml
Type: Nullable`1
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Gets or sets the status.

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

### -StreamOutput
Gets or sets a value indicating whether progressive command output chunks are written to the pipeline.

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
