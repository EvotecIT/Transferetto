---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPModifiedTime
## SYNOPSIS
Returns the last modified time for a remote FTP item.

Reads the remote timestamp reported by the FTP server, which is useful for deployment comparisons, freshness checks, and timestamp synchronization.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPModifiedTime -Client <TransferettoFtpSession> -RemotePath <string> [<CommonParameters>]
```

## DESCRIPTION
Returns the last modified time for a remote FTP item.

Reads the remote timestamp reported by the FTP server, which is useful for deployment comparisons, freshness checks, and timestamp synchronization.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPModifiedTime -Client 'Value' -RemotePath 'C:\Path'
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
