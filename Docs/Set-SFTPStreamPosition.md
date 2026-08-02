---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SFTPStreamPosition
## SYNOPSIS
Moves the current position within an open SFTP stream session.

Seeks to a new offset in the SFTP stream so callers can reread, skip ahead, or resume low-level stream-based operations from a specific location.

## SYNTAX
### __AllParameterSets
```powershell
Set-SFTPStreamPosition -StreamSession <TransferettoSftpStreamSession> -Offset <long> [-Origin <SeekOrigin>] [<CommonParameters>]
```

## DESCRIPTION
Moves the current position within an open SFTP stream session.

Seeks to a new offset in the SFTP stream so callers can reread, skip ahead, or resume low-level stream-based operations from a specific location.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SFTPStreamPosition -StreamSession 'Value' -Offset 1
```


## PARAMETERS

### -Offset
Gets or sets the offset.

```yaml
Type: Int64
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Origin
Gets or sets the origin.

```yaml
Type: SeekOrigin
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Begin, Current, End

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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
