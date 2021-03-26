function Request-FTPConfiguration {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
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
        [switch] $FirstOnly
    )
    $Client = [FluentFTP.FtpClient]::new($Server)
    if ($Username -and $Password) {
        $Client.Credentials = [System.Net.NetworkCredential]::new($Username, $Password)
    } elseif ($Credential) {
        $Client.Credentials = [System.Net.NetworkCredential]::new($Credential.Username, $Credential.Password)
    } else {
        # anonymous
    }
    try {
        $Client.AutoDetect($FirstOnly.IsPresent)
    } catch {
        $Client | Add-Member -Name 'Error' -Value $($_.Exception.Message) -Force -MemberType NoteProperty
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Request-FTPConfiguration - Error: $($_.Exception.Message)"
        }
    }
}