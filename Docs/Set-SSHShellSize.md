---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Set-SSHShellSize
## SYNOPSIS
Resizes the virtual terminal backing an interactive SSH shell session.

Updates terminal rows, columns, and optional pixel dimensions so interactive programs such as editors, pagers, or full-screen tools render correctly in the remote shell.

## SYNTAX
### __AllParameterSets
```powershell
Set-SSHShellSize -ShellSession <TransferettoSshShellSession> -Columns <uint> -Rows <uint> [-Width <uint>] [-Height <uint>] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Resizes the virtual terminal backing an interactive SSH shell session.

Updates terminal rows, columns, and optional pixel dimensions so interactive programs such as editors, pagers, or full-screen tools render correctly in the remote shell.

## EXAMPLES

### EXAMPLE 1
```powershell
Set-SSHShellSize -ShellSession 'Value' -Columns 1 -Rows 1
```


## PARAMETERS

### -Columns
Gets or sets the columns.

```yaml
Type: UInt32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Gets or sets the height.

```yaml
Type: UInt32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
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

### -Rows
Gets or sets the rows.

```yaml
Type: UInt32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShellSession
Gets or sets the shell Session.

```yaml
Type: TransferettoSshShellSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Gets or sets the width.

```yaml
Type: UInt32
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
