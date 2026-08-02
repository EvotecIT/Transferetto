---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPWorkingDirectory
## SYNOPSIS
Returns the current working directory for an FTP or FTPS session.

Exposes the session’s active remote working directory so scripts can confirm navigation state before relative listing, upload, download, or rename operations.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPWorkingDirectory -Client <TransferettoFtpSession> [<CommonParameters>]
```

## DESCRIPTION
Returns the current working directory for an FTP or FTPS session.

Exposes the session’s active remote working directory so scripts can confirm navigation state before relative listing, upload, download, or rename operations.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPWorkingDirectory -Client 'Value'
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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoFtpSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
