---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Write-SFTPStream
## SYNOPSIS
Writes text or bytes to an open SFTP stream session.

Supports text encoding or raw byte writes, optional flush behavior, and progress-aware async execution for low-level SFTP upload or remote content-editing scenarios.

## SYNTAX
### Text (Default)
```powershell
Write-SFTPStream -StreamSession <TransferettoSftpStreamSession> -Text <string> [-Encoding <Encoding>] [-Flush] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

### Bytes
```powershell
Write-SFTPStream -StreamSession <TransferettoSftpStreamSession> -ByteContent <byte[]> [-Flush] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Writes text or bytes to an open SFTP stream session.

Supports text encoding or raw byte writes, optional flush behavior, and progress-aware async execution for low-level SFTP upload or remote content-editing scenarios.

## EXAMPLES

### EXAMPLE 1
```powershell
Write-SFTPStream -StreamSession 'Value' -Text 'Value'
```


### EXAMPLE 2
```powershell
Write-SFTPStream -StreamSession 'Value' -ByteContent @('Value')
```


## PARAMETERS

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

### -Flush
Gets or sets the flush.

```yaml
Type: SwitchParameter
Parameter Sets: Text, Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressIntervalBytes
Gets or sets the minimum number of bytes between progress updates.

```yaml
Type: Int64
Parameter Sets: Text, Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowProgress
Gets or sets a value indicating whether stream progress is displayed.

```yaml
Type: SwitchParameter
Parameter Sets: Text, Bytes
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StreamSession
Gets or sets the stream Session.

```yaml
Type: TransferettoSftpStreamSession
Parameter Sets: Text, Bytes
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Gets or sets the text.

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
