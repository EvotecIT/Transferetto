---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Move-SFTPFile
## SYNOPSIS
Moves or renames a file on an SFTP server.

Renames or relocates a remote SFTP file, optionally using POSIX rename semantics when the server supports them, and returns a structured operation result unless suppressed.

## SYNTAX
### __AllParameterSets
```powershell
Move-SFTPFile -SftpClient <TransferettoSftpSession> -SourcePath <string> -DestinationPath <string> [-PosixRename] [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Moves or renames a file on an SFTP server.

Renames or relocates a remote SFTP file, optionally using POSIX rename semantics when the server supports them, and returns a structured operation result unless suppressed.

## EXAMPLES

### EXAMPLE 1
```powershell
Move-SFTPFile -SftpClient 'Value' -SourcePath 'C:\Path' -DestinationPath 'C:\Path'
```


## PARAMETERS

### -DestinationPath
Gets or sets the destination Path.

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

### -PosixRename
Gets or sets the posix Rename.

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

### -SourcePath
Gets or sets the source Path.

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

### -Suppress
Gets or sets the suppress.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
