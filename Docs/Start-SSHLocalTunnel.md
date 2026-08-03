---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Start-SSHLocalTunnel
## SYNOPSIS
Starts a local SSH port-forwarding tunnel.

Binds a local host and port, then forwards traffic through the SSH session to a remote host and port, returning a reusable tunnel session that can be stopped later.

## SYNTAX
### __AllParameterSets
```powershell
Start-SSHLocalTunnel -SshClient <TransferettoSshSession> -BoundPort <uint> -RemoteHost <string> -RemotePort <uint> [-BoundHost <string>] [<CommonParameters>]
```

## DESCRIPTION
Starts a local SSH port-forwarding tunnel.

Binds a local host and port, then forwards traffic through the SSH session to a remote host and port, returning a reusable tunnel session that can be stopped later.

## EXAMPLES

### EXAMPLE 1
```powershell
Start-SSHLocalTunnel -SshClient 'Value' -BoundPort 1 -RemoteHost 'Value' -RemotePort 1
```


## PARAMETERS

### -BoundHost
Gets or sets the bound Host.

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

### -BoundPort
Gets or sets the bound Port.

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

### -RemoteHost
Gets or sets the remote Host.

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

### -RemotePort
Gets or sets the remote Port.

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

### -SshClient
Gets or sets the session object used by the cmdlet.

```yaml
Type: TransferettoSshSession
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSshSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
