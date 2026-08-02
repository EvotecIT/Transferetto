---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Test-SFTPFile
## SYNOPSIS
Checks whether a remote SFTP file exists.

Returns a Boolean-like existence result for a remote SFTP file path, which is useful for guard clauses and idempotent deployment flows.

## SYNTAX
### __AllParameterSets
```powershell
Test-SFTPFile -SftpClient <TransferettoSftpSession> -Path <string> [<CommonParameters>]
```

## DESCRIPTION
Checks whether a remote SFTP file exists.

Returns a Boolean-like existence result for a remote SFTP file path, which is useful for guard clauses and idempotent deployment flows.

## EXAMPLES

### EXAMPLE 1
```powershell
Test-SFTPFile -SftpClient 'Value' -Path 'C:\Path'
```


## PARAMETERS

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
