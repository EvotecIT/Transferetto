---
external help file: Transferetto-help.xml
Module Name: Transferetto
online version: https://github.com/EvotecIT/Transferetto/blob/master/README.md
schema: 2.0.0
---
# Connect-FTP
## SYNOPSIS
Creates an FTP or FTPS session with runtime tuning, proxy support, and certificate trust controls.

Supports classic username/password and credential-based authentication, FluentFTP profiles, FTPS encryption modes, proxy settings, trust-on-first-use and known-certificate validation, plus transfer/runtime tuning that can be reused by later FTP and FTPS cmdlets.

## SYNTAX
### Password (Default)
```powershell
Connect-FTP [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [-ProxyUserName <string>] [-ProxyPassword <string>] [-ProxyType <TransferettoFtpProxyType>] [-Server <string>] [-Credential <pscredential>] [-EncryptionMode <FtpEncryptionMode[]>] [-DataConnectionType <FtpDataConnectionType>] [-SslBuffering <FtpsBuffering>] [-DisableDataConnectionEncryption] [-DisableValidateCertificateRevocation] [-ValidateAnyCertificate] [-UseGnuTls] [-ExpectedCertificateThumbprint <string[]>] [-CertificatePolicy <TransferettoFtpCertificatePolicy>] [-KnownCertificatesPath <string>] [-ConnectTimeout <int>] [-ReadTimeout <int>] [-DataConnectionConnectTimeout <int>] [-DataConnectionReadTimeout <int>] [-NoopInterval <int>] [-SslSessionLength <int>] [-EncryptAuthenticationOnly] [-SelfConnectMode <FtpSelfConnectMode>] [-RetryAttempts <int>] [-TransferChunkSize <int>] [-LocalFileBufferSize <int>] [-InternetProtocolVersions <FtpIpVersion>] [-UploadRateLimit <uint>] [-DownloadRateLimit <uint>] [-UploadDataType <FtpDataType>] [-DownloadDataType <FtpDataType>] [-ListingDataType <FtpDataType>] [-FXPDataType <FtpDataType>] [-FXPProgressInterval <int>] [-ActivePorts <int[]>] [-PassiveBlockedPorts <int[]>] [-PassiveMaxAttempts <int>] [-EncodingName <string>] [-Port <int>] [-SendHost] [-SocketKeepAlive] [-AutoConnect] [<CommonParameters>]
```

### FtpProfile
```powershell
Connect-FTP [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [-ProxyUserName <string>] [-ProxyPassword <string>] [-ProxyType <TransferettoFtpProxyType>] [-FtpProfile <FtpProfile>] [-UseGnuTls] [-ExpectedCertificateThumbprint <string[]>] [-CertificatePolicy <TransferettoFtpCertificatePolicy>] [-KnownCertificatesPath <string>] [-ConnectTimeout <int>] [-ReadTimeout <int>] [-DataConnectionConnectTimeout <int>] [-DataConnectionReadTimeout <int>] [-NoopInterval <int>] [-SslSessionLength <int>] [-EncryptAuthenticationOnly] [-SelfConnectMode <FtpSelfConnectMode>] [-RetryAttempts <int>] [-TransferChunkSize <int>] [-LocalFileBufferSize <int>] [-InternetProtocolVersions <FtpIpVersion>] [-UploadRateLimit <uint>] [-DownloadRateLimit <uint>] [-UploadDataType <FtpDataType>] [-DownloadDataType <FtpDataType>] [-ListingDataType <FtpDataType>] [-FXPDataType <FtpDataType>] [-FXPProgressInterval <int>] [-ActivePorts <int[]>] [-PassiveBlockedPorts <int[]>] [-PassiveMaxAttempts <int>] [-EncodingName <string>] [<CommonParameters>]
```

