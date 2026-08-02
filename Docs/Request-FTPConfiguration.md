---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Request-FTPConfiguration
## SYNOPSIS
Probes an FTP or FTPS endpoint to discover compatible connection settings.

Runs Transferetto’s FTP configuration detection against a target server, optionally with credentials, and can return either the first working configuration or the full candidate set.

## SYNTAX
### Password (Default)
```powershell
Request-FTPConfiguration [-Server <string>] [-Credential <pscredential>] [-Port <int>] [-FirstOnly] [<CommonParameters>]
```

### ClearText
```powershell
Request-FTPConfiguration [-Server <string>] [-Username <string>] [-Password <string>] [-Port <int>] [-FirstOnly] [<CommonParameters>]
```

## DESCRIPTION
Probes an FTP or FTPS endpoint to discover compatible connection settings.

Runs Transferetto’s FTP configuration detection against a target server, optionally with credentials, and can return either the first working configuration or the full candidate set.

## EXAMPLES

### EXAMPLE 1
```powershell
Request-FTPConfiguration -Credential Get-Credential
```


## PARAMETERS

### -Credential
Gets or sets the credential used by the cmdlet.

```yaml
Type: PSCredential
Parameter Sets: Password
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FirstOnly
Gets or sets the first Only.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
Gets or sets the password.

```yaml
Type: String
Parameter Sets: ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Port
Gets or sets the network port.

```yaml
Type: Int32
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Server
Gets or sets the server name or address.

```yaml
Type: String
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Gets or sets the username.

```yaml
Type: String
Parameter Sets: ClearText
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
