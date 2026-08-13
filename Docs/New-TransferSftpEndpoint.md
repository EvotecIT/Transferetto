---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-TransferSftpEndpoint
## SYNOPSIS
Wraps a connected SFTP session as a Transferetto endpoint.

Reuses a session created by Connect-SFTP so provider-neutral commands such as Copy-TransferItem can stream data between SFTP and other Transferetto providers.

## SYNTAX
### __AllParameterSets
```powershell
New-TransferSftpEndpoint [-SftpSession] <TransferettoSftpSession> [-Prefix <string>] [-OwnSession] [<CommonParameters>]
```

## DESCRIPTION
Wraps a connected SFTP session as a Transferetto endpoint.

Reuses a session created by Connect-SFTP so provider-neutral commands such as Copy-TransferItem can stream data between SFTP and other Transferetto providers.

## EXAMPLES

### EXAMPLE 1
```powershell
New-TransferSftpEndpoint -OwnSession
```


## PARAMETERS

### -OwnSession
Gets or sets whether closing the endpoint also disposes the wrapped session.

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

### -Prefix
Gets or sets the endpoint-relative remote path prefix.

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

### -SftpSession
Gets or sets the connected SFTP session.

```yaml
Type: TransferettoSftpSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSftpSession`

## OUTPUTS

- `Transferetto.SftpTransferEndpoint`

## RELATED LINKS

- None
