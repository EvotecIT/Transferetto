---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPItem
## SYNOPSIS
Retrieves metadata for a single FTP or FTPS file-system item.

Returns a single remote item with file or directory metadata, optionally following symbolic links when the remote server exposes them through the FTP listing surface.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPItem -Client <TransferettoFtpSession> -RemotePath <string> [-FollowLinks] [<CommonParameters>]
```

## DESCRIPTION
Retrieves metadata for a single FTP or FTPS file-system item.

Returns a single remote item with file or directory metadata, optionally following symbolic links when the remote server exposes them through the FTP listing surface.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPItem -Client 'Value' -RemotePath 'C:\Path'
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

### -FollowLinks
Gets or sets the follow Links.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
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
