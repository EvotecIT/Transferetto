---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Sync-SFTPDirectory
## SYNOPSIS
Synchronizes a local directory with an SFTP directory.

Uses the shared Transferetto synchronization planner to upload or download missing and changed files, optionally mirror-delete extra destination items, filter paths by wildcard patterns, preserve timestamps, and preview planned work with dry-run mode.

## SYNTAX
### __AllParameterSets
```powershell
Sync-SFTPDirectory -SftpClient <TransferettoSftpSession> -LocalPath <string> -RemotePath <string> [-Direction <TransferettoSyncDirection>] [-Mode <TransferettoSyncMode>] [-Comparison <TransferettoSyncComparison>] [-Include <string[]>] [-Exclude <string[]>] [-DryRun] [-NoOverwrite] [-NoCreateDirectories] [-NoPreserveTimestamps] [-TimestampToleranceSeconds <int>] [-ShowProgress] [-ProgressIntervalBytes <long>] [<CommonParameters>]
```

## DESCRIPTION
Synchronizes a local directory with an SFTP directory.

Uses the shared Transferetto synchronization planner to upload or download missing and changed files, optionally mirror-delete extra destination items, filter paths by wildcard patterns, preserve timestamps, and preview planned work with dry-run mode.

## EXAMPLES

### EXAMPLE 1
```powershell
Sync-SFTPDirectory -SftpClient 'Value' -LocalPath 'C:\Path' -RemotePath 'C:\Path'
```


## PARAMETERS

### -Comparison
Gets or sets how existing files are compared.

```yaml
Type: TransferettoSyncComparison
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Always, Size, LastWriteTime, SizeOrLastWriteTime

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Direction
Gets or sets the synchronization direction.

```yaml
Type: TransferettoSyncDirection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Upload, Download

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DryRun
Gets or sets a value indicating whether planned operations are returned without changing files.

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

### -Exclude
Gets or sets wildcard exclude patterns matched against relative paths and names.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Include
Gets or sets wildcard include patterns matched against relative paths and names.

```yaml
Type: String[]
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
Gets or sets the local directory path.

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

### -Mode
Gets or sets whether synchronization updates destination items or mirrors deletes too.

```yaml
Type: TransferettoSyncMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Update, Mirror

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCreateDirectories
Gets or sets a value indicating whether missing destination directories should not be created.

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

### -NoOverwrite
Gets or sets a value indicating whether changed existing files should not be overwritten.

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

### -NoPreserveTimestamps
Gets or sets a value indicating whether timestamps should not be preserved after transfers.

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
Gets or sets the remote SFTP directory path.

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
Gets or sets the SFTP session.

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

### -TimestampToleranceSeconds
Gets or sets the timestamp tolerance in seconds for timestamp comparisons.

```yaml
Type: Int32
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
