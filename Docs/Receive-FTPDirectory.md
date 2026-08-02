---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Receive-FTPDirectory
## SYNOPSIS
Downloads a remote FTP or FTPS directory tree to the local machine.

Supports FluentFTP folder sync modes, local collision policy, optional verification rules, shared progress reporting, and cancellation-aware async directory downloads for both FTP and FTPS sessions.

## SYNTAX
### __AllParameterSets
```powershell
Receive-FTPDirectory -Client <TransferettoFtpSession> -RemotePath <string> [-LocalPath <string>] [-FolderSyncMode <FtpFolderSyncMode>] [-LocalExists <FtpLocalExists>] [-VerifyOptions <FtpVerify>] [-Rules <FtpRule[]>] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Downloads a remote FTP or FTPS directory tree to the local machine.

Supports FluentFTP folder sync modes, local collision policy, optional verification rules, shared progress reporting, and cancellation-aware async directory downloads for both FTP and FTPS sessions.

## EXAMPLES

### EXAMPLE 1
```powershell
Receive-FTPDirectory -Client 'Value' -RemotePath 'C:\Path'
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

### -FolderSyncMode
Gets or sets the folder Sync Mode.

```yaml
Type: FtpFolderSyncMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Mirror, Update

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalExists
Gets or sets the local exists behavior.

```yaml
Type: FtpLocalExists
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Overwrite, Resume, Skip, Append

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

### -Rules
Gets or sets the rules.

```yaml
Type: FtpRule[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
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

### -VerifyOptions
Gets or sets the verify options.

```yaml
Type: FtpVerify
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Retry, Delete, Throw, OnlyChecksum, OnlyVerify

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
