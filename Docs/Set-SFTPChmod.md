---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SFTPChmod
## SYNOPSIS
Sets POSIX-style permissions for a remote SFTP item.

Supports symbolic permission strings or explicit owner/group/other digit values and can optionally return refreshed item metadata after the change.

## SYNTAX
### ByString (Default)
```powershell
Set-SFTPChmod -SftpClient <TransferettoSftpSession> -Path <string> -Permissions <string> [-PassThru] [<CommonParameters>]
```

### ByDigits
```powershell
Set-SFTPChmod -SftpClient <TransferettoSftpSession> -Path <string> -Owner <int> -Group <int> -Other <int> [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Sets POSIX-style permissions for a remote SFTP item.

Supports symbolic permission strings or explicit owner/group/other digit values and can optionally return refreshed item metadata after the change.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SFTPChmod -SftpClient 'Value' -Path 'C:\Path' -Permissions 'Value'
```


### EXAMPLE 2
```powershell
Set-SFTPChmod -SftpClient 'Value' -Path 'C:\Path' -Owner 1 -Group 1 -Other 1
```


## PARAMETERS

### -Group
Gets or sets the group.

```yaml
Type: Int32
Parameter Sets: ByDigits
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Other
Gets or sets the other.

```yaml
Type: Int32
Parameter Sets: ByDigits
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Owner
Gets or sets the owner.

```yaml
Type: Int32
Parameter Sets: ByDigits
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
Parameter Sets: ByString, ByDigits
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Gets or sets the path.

```yaml
Type: String
Parameter Sets: ByString, ByDigits
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Permissions
Gets or sets the permissions.

```yaml
Type: String
Parameter Sets: ByString
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
Parameter Sets: ByString, ByDigits
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
