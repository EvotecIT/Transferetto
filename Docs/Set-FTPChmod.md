---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-FTPChmod
## SYNOPSIS
Sets POSIX-style permissions for a remote FTP item.

Supports both octal-style integer permission values and explicit owner/group/other permission flags, depending on which representation is more convenient for the caller.

## SYNTAX
### ByInt (Default)
```powershell
Set-FTPChmod -Client <TransferettoFtpSession> -RemotePath <string> -Permissions <int> [<CommonParameters>]
```

### Explicit
```powershell
Set-FTPChmod -Client <TransferettoFtpSession> -RemotePath <string> -Owner <FtpPermission> -Group <FtpPermission> -Other <FtpPermission> [<CommonParameters>]
```

## DESCRIPTION
Sets POSIX-style permissions for a remote FTP item.

Supports both octal-style integer permission values and explicit owner/group/other permission flags, depending on which representation is more convenient for the caller.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-FTPChmod -Client 'Value' -RemotePath 'C:\Path' -Permissions 1
```


### EXAMPLE 2
```powershell
Set-FTPChmod -Client 'Value' -RemotePath 'C:\Path' -Owner 'Value' -Group 'Value' -Other 'Value'
```


## PARAMETERS

### -Client
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoFtpSession
Parameter Sets: ByInt, Explicit
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Group
Gets or sets the group.

```yaml
Type: FtpPermission
Parameter Sets: Explicit
Aliases: None
Possible values: None, Execute, Write, Read

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Other
Gets or sets the other.

```yaml
Type: FtpPermission
Parameter Sets: Explicit
Aliases: None
Possible values: None, Execute, Write, Read

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Owner
Gets or sets the owner.

```yaml
Type: FtpPermission
Parameter Sets: Explicit
Aliases: None
Possible values: None, Execute, Write, Read

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Permissions
Gets or sets the permissions.

```yaml
Type: Int32
Parameter Sets: ByInt
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePath
Gets or sets the remote Path.

```yaml
Type: String
Parameter Sets: ByInt, Explicit
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
