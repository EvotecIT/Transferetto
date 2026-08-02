---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Receive-SFTPDirectory
## SYNOPSIS
Downloads a remote directory tree from an SFTP session.

Supports recursive SFTP downloads with overwrite control, progress reporting, and cancellation-aware async execution so local staging and backup workflows behave consistently across protocols.

## SYNTAX
### __AllParameterSets
```powershell
Receive-SFTPDirectory -SftpClient <TransferettoSftpSession> -RemotePath <string> -LocalPath <string> [-AllowOverride] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Downloads a remote directory tree from an SFTP session.

Supports recursive SFTP downloads with overwrite control, progress reporting, and cancellation-aware async execution so local staging and backup workflows behave consistently across protocols.

## EXAMPLES

### EXAMPLE 1
```powershell
Receive-SFTPDirectory -SftpClient 'Value' -RemotePath 'C:\Path' -LocalPath 'C:\Path'
```


## PARAMETERS

### -AllowOverride
Gets or sets the allow Override.

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

### -SftpClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoSftpSession
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
