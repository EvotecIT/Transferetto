---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SSHShellPrompt
## SYNOPSIS
Updates the prompt detection settings for an interactive SSH shell session.

Configures either an explicit prompt regex or a reusable prompt preset so later read, expect, and command cmdlets can synchronize against the correct shell prompt.

## SYNTAX
### __AllParameterSets
```powershell
Set-SSHShellPrompt -ShellSession <TransferettoSshShellSession> [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Updates the prompt detection settings for an interactive SSH shell session.

Configures either an explicit prompt regex or a reusable prompt preset so later read, expect, and command cmdlets can synchronize against the correct shell prompt.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SSHShellPrompt -ShellSession 'Value'
```


## PARAMETERS

### -PassThru
Gets or sets the pass Thru.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
