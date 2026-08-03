---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Write-SSHShell
## SYNOPSIS
Writes text into an interactive SSH shell session.

Sends raw text or line-based input to an existing shell stream, with optional newline suppression and pass-through support so command composition can stay in PowerShell pipelines.

## SYNTAX
### __AllParameterSets
```powershell
Write-SSHShell -ShellSession <TransferettoSshShellSession> -Text <string> [-NoNewLine] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Writes text into an interactive SSH shell session.

Sends raw text or line-based input to an existing shell stream, with optional newline suppression and pass-through support so command composition can stay in PowerShell pipelines.

## EXAMPLES

### EXAMPLE 1
```powershell
Write-SSHShell -ShellSession 'Value' -Text 'Value'
```


## PARAMETERS

### -NoNewLine
Gets or sets the no New Line.

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

### -Text
Gets or sets the text.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