### ClearText
```powershell
Connect-FTP [-ProxyHost <string>] [-ProxyPort <int>] [-ProxyCredential <pscredential>] [-ProxyUserName <string>] [-ProxyPassword <string>] [-ProxyType <TransferettoFtpProxyType>] [-Server <string>] [-Username <string>] [-Password <string>] [-EncryptionMode <FtpEncryptionMode[]>] [-DataConnectionType <FtpDataConnectionType>] [-SslBuffering <FtpsBuffering>] [-DisableDataConnectionEncryption] [-DisableValidateCertificateRevocation] [-ValidateAnyCertificate] [-UseGnuTls] [-ExpectedCertificateThumbprint <string[]>] [-CertificatePolicy <TransferettoFtpCertificatePolicy>] [-KnownCertificatesPath <string>] [-ConnectTimeout <int>] [-ReadTimeout <int>] [-DataConnectionConnectTimeout <int>] [-DataConnectionReadTimeout <int>] [-NoopInterval <int>] [-SslSessionLength <int>] [-EncryptAuthenticationOnly] [-SelfConnectMode <FtpSelfConnectMode>] [-RetryAttempts <int>] [-TransferChunkSize <int>] [-LocalFileBufferSize <int>] [-InternetProtocolVersions <FtpIpVersion>] [-UploadRateLimit <uint>] [-DownloadRateLimit <uint>] [-UploadDataType <FtpDataType>] [-DownloadDataType <FtpDataType>] [-ListingDataType <FtpDataType>] [-FXPDataType <FtpDataType>] [-FXPProgressInterval <int>] [-ActivePorts <int[]>] [-PassiveBlockedPorts <int[]>] [-PassiveMaxAttempts <int>] [-EncodingName <string>] [-Port <int>] [-SendHost] [-SocketKeepAlive] [-AutoConnect] [<CommonParameters>]
```

## DESCRIPTION
Creates an FTP or FTPS session with runtime tuning, proxy support, and certificate trust controls.

Supports classic username/password and credential-based authentication, FluentFTP profiles, FTPS encryption modes, proxy settings, trust-on-first-use and known-certificate validation, plus transfer/runtime tuning that can be reused by later FTP and FTPS cmdlets.

## EXAMPLES

### EXAMPLE 1
```powershell
Connect-FTP -KnownCertificatesPath 'C:\Path'
```


## PARAMETERS

### -ActivePorts
Gets or sets active-mode data ports.

```yaml
Type: Int32[]
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AutoConnect
Gets or sets the auto Connect.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificatePolicy
Gets or sets the FTPS certificate validation policy.

```yaml
Type: TransferettoFtpCertificatePolicy
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: PolicyChain, KnownCertificates, TrustOnFirstUse

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectTimeout
Gets or sets the FTP control connection timeout, in milliseconds.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
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

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataConnectionConnectTimeout
Gets or sets the FTP data connection timeout, in milliseconds.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataConnectionReadTimeout
Gets or sets the FTP data socket read timeout, in milliseconds.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataConnectionType
Gets or sets the data Connection Type.

```yaml
Type: FtpDataConnectionType
Parameter Sets: Password, ClearText
Aliases: None
Possible values: AutoActive, AutoPassive, PASV, EPSV, PORT, EPRT, PASVEX, PassiveExtended, PASVUSE, PassiveAllowUnroutable

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisableDataConnectionEncryption
Gets or sets a value indicating whether disable Data Connection Encryption.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisableValidateCertificateRevocation
Gets or sets a value indicating whether disable Validate Certificate Revocation.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DownloadDataType
Gets or sets the data type used by high-level FTP downloads.

```yaml
Type: FtpDataType
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: ASCII, Binary, Unknown

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DownloadRateLimit
Gets or sets the download rate limit in kilobytes per second.

```yaml
Type: UInt32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EncodingName
Gets or sets the text encoding name used by the FTP control channel.

```yaml
Type: String
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EncryptAuthenticationOnly
Gets or sets a value indicating whether the CCC command should be used after authentication.

