---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Invoke-SSHShellRecipe
## SYNOPSIS
Runs a reusable Linux administration recipe inside an interactive SSH shell.

Provides a higher-level shell automation layer for common administration flows such as running a sudo command, following a remote file with tail, or following systemd logs with journalctl, while still supporting prompt presets, streaming output, and cancellation.

## SYNTAX
### __AllParameterSets
```powershell
Invoke-SSHShellRecipe -ShellSession <TransferettoSshShellSession> -Recipe <TransferettoSshShellRecipeKind> [-Command <string>] [-Password <string>] [-PasswordPromptPattern <string>] [-RemotePath <string>] [-ServiceName <string>] [-TailLines <int>] [-StopPattern <string>] [-Lookback <int>] [-TimeoutSeconds <double>] [-InterruptTimeoutSeconds <double>] [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-StreamOutput] [-PollIntervalMilliseconds <int>] [-RawOutput] [<CommonParameters>]
```

## DESCRIPTION
Runs a reusable Linux administration recipe inside an interactive SSH shell.

Provides a higher-level shell automation layer for common administration flows such as running a sudo command, following a remote file with tail, or following systemd logs with journalctl, while still supporting prompt presets, streaming output, and cancellation.

## EXAMPLES

### EXAMPLE 1
```powershell
Invoke-SSHShellRecipe -ShellSession 'Value' -Recipe 'Value'
```


## PARAMETERS

### -Command
Gets or sets the command used by the sudo recipe.

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

### -InterruptTimeoutSeconds
Gets or sets the timeout used to interrupt a long-running follow recipe, in seconds.

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
Gets or sets the lookback window used for text and regular expression matching.

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

### -Password
Gets or sets the sudo password used by the sudo recipe.

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

### -PasswordPromptPattern
Gets or sets the sudo password prompt pattern when it must be overridden.

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
Gets or sets the explicit prompt pattern used when returning to the shell prompt.

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
Gets or sets the reusable prompt preset used when PromptPattern is not supplied.

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
Gets or sets a value indicating whether only the raw output text should be written to the pipeline.

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

### -Recipe
Gets or sets the reusable recipe kind.

```yaml
Type: TransferettoSshShellRecipeKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: SudoCommand, FollowFile, FollowJournal

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePath
Gets or sets the remote path used by the file-follow recipe.

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

### -ServiceName
Gets or sets the service name used by the journal-follow recipe.

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

### -StopPattern
Gets or sets the optional stop pattern used while following output.

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

### -StreamOutput
Gets or sets a value indicating whether progressive shell output chunks are written to the pipeline while the recipe runs.

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

### -TailLines
Gets or sets the number of lines shown before follow mode starts.

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

### -TimeoutSeconds
Gets or sets the recipe timeout, in seconds.

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
