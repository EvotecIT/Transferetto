---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPList
## SYNOPSIS
Lists files and directories from an FTP or FTPS session.

Returns Transferetto remote item objects for a target FTP path, with optional FluentFTP listing flags for recursive, force-listing, or link-aware enumeration scenarios.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPList -Client <TransferettoFtpSession> [-Path <string>] [-Options <FtpListOption>] [<CommonParameters>]
```

## DESCRIPTION
Lists files and directories from an FTP or FTPS session.

Returns Transferetto remote item objects for a target FTP path, with optional FluentFTP listing flags for recursive, force-listing, or link-aware enumeration scenarios.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPList -Client 'Value'
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

### -Options
Gets or sets the options.

```yaml
Type: FtpListOption
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, Modify, Size, SizeModify, AllFiles, ForceList, NameList, ForceNameList, UseLS, Recursive, NoPath, IncludeSelfAndParent, UseStat

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoFtpSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