```yaml
Type: SwitchParameter
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EncryptionMode
Gets or sets the encryption Mode.

```yaml
Type: FtpEncryptionMode[]
Parameter Sets: Password, ClearText
Aliases: None
Possible values: None, Implicit, Explicit, Auto

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpectedCertificateThumbprint
Gets or sets expected FTPS certificate thumbprints.

```yaml
Type: String[]
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FtpProfile
Gets or sets the ftp Profile.

```yaml
Type: FtpProfile
Parameter Sets: FtpProfile
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FXPDataType
Gets or sets the data type used by FXP server-to-server transfers.

```yaml
Type: FtpDataType
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: ASCII, Binary, Unknown

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FXPProgressInterval
Gets or sets how often FXP progress is reported.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InternetProtocolVersions
Gets or sets the internet protocol versions allowed for FTP connections.

```yaml
Type: FtpIpVersion
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: Unknown, IPv4, IPv6, ANY

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KnownCertificatesPath
Gets or sets the known-certificate store path.

```yaml
Type: String
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ListingDataType
Gets or sets the data type used by FTP directory listings.

```yaml
Type: FtpDataType
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: ASCII, Binary, Unknown

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LocalFileBufferSize
Gets or sets the local file buffer size used by FTP transfers.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoopInterval
Gets or sets the FTP control connection NOOP interval, in milliseconds. Set to 0 to disable NOOPs.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassiveBlockedPorts
Gets or sets passive-mode ports to avoid.

```yaml
Type: Int32[]
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassiveMaxAttempts
Gets or sets the maximum number of passive-mode connection attempts.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
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

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Port
Gets or sets the network port.

```yaml
Type: Int32
Parameter Sets: Password, ClearText
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
Parameter Sets: Password, FtpProfile, ClearText
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
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyPassword
Gets or sets the proxy Password.

```yaml
Type: String
Parameter Sets: Password, FtpProfile, ClearText
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
Parameter Sets: Password, FtpProfile, ClearText
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
Type: TransferettoFtpProxyType
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: FtpClientSocks5Proxy, FtpClientHttp11Proxy, FtpClientSocks4aProxy, FtpClientSocks4Proxy, FtpClientUserAtHostProxy, FtpClientBlueCoatProxy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyUserName
Gets or sets the proxy User Name.

```yaml
Type: String
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReadTimeout
Gets or sets the FTP control socket read timeout, in milliseconds.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetryAttempts
Gets or sets the number of retry attempts for verified transfers.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelfConnectMode
Gets or sets how FluentFTP should reconnect when it needs a control connection.

```yaml
Type: FtpSelfConnectMode
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: Always, OnConnectionLost, Never

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SendHost
Gets or sets a value indicating whether send Host.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
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
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SocketKeepAlive
Gets or sets the socket Keep Alive.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SslBuffering
Gets or sets the ssl Buffering.

```yaml
Type: FtpsBuffering
Parameter Sets: Password, ClearText
Aliases: None
Possible values: Auto, Off, On

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SslSessionLength
Gets or sets the maximum number of FTPS control-channel transactions before the client reconnects.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TransferChunkSize
Gets or sets the number of bytes transferred in a single FTP transfer chunk.

```yaml
Type: Int32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UploadDataType
Gets or sets the data type used by high-level FTP uploads.

```yaml
Type: FtpDataType
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values: ASCII, Binary, Unknown

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UploadRateLimit
Gets or sets the upload rate limit in kilobytes per second.

```yaml
Type: UInt32
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseGnuTls
Gets or sets a value indicating whether the FluentFTP GnuTLS stream should be used for FTPS connections.

```yaml
Type: SwitchParameter
Parameter Sets: Password, FtpProfile, ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Gets or sets the username.

```yaml
Type: String
Parameter Sets: ClearText
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValidateAnyCertificate
Gets or sets a value indicating whether validate Any Certificate.

```yaml
Type: SwitchParameter
Parameter Sets: Password, ClearText
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
