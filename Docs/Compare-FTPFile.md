---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Compare-FTPFile
## SYNOPSIS
Compares a local file with a remote FTP file.

Uses FluentFTP comparison strategies to determine whether a local file matches a remote file by size, hash, or server-supported auto-detection logic.

## SYNTAX
### __AllParameterSets
```powershell
Compare-FTPFile -Client <TransferettoFtpSession> -LocalPath <string> -RemotePath <string> [-CompareOption <FtpCompareOption>] [<CommonParameters>]
```

## DESCRIPTION
Compares a local file with a remote FTP file.

Uses FluentFTP comparison strategies to determine whether a local file matches a remote file by size, hash, or server-supported auto-detection logic.

## EXAMPLES

### EXAMPLE 1
```powershell
Compare-FTPFile -Client 'Value' -LocalPath 'C:\Path' -RemotePath 'C:\Path'
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

### -CompareOption
Gets or sets the compare Option.

```yaml
Type: FtpCompareOption
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, Size, DateModified, Checksum

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalPath
Gets or sets the local Path.

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
