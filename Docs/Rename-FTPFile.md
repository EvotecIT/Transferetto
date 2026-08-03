---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Rename-FTPFile
## SYNOPSIS
Renames a remote FTP file or relocates it to a new path.

Performs a server-side rename for a remote FTP item, which is useful for finalizing staged uploads or rotating files in place.

## SYNTAX
### __AllParameterSets
```powershell
Rename-FTPFile -Client <TransferettoFtpSession> -Path <string> -DestinationPath <string> [<CommonParameters>]
```

## DESCRIPTION
Renames a remote FTP file or relocates it to a new path.

Performs a server-side rename for a remote FTP item, which is useful for finalizing staged uploads or rotating files in place.

## EXAMPLES

### EXAMPLE 1
```powershell
Rename-FTPFile -Client 'Value' -Path 'C:\Path' -DestinationPath 'C:\Path'
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
