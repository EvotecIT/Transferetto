---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Test-FXPTransfer
## SYNOPSIS
Preflights whether an FXP transfer can run between two FTP/FTPS sessions.

Evaluates the requested source, destination, transfer kind, and optional destination-directory creation rules before a full FXP transfer is attempted.

## SYNTAX
### __AllParameterSets
```powershell
Test-FXPTransfer -Client <TransferettoFtpSession> -SourcePath <string> -DestinationClient <TransferettoFtpSession> -DestinationPath <string> [-TransferKind <TransferettoFxpTransferKind>] [-CreateRemoteDirectory] [<CommonParameters>]
```

## DESCRIPTION
Preflights whether an FXP transfer can run between two FTP/FTPS sessions.

Evaluates the requested source, destination, transfer kind, and optional destination-directory creation rules before a full FXP transfer is attempted.

## EXAMPLES

### EXAMPLE 1
```powershell
Test-FXPTransfer -Client 'Value' -SourcePath 'C:\Path' -DestinationClient 'Value' -DestinationPath 'C:\Path'
```


## PARAMETERS

### -Client
Gets or sets the source session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: __AllParameterSets
Aliases: SourceClient
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CreateRemoteDirectory
Gets or sets a value indicating whether a missing destination parent can be created by the transfer.

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

### -DestinationClient
Gets or sets the destination session object used by the cmdlet.

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

### -DestinationPath
Gets or sets the destination path.

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

### -SourcePath
Gets or sets the source path.

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

### -TransferKind
Gets or sets the FXP transfer kind.

```yaml
Type: TransferettoFxpTransferKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: File, Directory

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
