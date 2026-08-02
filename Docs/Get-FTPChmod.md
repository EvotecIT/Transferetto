---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPChmod
## SYNOPSIS
Reads POSIX-style permission bits for a remote FTP item.

Returns the remote mode/permission information reported by the FTP server so scripts can inspect Unix-style access flags before applying changes.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPChmod -Client <TransferettoFtpSession> [-RemotePath <string>] [<CommonParameters>]
```

## DESCRIPTION
Reads POSIX-style permission bits for a remote FTP item.

Returns the remote mode/permission information reported by the FTP server so scripts can inspect Unix-style access flags before applying changes.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPChmod -Client 'Value'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePath
Gets or sets the remote Path.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
