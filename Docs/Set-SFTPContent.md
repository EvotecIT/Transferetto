---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SFTPContent
## SYNOPSIS
Writes text or bytes to a remote SFTP file.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, supporting text writes, append mode, or raw byte content.

## SYNTAX
### Text (Default)
```powershell
Set-SFTPContent -SftpClient <TransferettoSftpSession> -Path <string> -Value <string> [-Encoding <Encoding>] [-Append] [<CommonParameters>]
```

### Bytes
```powershell
Set-SFTPContent -SftpClient <TransferettoSftpSession> -Path <string> -ByteContent <byte[]> [<CommonParameters>]
```

## DESCRIPTION
Writes text or bytes to a remote SFTP file.

Provides a simple content-oriented shortcut over the SFTP stream APIs for smaller files, supporting text writes, append mode, or raw byte content.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SFTPContent -SftpClient 'Value' -Path 'C:\Path' -Value 'Value'
```


### EXAMPLE 2
```powershell
Set-SFTPContent -SftpClient 'Value' -Path 'C:\Path' -ByteContent @('Value')
```


## PARAMETERS

### -Append
Gets or sets the append.

```yaml
Type: SwitchParameter
Parameter Sets: Text
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ByteContent
Gets or sets the byte Content.

```yaml
Type: Byte[]
Parameter Sets: Bytes
Aliases: None
Possible values:

Required: True
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

### -Value
Gets or sets the value.

```yaml
Type: String
Parameter Sets: Text
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
