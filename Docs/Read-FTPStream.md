---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Read-FTPStream
## SYNOPSIS
Reads bytes or text from an open FTP stream session.

Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental FTP stream reads can be scripted without buffering an entire file up front.

## SYNTAX
### Bytes (Default)
```powershell
Read-FTPStream -StreamSession <TransferettoFtpStreamSession> [-Count <int>] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

### Text
```powershell
Read-FTPStream -StreamSession <TransferettoFtpStreamSession> [-Count <int>] [-AsText] [-Encoding <Encoding>] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Reads bytes or text from an open FTP stream session.

Supports chunked reads, optional text decoding, and progress-aware async execution so large or incremental FTP stream reads can be scripted without buffering an entire file up front.

## EXAMPLES

### EXAMPLE 1
```powershell
Read-FTPStream -StreamSession 'Value'
```


## PARAMETERS

### -AsText
Gets or sets the as Text.

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

### -Count
Gets or sets the count.

```yaml
Type: Int32
Parameter Sets: Bytes, Text
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

### -ProgressIntervalBytes
Gets or sets the minimum number of bytes between progress updates.

```yaml
Type: Int64
Parameter Sets: Bytes, Text
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
Parameter Sets: Bytes, Text
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
Type: TransferettoFtpStreamSession
Parameter Sets: Bytes, Text
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

- `Transferetto.TransferettoFtpStreamSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
