---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-FTPModifiedTime
## SYNOPSIS
Sets the last modified time for a remote FTP item.

Writes a remote timestamp and can optionally return the updated item metadata, which is helpful for preserving deployment timestamps after upload.

## SYNTAX
### __AllParameterSets
```powershell
Set-FTPModifiedTime -Client <TransferettoFtpSession> -RemotePath <string> -ModifiedTime <datetime> [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Sets the last modified time for a remote FTP item.

Writes a remote timestamp and can optionally return the updated item metadata, which is helpful for preserving deployment timestamps after upload.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-FTPModifiedTime -Client 'Value' -RemotePath 'C:\Path' -ModifiedTime '2000-01-01'
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

### -ModifiedTime
Gets or sets the modified Time.

```yaml
Type: DateTime
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Gets or sets the pass Thru.

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
