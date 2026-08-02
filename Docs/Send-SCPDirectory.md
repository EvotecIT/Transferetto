---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Send-SCPDirectory
## SYNOPSIS
Uploads a local directory tree through an SCP session.

Supports recursive SCP uploads with shared progress reporting and cancellation-aware async execution, making it suitable for simple release and backup flows that do not need SFTP-specific metadata operations.

## SYNTAX
### __AllParameterSets
```powershell
Send-SCPDirectory -ScpClient <TransferettoScpSession> -LocalPath <string> -RemotePath <string> [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Uploads a local directory tree through an SCP session.

Supports recursive SCP uploads with shared progress reporting and cancellation-aware async execution, making it suitable for simple release and backup flows that do not need SFTP-specific metadata operations.

## EXAMPLES

### EXAMPLE 1
```powershell
Send-SCPDirectory -ScpClient 'Value' -LocalPath 'C:\Path' -RemotePath 'C:\Path'
```


## PARAMETERS

### -LocalPath
Gets or sets the local Path.

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

### -ProgressIntervalBytes
Gets or sets the minimum number of bytes between progress updates.

```yaml
Type: Int64
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

### -ScpClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoScpSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowProgress
Gets or sets a value indicating whether transfer progress is displayed.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- None
