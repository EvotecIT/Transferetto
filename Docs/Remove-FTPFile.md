---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Remove-FTPFile
## SYNOPSIS
Removes a file from an FTP or FTPS server.

Deletes a single remote FTP file, which fits cleanup, rollback, and artifact rotation workflows.

## SYNTAX
### __AllParameterSets
```powershell
Remove-FTPFile -Client <TransferettoFtpSession> -RemotePath <string> [<CommonParameters>]
```

## DESCRIPTION
Removes a file from an FTP or FTPS server.

Deletes a single remote FTP file, which fits cleanup, rollback, and artifact rotation workflows.

## EXAMPLES

### EXAMPLE 1
```powershell
Remove-FTPFile -Client 'Value' -RemotePath 'C:\Path'
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
