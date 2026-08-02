---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-FTPTracing
## SYNOPSIS
Enables or disables global FTP protocol tracing for the current PowerShell session.

Configures diagnostic logging visibility for usernames, passwords, and hosts so troubleshooting can be more detailed or more redacted depending on the scenario.

## SYNTAX
### __AllParameterSets
```powershell
Set-FTPTracing [-Enable] [-Disable] [-ShowPassword] [-HideUserName] [-HideIP] [<CommonParameters>]
```

## DESCRIPTION
Enables or disables global FTP protocol tracing for the current PowerShell session.

Configures diagnostic logging visibility for usernames, passwords, and hosts so troubleshooting can be more detailed or more redacted depending on the scenario.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-FTPTracing -Enable
```


## PARAMETERS

### -Disable
Gets or sets a value indicating whether disable.

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

### -Enable
Gets or sets a value indicating whether enable.

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

### -HideIP
Gets or sets the hide IP.

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

### -HideUserName
Gets or sets the hide User Name.

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

### -ShowPassword
Gets or sets the show Password.

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
