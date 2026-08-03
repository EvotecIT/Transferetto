---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Rename-SFTPFile
## SYNOPSIS
Renames a remote SFTP file or relocates it to a new path.

Performs a server-side rename for a remote SFTP item, which is useful for finalizing staged uploads or rotating files in place.

## SYNTAX
### __AllParameterSets
```powershell
Rename-SFTPFile -SftpClient <TransferettoSftpSession> [-SourcePath <string>] [-DestinationPath <string>] [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Renames a remote SFTP file or relocates it to a new path.

Performs a server-side rename for a remote SFTP item, which is useful for finalizing staged uploads or rotating files in place.

## EXAMPLES

### EXAMPLE 1
```powershell
Rename-SFTPFile -SftpClient 'Value'
```


## PARAMETERS

### -DestinationPath
Gets or sets the destination Path.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: NewPath
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
Aliases: OldPath
Possible values:

Required: False
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
