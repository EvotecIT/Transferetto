---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Start-SSHRemoteTunnel
## SYNOPSIS
Starts a remote SSH port-forwarding tunnel.

Requests the SSH server to bind a remote host and port, then forwards traffic back through the SSH session to a target host and port reachable from the client side.

## SYNTAX
### __AllParameterSets
```powershell
Start-SSHRemoteTunnel -SshClient <TransferettoSshSession> -BoundPort <uint> -TargetHost <string> -TargetPort <uint> [-BoundHost <string>] [<CommonParameters>]
```

## DESCRIPTION
Starts a remote SSH port-forwarding tunnel.

Requests the SSH server to bind a remote host and port, then forwards traffic back through the SSH session to a target host and port reachable from the client side.

## EXAMPLES

### EXAMPLE 1
```powershell
Start-SSHRemoteTunnel -SshClient 'Value' -BoundPort 1 -TargetHost 'Value' -TargetPort 1
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

### -TargetHost
Gets or sets the target Host.

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

### -TargetPort
Gets or sets the target Port.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `Transferetto.TransferettoSshSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
