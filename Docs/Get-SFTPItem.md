---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-SFTPItem
## SYNOPSIS
Retrieves metadata for a single SFTP file-system item.

Returns SFTP attributes for a target path so scripts can inspect timestamps, permissions, size, and item type before taking further action.

## SYNTAX
### __AllParameterSets
```powershell
Get-SFTPItem -SftpClient <TransferettoSftpSession> -Path <string> [<CommonParameters>]
```

## DESCRIPTION
Retrieves metadata for a single SFTP file-system item.

Returns SFTP attributes for a target path so scripts can inspect timestamps, permissions, size, and item type before taking further action.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SFTPItem -SftpClient 'Value' -Path 'C:\Path'
```


## PARAMETERS

### -Path
Gets or sets the path.

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

### -SftpClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoSftpSession
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
