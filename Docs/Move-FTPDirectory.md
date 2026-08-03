---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Move-FTPDirectory
## SYNOPSIS
Moves or renames a directory on an FTP or FTPS server.

Relocates a remote FTP directory to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.

## SYNTAX
### __AllParameterSets
```powershell
Move-FTPDirectory -Client <TransferettoFtpSession> -RemoteSource <string> -RemoteDestination <string> [-RemoteExists <FtpRemoteExists>] [<CommonParameters>]
```

## DESCRIPTION
Moves or renames a directory on an FTP or FTPS server.

Relocates a remote FTP directory to a new path, with optional destination collision handling that follows FluentFTP remote-exists behavior.

## EXAMPLES

### EXAMPLE 1
```powershell
Move-FTPDirectory -Client 'Value' -RemoteSource 'Value' -RemoteDestination 'Value'
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

### -RemoteDestination
Gets or sets the remote Destination.

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

### -RemoteExists
Gets or sets the remote Exists.

```yaml
Type: FtpRemoteExists
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: NoCheck, ResumeNoCheck, AddToEndNoCheck, Skip, Overwrite, OverwriteInPlace, Resume, AddToEnd, Append

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoteSource
Gets or sets the remote Source.

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
