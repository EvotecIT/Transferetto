---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-SFTPContent
## SYNOPSIS
Reads the full content of a remote SFTP file as text or bytes.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, with support for text decoding or raw byte retrieval.

## SYNTAX
### Text (Default)
```powershell
Get-SFTPContent -SftpClient <TransferettoSftpSession> -Path <string> [-Encoding <Encoding>] [<CommonParameters>]
```

### Bytes
```powershell
Get-SFTPContent -SftpClient <TransferettoSftpSession> -Path <string> [-AsByteArray] [<CommonParameters>]
```

## DESCRIPTION
Reads the full content of a remote SFTP file as text or bytes.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, with support for text decoding or raw byte retrieval.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SFTPContent -SftpClient 'Value' -Path 'C:\Path'
```


## PARAMETERS

### -AsByteArray
Gets or sets the as Byte Array.

```yaml
Type: SwitchParameter
Parameter Sets: Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Encoding
Gets or sets the encoding.

```yaml
Type: Encoding
Parameter Sets: Text
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
Parameter Sets: Text, Bytes
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
Parameter Sets: Text, Bytes
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
