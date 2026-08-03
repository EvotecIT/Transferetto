---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Disconnect-FTP
## SYNOPSIS
Disconnects an FTP or FTPS session.

Closes the reusable FTP session created by Connect-FTP so sockets and authentication state are released cleanly at the end of a script or pipeline.

## SYNTAX
### __AllParameterSets
```powershell
Disconnect-FTP [-Client <TransferettoFtpSession>] [<CommonParameters>]
```

## DESCRIPTION
Disconnects an FTP or FTPS session.

Closes the reusable FTP session created by Connect-FTP so sockets and authentication state are released cleanly at the end of a script or pipeline.

## EXAMPLES

### EXAMPLE 1
```powershell
Disconnect-FTP -Client 'Value'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
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
