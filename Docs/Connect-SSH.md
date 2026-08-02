---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Connect-SSH
## SYNOPSIS
Creates a reusable SSH session for one-shot commands, interactive shells, and SSH tunnels.

Supports password, PSCredential, and private-key authentication together with keyboard-interactive auth, TOFU or known-hosts validation, retry and keepalive settings, and SSH proxy configuration for shell, tunnel, and command-based workflows.

## SYNTAX
### Password (Default)
```powershell
Connect-SSH -Server <string> -Credential <pscredential> [-Port <int>] [-PrivateKeyPassphrase <string>] [-KeyboardInteractive] [-AcceptAnyHostKey] [-ExpectedHostKeyFingerprint <string[]>] [-HostKeyPolicy <TransferettoSshHostKeyPolicy>] [-KnownHostsPath <string>] [-KeepAliveIntervalSeconds <int>] [-ConnectionTimeoutSeconds <int>] [-RetryAttempts <int>] [-ProxyType <TransferettoSshProxyType>] [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [<CommonParameters>]
```

### ClearText
```powershell
Connect-SSH -Server <string> -Username <string> -Password <string> [-Port <int>] [-PrivateKeyPassphrase <string>] [-KeyboardInteractive] [-AcceptAnyHostKey] [-ExpectedHostKeyFingerprint <string[]>] [-HostKeyPolicy <TransferettoSshHostKeyPolicy>] [-KnownHostsPath <string>] [-KeepAliveIntervalSeconds <int>] [-ConnectionTimeoutSeconds <int>] [-RetryAttempts <int>] [-ProxyType <TransferettoSshProxyType>] [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [<CommonParameters>]
```

### PrivateKey
```powershell
Connect-SSH -Server <string> -Username <string> -PrivateKey <string> [-Port <int>] [-PrivateKeyPassphrase <string>] [-KeyboardInteractive] [-AcceptAnyHostKey] [-ExpectedHostKeyFingerprint <string[]>] [-HostKeyPolicy <TransferettoSshHostKeyPolicy>] [-KnownHostsPath <string>] [-KeepAliveIntervalSeconds <int>] [-ConnectionTimeoutSeconds <int>] [-RetryAttempts <int>] [-ProxyType <TransferettoSshProxyType>] [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [<CommonParameters>]
```

## DESCRIPTION
Creates a reusable SSH session for one-shot commands, interactive shells, and SSH tunnels.

Supports password, PSCredential, and private-key authentication together with keyboard-interactive auth, TOFU or known-hosts validation, retry and keepalive settings, and SSH proxy configuration for shell, tunnel, and command-based workflows.

## EXAMPLES

### EXAMPLE 1
```powershell
Connect-SSH -Server 'Value' -Credential Get-Credential
```


### EXAMPLE 2
```powershell
Connect-SSH -Server 'Value' -Username 'Name' -Password 'Value'
```


### EXAMPLE 3
```powershell
Connect-SSH -Server 'Value' -Username 'Name' -PrivateKey 'Value'
```


## PARAMETERS

### -AcceptAnyHostKey
Gets or sets the accept Any Host Key.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionTimeoutSeconds
Gets or sets the connection Timeout Seconds.

```yaml
Type: Int32
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Credential
Gets or sets the credential used by the cmdlet.

```yaml
Type: PSCredential
Parameter Sets: Password
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpectedHostKeyFingerprint
Gets or sets the expected Host Key Fingerprint.

```yaml
Type: String[]
Parameter Sets: Password, ClearText, PrivateKey
Aliases: HostKeyFingerprint
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HostKeyPolicy
Gets or sets the host Key Policy.

```yaml
Type: TransferettoSshHostKeyPolicy
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values: Loose, KnownHosts, TrustOnFirstUse

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeepAliveIntervalSeconds
Gets or sets the keep Alive Interval Seconds.

```yaml
Type: Int32
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeyboardInteractive
Gets or sets the keyboard Interactive.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KnownHostsPath
Gets or sets the known Hosts Path.

```yaml
Type: String
Parameter Sets: Password, ClearText, PrivateKey
Aliases: KnownHostsFile
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
Gets or sets the password.

```yaml
Type: String
Parameter Sets: ClearText
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Port
Gets or sets the network port.

```yaml
Type: Int32
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivateKey
Gets or sets the private Key.

```yaml
Type: String
Parameter Sets: PrivateKey
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivateKeyPassphrase
Gets or sets the private Key Passphrase.

```yaml
Type: String
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyCredential
Gets or sets the credential used by the cmdlet.

```yaml
Type: PSCredential
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyHost
Gets or sets the proxy Host.

```yaml
Type: String
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyPort
Gets or sets the proxy Port.

```yaml
Type: Int32
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyType
Gets or sets the proxy Type.

```yaml
Type: TransferettoSshProxyType
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values: None, Http, Socks4, Socks5

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetryAttempts
Gets or sets the retry Attempts.

```yaml
Type: Int32
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Server
Gets or sets the server name or address.

```yaml
Type: String
Parameter Sets: Password, ClearText, PrivateKey
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Gets or sets the username.

```yaml
Type: String
Parameter Sets: ClearText, PrivateKey
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
