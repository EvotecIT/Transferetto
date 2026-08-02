---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-SFTPWorkingDirectory
## SYNOPSIS
Returns the current working directory for an SFTP session.

Exposes the session’s active SFTP path so scripts can coordinate relative reads, writes, and directory-management steps safely.

## SYNTAX
### __AllParameterSets
```powershell
Get-SFTPWorkingDirectory -SftpClient <TransferettoSftpSession> [<CommonParameters>]
```

## DESCRIPTION
Returns the current working directory for an SFTP session.

Exposes the session’s active SFTP path so scripts can coordinate relative reads, writes, and directory-management steps safely.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SFTPWorkingDirectory -SftpClient 'Value'
```


## PARAMETERS

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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSftpSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
