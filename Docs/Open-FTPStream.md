---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Open-FTPStream
## SYNOPSIS
Opens a readable or writable FTP stream for a remote file.

Creates a reusable stream session for low-level FTP file access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

## SYNTAX
### __AllParameterSets
```powershell
Open-FTPStream -Client <TransferettoFtpSession> -RemotePath <string> [-Mode <TransferettoFtpStreamMode>] [<CommonParameters>]
```

## DESCRIPTION
Opens a readable or writable FTP stream for a remote file.

Creates a reusable stream session for low-level FTP file access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

## EXAMPLES

### EXAMPLE 1
```powershell
Open-FTPStream -Client 'Value' -RemotePath 'C:\Path'
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

### -Mode
Gets or sets the mode.

```yaml
Type: TransferettoFtpStreamMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Read, Write, Append

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
