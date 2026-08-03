---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Remove-SFTPDirectory
## SYNOPSIS
Removes a directory from an SFTP server.

Deletes a remote SFTP directory and returns the operation result unless suppressed, which fits well into cleanup and deployment-rollback scripts.

## SYNTAX
### __AllParameterSets
```powershell
Remove-SFTPDirectory -SftpClient <TransferettoSftpSession> -Path <string> [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Removes a directory from an SFTP server.

Deletes a remote SFTP directory and returns the operation result unless suppressed, which fits well into cleanup and deployment-rollback scripts.

## EXAMPLES

### EXAMPLE 1
```powershell
Remove-SFTPDirectory -SftpClient 'Value' -Path 'C:\Path'
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
