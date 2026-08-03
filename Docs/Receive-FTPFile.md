---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Receive-FTPFile
## SYNOPSIS
Downloads one or more files from an FTP or FTPS session to the local machine.

Supports explicit remote paths or native listing objects, local collision policy, optional verification, shared transfer progress, and cancellation-aware async downloads for both FTP and FTPS sessions.

## SYNTAX
### Text (Default)
```powershell
Receive-FTPFile -Client <TransferettoFtpSession> [-RemotePath <string[]>] [-LocalPath <string>] [-LocalExists <FtpLocalExists>] [-VerifyOptions <FtpVerify>] [-FtpError <FtpError>] [-Suppress] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

### Native
```powershell
Receive-FTPFile -Client <TransferettoFtpSession> [-RemoteFile <psobject[]>] [-RemotePath <string[]>] [-LocalPath <string>] [-LocalExists <FtpLocalExists>] [-VerifyOptions <FtpVerify>] [-FtpError <FtpError>] [-Suppress] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Downloads one or more files from an FTP or FTPS session to the local machine.

Supports explicit remote paths or native listing objects, local collision policy, optional verification, shared transfer progress, and cancellation-aware async downloads for both FTP and FTPS sessions.

## EXAMPLES

### EXAMPLE 1
```powershell
Receive-FTPFile -Client 'Value'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: Text, Native
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FtpError
Gets or sets the ftp Error.

```yaml
Type: FtpError
Parameter Sets: Text, Native
Aliases: None
Possible values: None, DeleteProcessed, Stop, Throw

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalExists
Gets or sets the local Exists.

```yaml
Type: FtpLocalExists
Parameter Sets: Text, Native
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
Parameter Sets: Text, Native
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
Parameter Sets: Text, Native
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoteFile
Gets or sets the remote File.

```yaml
Type: PSObject[]
Parameter Sets: Native
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
Type: String[]
Parameter Sets: Text, Native
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
Parameter Sets: Text, Native
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Suppress
Gets or sets the suppress.

```yaml
Type: SwitchParameter
Parameter Sets: Text, Native
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerifyOptions
Gets or sets the verify Options.

```yaml
Type: FtpVerify
Parameter Sets: Text, Native
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
