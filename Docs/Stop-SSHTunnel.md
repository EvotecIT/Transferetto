---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Stop-SSHTunnel
## SYNOPSIS
Stops an SSH tunnel session.

Closes a tunnel created by Start-SSHLocalTunnel or Start-SSHRemoteTunnel, releasing the forwarded port cleanly.

## SYNTAX
### __AllParameterSets
```powershell
Stop-SSHTunnel -TunnelSession <TransferettoSshTunnelSession> [<CommonParameters>]
```

## DESCRIPTION
Stops an SSH tunnel session.

Closes a tunnel created by Start-SSHLocalTunnel or Start-SSHRemoteTunnel, releasing the forwarded port cleanly.

## EXAMPLES

### EXAMPLE 1
```powershell
Stop-SSHTunnel -TunnelSession 'Value'
```


## PARAMETERS

### -TunnelSession
Gets or sets the tunnel Session.

```yaml
Type: TransferettoSshTunnelSession
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

- `Transferetto.TransferettoSshTunnelSession`

## OUTPUTS

- `None`

## RELATED LINKS

- None
