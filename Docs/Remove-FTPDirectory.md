---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Remove-FTPDirectory
## SYNOPSIS
Removes a directory from an FTP or FTPS server.

Deletes a remote FTP directory and can pass explicit listing options for servers that need additional directory enumeration behavior during recursive removal.

## SYNTAX
### __AllParameterSets
```powershell
Remove-FTPDirectory -Client <TransferettoFtpSession> -RemotePath <string> [-FtpListOption <FtpListOption>] [<CommonParameters>]
```

## DESCRIPTION
Removes a directory from an FTP or FTPS server.

Deletes a remote FTP directory and can pass explicit listing options for servers that need additional directory enumeration behavior during recursive removal.

## EXAMPLES

### EXAMPLE 1
```powershell
Remove-FTPDirectory -Client 'Value' -RemotePath 'C:\Path'
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

### -FtpListOption
Gets or sets the ftp List Option.

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
