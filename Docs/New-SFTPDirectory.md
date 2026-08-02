---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-SFTPDirectory
## SYNOPSIS
Creates a directory on an SFTP server.

Creates a remote SFTP directory and returns the operation result unless output is suppressed, making it easy to compose idempotent provisioning flows.

## SYNTAX
### __AllParameterSets
```powershell
New-SFTPDirectory -SftpClient <TransferettoSftpSession> -Path <string> [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Creates a directory on an SFTP server.

Creates a remote SFTP directory and returns the operation result unless output is suppressed, making it easy to compose idempotent provisioning flows.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SFTPDirectory -SftpClient 'Value' -Path 'C:\Path'
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
