function Connect-FTP {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [string] $ProxyHost,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [int] $ProxyPort,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [pscredential] $ProxyCredential,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [string] $ProxyUserName,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [string] $ProxyPassword,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [ValidateSet(
            'FtpClientSocks5Proxy',
            'FtpClientHttp11Proxy',
            'FtpClientSocks4aProxy',
            'FtpClientSocks4Proxy',
            'FtpClientUserAtHostProxy',
            'FtpClientBlueCoatProxy'
        )][string] $ProxyType,

        [Parameter(ParameterSetName = 'FtpProfile')]
        [FluentFTP.FtpProfile] $FtpProfile,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [string] $Server,

        [Parameter(ParameterSetName = 'ClearText')]
        [string] $Username,

        [Parameter(ParameterSetName = 'ClearText')]
        [string] $Password,

        [Parameter(ParameterSetName = 'Password')]
        [pscredential] $Credential,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [FluentFTP.FtpEncryptionMode[]] $EncryptionMode,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [FluentFTP.FtpDataConnectionType] $DataConnectionType,


        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [FluentFTP.FtpsBuffering] $SslBuffering,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $DisableDataConnectionEncryption,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $DisableValidateCertificateRevocation,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $ValidateAnyCertificate,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [int] $Port,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $SendHost,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $SocketKeepAlive,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [switch] $AutoConnect
    )
    $ClientInternal = $null
    if ($PSBoundParameters.ContainsKey('ProxyHost') -or $PSBoundParameters.ContainsKey('ProxyPort') -or $PSBoundParameters.ContainsKey('ProxyType')) {
        if ($PSBoundParameters.ContainsKey('ProxyHost') -and $PSBoundParameters.ContainsKey('ProxyType')) {
            $ProxyProfile = [FluentFTP.FtpProxyProfile]::new()
            $ProxyProfile.ProxyHost = $ProxyHost
            $ProxyProfile.ProxyPort = $ProxyPort

            if ($ProxyUsername -and $ProxyPassword) {
                $ProxyProfile.ProxyCredentials = [System.Net.NetworkCredential]::new($Username, $Password)
            } elseif ($ProxyCredential) {
                $ProxyProfile.ProxyCredentials = [System.Net.NetworkCredential]::new($ProxyCredential.Username, $ProxyCredential.Password)
            } else {
                # anonymous
            }

            $ProxyProfile.FtpHost = $Server
            if ($Port) {
                $ProxyProfile.FtpPort = $Port
            }

            if ($Username -and $Password) {
                $ProxyProfile.FTPCredentials = [System.Net.NetworkCredential]::new($Username, $Password)
            } elseif ($Credential) {
                $ProxyProfile.FTPCredentials = [System.Net.NetworkCredential]::new($Credential.Username, $Credential.Password)
            } else {
                # anonymous
            }

            if ($ProxyType -eq 'FtpClientSocks5Proxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientSocks5Proxy]::new($ProxyProfile)
            } elseif ($ProxyType -eq 'FtpClientHttp11Proxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientHttp11Proxy]::new($ProxyProfile)
            } elseif ($ProxyType -eq 'FtpClientSocks4aProxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientSocks4aProxy]::new($ProxyProfile)
            } elseif ($ProxyType -eq 'FtpClientSocks4Proxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientSocks4Proxy]::new($ProxyProfile)
            } elseif ($ProxyType -eq 'FtpClientUserAtHostProxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientUserAtHostProxy]::new($ProxyProfile)
            } elseif ($ProxyType -eq 'FtpClientBlueCoatProxy') {
                $ClientInternal = [FluentFTP.Proxy.SyncProxy.FtpClientBlueCoatProxy]::new($ProxyProfile)
            }
        } else {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error "ProxyHost, ProxyPort, and ProxyType must be specified together when using Proxy. Only ProxyUserName, ProxyPassword or ProxyCredential are optional."
                return
            } else {
                Write-Warning "Connect-FTP - ProxyHost, ProxyPort, and ProxyType must be specified together when using Proxy. Only ProxyUserName, ProxyPassword or ProxyCredential are optional."
            }
        }
    }
    if ($FtpProfile) {
        if (-not $ClientInternal) {
            $ClientInternal = [FluentFTP.FtpClient]::new()
        }
        $ClientInternal.LoadProfile($FtpProfile)
    } else {
        if (-not $ClientInternal) {
            $ClientInternal = [FluentFTP.FtpClient]::new($Server)
        }
        if ($Username -and $Password) {
            $ClientInternal.Credentials = [System.Net.NetworkCredential]::new($Username, $Password)
        } elseif ($Credential) {
            $ClientInternal.Credentials = [System.Net.NetworkCredential]::new($Credential.Username, $Credential.Password)
        } else {
            # anonymous
        }
    }
    if ($Port) {
        $ClientInternal.Port = $Port
    }
    if ($DataConnectionType) {
        $ClientInternal.Config.DataConnectionType = $DataConnectionType
    }
    if ($DisableDataConnectionEncryption) {
        $ClientInternal.Config.DataConnectionEncryption = $false
    }
    if ($EncryptionMode) {
        $ClientInternal.Config.EncryptionMode = $EncryptionMode
    }
    if ($ValidateAnyCertificate) {
        $ClientInternal.Config.ValidateAnyCertificate = $true
    }
    if ($DisableValidateCertificateRevocation) {
        $ClientInternal.Config.ValidateCertificateRevocation = $false
    }
    if ($SendHost) {
        $ClientInternal.Config.SendHost = $true
    }
    if ($SocketKeepAlive) {
        $ClientInternal.Config.SocketKeepAlive = $true
    }
    if ($FtpsBuffering) {
        $ClientInternal.Config.SslBuffering = $SslBuffering
    }
    try {
        if ($AutoConnect) {
            $TempFtpProfile = $ClientInternal.AutoConnect()
            if ($TempFtpProfile -and $ClientInternal.IsConnected) {
                Write-Verbose "Following options where used to autoconnect: "
                foreach ($Name in $TempFtpProfile.PSObject.Properties.Name) {
                    Write-Verbose "[x] $Name -> $($TempFtpProfile.$Name)"
                }
            }
        } else {
            $ClientInternal.Connect()
        }
        $ClientInternal | Add-Member -Name 'Error' -Value $null -Force -MemberType NoteProperty
    } catch {
        $ClientInternal | Add-Member -Name 'Error' -Value $($_.Exception.Message) -Force -MemberType NoteProperty
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Connect-FTP - Error: $($_.Exception.Message)"
        }
    }
    $ClientInternal
}