---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-FTPOption
## SYNOPSIS
Adjusts runtime options on an existing FTP session.

Lets scripts fine-tune retry behavior and zero-byte download handling on a live session without reconnecting.

## SYNTAX
### __AllParameterSets
```powershell
Set-FTPOption -Client <TransferettoFtpSession> [-RetryAttempts <Int32>] [-DownloadZeroByteFiles <Boolean>] [<CommonParameters>]
```

## DESCRIPTION
Adjusts runtime options on an existing FTP session.

Lets scripts fine-tune retry behavior and zero-byte download handling on a live session without reconnecting.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-FTPOption -Client 'Value'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

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

### -DownloadZeroByteFiles
Gets or sets the download Zero Byte Files.

```yaml
Type: Boolean
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetryAttempts
Gets or sets the retry Attempts.

```yaml
Type: Int32
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
