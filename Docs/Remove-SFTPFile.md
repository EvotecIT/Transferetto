---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Remove-SFTPFile
## SYNOPSIS
Removes a file from an SFTP server.

Deletes a single remote SFTP file and can suppress the returned operation result when used inside larger maintenance scripts.

## SYNTAX
### __AllParameterSets
```powershell
Remove-SFTPFile -SftpClient <TransferettoSftpSession> [-RemotePath <string>] [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Removes a file from an SFTP server.

Deletes a single remote SFTP file and can suppress the returned operation result when used inside larger maintenance scripts.

## EXAMPLES

### EXAMPLE 1
```powershell
Remove-SFTPFile -SftpClient 'Value'
```


## PARAMETERS

### -RemotePath
Gets or sets the remote Path.

```yaml
Type: String
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
