---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Close-SFTPStream
## SYNOPSIS
Closes an open SFTP stream session.

Releases the low-level SFTP stream created by Open-SFTPStream so the remote file handle and associated transport resources are closed cleanly.

## SYNTAX
### __AllParameterSets
```powershell
Close-SFTPStream -StreamSession <TransferettoSftpStreamSession> [<CommonParameters>]
```

## DESCRIPTION
Closes an open SFTP stream session.

Releases the low-level SFTP stream created by Open-SFTPStream so the remote file handle and associated transport resources are closed cleanly.

## EXAMPLES

### EXAMPLE 1
```powershell
Close-SFTPStream -StreamSession 'Value'
```


## PARAMETERS

### -StreamSession
Gets or sets the stream Session.

```yaml
Type: TransferettoSftpStreamSession
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

- `Transferetto.TransferettoSftpStreamSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
