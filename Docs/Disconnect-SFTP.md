---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Disconnect-SFTP
## SYNOPSIS
Disconnects an SFTP session.

Closes the reusable SFTP session created by Connect-SFTP so the underlying SSH transport is released cleanly when file operations are done.

## SYNTAX
### __AllParameterSets
```powershell
Disconnect-SFTP -SftpClient <TransferettoSftpSession> [<CommonParameters>]
```

## DESCRIPTION
Disconnects an SFTP session.

Closes the reusable SFTP session created by Connect-SFTP so the underlying SSH transport is released cleanly when file operations are done.

## EXAMPLES

### EXAMPLE 1
```powershell
Disconnect-SFTP -SftpClient 'Value'
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
