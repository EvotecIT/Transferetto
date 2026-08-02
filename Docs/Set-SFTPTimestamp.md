---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SFTPTimestamp
## SYNOPSIS
Sets access and/or write timestamps for a remote SFTP item.

Updates one or both SFTP timestamps, with optional UTC semantics, and can return refreshed item metadata after the change.

## SYNTAX
### __AllParameterSets
```powershell
Set-SFTPTimestamp -SftpClient <TransferettoSftpSession> -Path <string> [-LastAccessTime <datetime>] [-LastWriteTime <datetime>] [-Utc] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Sets access and/or write timestamps for a remote SFTP item.

Updates one or both SFTP timestamps, with optional UTC semantics, and can return refreshed item metadata after the change.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SFTPTimestamp -SftpClient 'Value' -Path 'C:\Path'
```


## PARAMETERS

### -LastAccessTime
Gets or sets the last Access Time.

```yaml
Type: DateTime
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LastWriteTime
Gets or sets the last Write Time.

```yaml
Type: DateTime
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Gets or sets the pass Thru.

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

### -Utc
Gets or sets the utc.

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
