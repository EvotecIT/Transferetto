function Connect-FTP {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
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
    if ($FtpProfile) {
        $Client = [FluentFTP.FtpClient]::new()
        $Client.LoadProfile($FtpProfile)
    } else {
        $Client = [FluentFTP.FtpClient]::new($Server)
        if ($Username -and $Password) {
            $Client.Credentials = [System.Net.NetworkCredential]::new($Username, $Password)
        } elseif ($Credential) {
            $Client.Credentials = [System.Net.NetworkCredential]::new($Credential.Username, $Credential.Password)
        } else {
            # anonymous
        }
    }
    if ($Port) {
        $Client.Port = $Port
    }
    if ($DataConnectionType) {
        $Client.DataConnectionType = $DataConnectionType
    }
    if ($DisableDataConnectionEncryption) {
        $Client.DataConnectionEncryption = $false
    }
    if ($EncryptionMode) {
        $Client.EncryptionMode = $EncryptionMode
    }
    if ($ValidateAnyCertificate) {
        $Client.ValidateAnyCertificate = $true
    }
    if ($DisableValidateCertificateRevocation) {
        $Client.ValidateCertificateRevocation = $false
    }
    if ($SendHost) {
        $Client.SendHost = $true
    }
    if ($SocketKeepAlive) {
        $Client.SocketKeepAlive = $true
    }
    if ($FtpsBuffering) {
        $Client.SslBuffering = $SslBuffering
    }
    try {
        if ($AutoConnect) {
            $TempFtpProfile = $Client.AutoConnect()
            if ($TempFtpProfile -and $Client.IsConnected) {
                Write-Verbose "Following options where used to autoconnect: "
                foreach ($Name in $TempFtpProfile.PSObject.Properties.Name) {
                    Write-Verbose "[x] $Name -> $($TempFtpProfile.$Name)"
                }
            }
        } else {
            $Client.Connect()
        }
        $Client | Add-Member -Name 'Error' -Value $null -Force -MemberType NoteProperty
    } catch {
        $Client | Add-Member -Name 'Error' -Value $($_.Exception.Message) -Force -MemberType NoteProperty
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Connect-FTP - Error: $($_.Exception.Message)"
        }
    }
    $Client
}