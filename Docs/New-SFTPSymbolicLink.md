---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# New-SFTPSymbolicLink
## SYNOPSIS
Creates a symbolic link on an SFTP server.

Creates a remote symlink from the requested link path to the target path and can suppress the returned operation result for quieter automation.

## SYNTAX
### __AllParameterSets
```powershell
New-SFTPSymbolicLink -SftpClient <TransferettoSftpSession> -TargetPath <string> -LinkPath <string> [-Suppress] [<CommonParameters>]
```

## DESCRIPTION
Creates a symbolic link on an SFTP server.

Creates a remote symlink from the requested link path to the target path and can suppress the returned operation result for quieter automation.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SFTPSymbolicLink -SftpClient 'Value' -TargetPath 'C:\Path' -LinkPath 'C:\Path'
```


## PARAMETERS

### -LinkPath
Gets or sets the link Path.

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

### -Suppress
Gets or sets the suppress.

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

### -TargetPath
Gets or sets the target Path.

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
