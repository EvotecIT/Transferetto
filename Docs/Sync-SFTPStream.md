---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Sync-SFTPStream
## SYNOPSIS
Flushes buffered writes for an open SFTP stream session.

Forces pending SFTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.

## SYNTAX
### __AllParameterSets
```powershell
Sync-SFTPStream -StreamSession <TransferettoSftpStreamSession> [<CommonParameters>]
```

## DESCRIPTION
Flushes buffered writes for an open SFTP stream session.

Forces pending SFTP stream data to be synchronized so stream-based writes are committed before later operations such as verification, rename, or close.

## EXAMPLES

### EXAMPLE 1
```powershell
Sync-SFTPStream -StreamSession 'Value'
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
