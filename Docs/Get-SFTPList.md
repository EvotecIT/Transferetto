---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-SFTPList
## SYNOPSIS
Lists files and directories from an SFTP session.

Returns Transferetto remote item objects for a target SFTP path so PowerShell scripts can inspect directory contents, filter entries, and pipe them into later file-management commands.

## SYNTAX
### __AllParameterSets
```powershell
Get-SFTPList -SftpClient <TransferettoSftpSession> [-Path <string>] [<CommonParameters>]
```

## DESCRIPTION
Lists files and directories from an SFTP session.

Returns Transferetto remote item objects for a target SFTP path so PowerShell scripts can inspect directory contents, filter entries, and pipe them into later file-management commands.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SFTPList -SftpClient 'Value'
```


## PARAMETERS

### -Path
Gets or sets the path.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: FtpPath
Possible values:

Required: False
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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSftpSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
