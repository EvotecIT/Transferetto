---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Start-FXPFileTransfer
## SYNOPSIS
Transfers a file directly between two FTP/FTPS servers by using FXP.

Starts a server-to-server file copy through the reusable Transferetto FXP layer, with remote collision handling, optional verification, destination directory creation, and progress reporting.

## SYNTAX
### __AllParameterSets
```powershell
Start-FXPFileTransfer -Client <TransferettoFtpSession> -SourcePath <string> -DestinationClient <TransferettoFtpSession> -DestinationPath <string> [-CreateRemoteDirectory] [-RemoteExists <FtpRemoteExists>] [-VerifyOptions <FtpVerify[]>] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Transfers a file directly between two FTP/FTPS servers by using FXP.

Starts a server-to-server file copy through the reusable Transferetto FXP layer, with remote collision handling, optional verification, destination directory creation, and progress reporting.

## EXAMPLES

### EXAMPLE 1
```powershell
Start-FXPFileTransfer -Client 'Value' -SourcePath 'C:\Path' -DestinationClient 'Value' -DestinationPath 'C:\Path'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: __AllParameterSets
Aliases: SourceClient
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CreateRemoteDirectory
Gets or sets the create Remote Directory.

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

### -DestinationClient
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

### -DestinationPath
Gets or sets the destination Path.

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

### -RemoteExists
Gets or sets the remote Exists.

```yaml
Type: FtpRemoteExists
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: NoCheck, ResumeNoCheck, AddToEndNoCheck, Skip, Overwrite, OverwriteInPlace, Resume, AddToEnd, Append

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

### -SourcePath
Gets or sets the source Path.

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

### -VerifyOptions
Gets or sets the verify Options.

```yaml
Type: FtpVerify[]
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
