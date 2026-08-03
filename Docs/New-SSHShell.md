---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-SSHShell
## SYNOPSIS
Creates a reusable interactive SSH shell session.

Builds an interactive shell stream on top of an SSH session, with optional prompt presets or explicit prompt patterns, transcript support, and terminal sizing that can be reused by shell read, write, expect, and recipe cmdlets.

## SYNTAX
### __AllParameterSets
```powershell
New-SSHShell -SshClient <TransferettoSshSession> [-TerminalName <string>] [-Columns <uint>] [-Rows <uint>] [-Width <uint>] [-Height <uint>] [-BufferSize <int>] [-NoTerminal] [-PromptPattern <string>] [-PromptPreset <TransferettoSshShellPromptPreset>] [<CommonParameters>]
```

## DESCRIPTION
Creates a reusable interactive SSH shell session.

Builds an interactive shell stream on top of an SSH session, with optional prompt presets or explicit prompt patterns, transcript support, and terminal sizing that can be reused by shell read, write, expect, and recipe cmdlets.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SSHShell -SshClient 'Value'
```


## PARAMETERS

### -BufferSize
Gets or sets the buffer Size.

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

### -Columns
Gets or sets the columns.

```yaml
Type: UInt32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Gets or sets the height.

```yaml
Type: UInt32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoTerminal
Gets or sets the no Terminal.

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

### -Rows
Gets or sets the rows.

```yaml
Type: UInt32
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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -TerminalName
Gets or sets the terminal Name.

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

### -Width
Gets or sets the width.

```yaml
Type: UInt32
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

- `Transferetto.TransferettoSshSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
