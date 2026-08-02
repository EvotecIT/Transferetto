---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Close-FTPStream
## SYNOPSIS
Closes an open FTP stream session.

Releases the low-level FTP stream created by Open-FTPStream so the remote file handle and associated transfer resources are closed cleanly.

## SYNTAX
### __AllParameterSets
```powershell
Close-FTPStream -StreamSession <TransferettoFtpStreamSession> [<CommonParameters>]
```

## DESCRIPTION
Closes an open FTP stream session.

Releases the low-level FTP stream created by Open-FTPStream so the remote file handle and associated transfer resources are closed cleanly.

## EXAMPLES

### EXAMPLE 1
```powershell
Close-FTPStream -StreamSession 'Value'
```


## PARAMETERS

### -StreamSession
Gets or sets the stream Session.

```yaml
Type: TransferettoFtpStreamSession
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

- `Transferetto.TransferettoFtpStreamSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
