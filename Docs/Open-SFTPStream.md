---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Open-SFTPStream
## SYNOPSIS
Opens a readable or writable SFTP stream for a remote file.

Creates a reusable stream session for low-level SFTP access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

## SYNTAX
### __AllParameterSets
```powershell
Open-SFTPStream -SftpClient <TransferettoSftpSession> -Path <string> [-Mode <TransferettoSftpStreamMode>] [<CommonParameters>]
```

## DESCRIPTION
Opens a readable or writable SFTP stream for a remote file.

Creates a reusable stream session for low-level SFTP access when callers need incremental reads or writes instead of a full-file transfer cmdlet.

## EXAMPLES

### EXAMPLE 1
```powershell
Open-SFTPStream -SftpClient 'Value' -Path 'C:\Path'
```


## PARAMETERS

### -Mode
Gets or sets the mode.

```yaml
Type: TransferettoSftpStreamMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Read, Write, Append

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
