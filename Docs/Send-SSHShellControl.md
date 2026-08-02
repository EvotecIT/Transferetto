---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Send-SSHShellControl
## SYNOPSIS
Sends control-key input to an interactive SSH shell session.

Provides a safe way to send interrupt and navigation keys such as Ctrl+C or Ctrl+D without embedding terminal escape sequences directly into shell automation scripts.

## SYNTAX
### __AllParameterSets
```powershell
Send-SSHShellControl -ShellSession <TransferettoSshShellSession> -Key <TransferettoSshShellControlKey> [-Repeat <int>] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Sends control-key input to an interactive SSH shell session.

Provides a safe way to send interrupt and navigation keys such as Ctrl+C or Ctrl+D without embedding terminal escape sequences directly into shell automation scripts.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-SSHShellControl -ShellSession 'Value' -Key 'Value'
```


## PARAMETERS

### -Key
Gets or sets the key.

```yaml
Type: TransferettoSshShellControlKey
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Interrupt, EndOfTransmission, Suspend, EndOfText, Escape, Tab, Enter

Required: True
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

### -Repeat
Gets or sets the repeat.

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
