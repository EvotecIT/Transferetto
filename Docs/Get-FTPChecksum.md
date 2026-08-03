---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Get-FTPChecksum
## SYNOPSIS
Calculates or retrieves a checksum for a remote FTP file.

Requests a remote hash from the FTP server by using the selected algorithm, which is useful for post-upload verification and drift detection workflows.

## SYNTAX
### __AllParameterSets
```powershell
Get-FTPChecksum -Client <TransferettoFtpSession> -RemotePath <string> [-HashAlgorithm <FtpHashAlgorithm>] [<CommonParameters>]
```

## DESCRIPTION
Calculates or retrieves a checksum for a remote FTP file.

Requests a remote hash from the FTP server by using the selected algorithm, which is useful for post-upload verification and drift detection workflows.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-FTPChecksum -Client 'Value' -RemotePath 'C:\Path'
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

### -HashAlgorithm
Gets or sets the hash Algorithm.

```yaml
Type: FtpHashAlgorithm
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: NONE, SHA1, SHA256, SHA512, MD5, CRC

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePath
Gets or sets the remote Path.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
