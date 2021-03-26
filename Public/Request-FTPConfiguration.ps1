function Request-FTPConfiguration {
    <#
    .SYNOPSIS
    Short description

    .DESCRIPTION
    Automatically discover working FTP connection settings and return those connection profiles. This method will try every possible connection type combination in a loop until it finds a working combination, and it will return the first found combination or all found combinations. The connection types are tried in this order of preference.

    Auto connection attempts to find working connection settings in this order of preference:

    Protocol Preference:
    1. None - Let the OS decide which TLS/SSL version to use
    2. Tls12 - TLS 1.2 (TLS 1.3 is not yet stable in .NET Framework)
    3. Tls11 - TLS 1.1
    4. Tls - TLS 1.0
    5. Ssl3 - SSL 3.0 (obsolete, need to use TLS instead)
    6. Ssl2 - SSL 2.0 (obsolete, need to use TLS instead)
    7. Default - Undefined/weird behaviour

    Data Connection Type Preference:

    1. PASV - We prefer passive as its the most reliable
    2. EPSV - Enhanced passive is not as well supported on servers
    3. PORT - PORT is an older connection type
    4. EPRT - Enhanced PORT is not as well supported on servers
    5. PASVEX

    Encoding Type Preference:

    1. UTF8 - We prefer Unicode encoding as there will be no issues with file and folder names
    2. ASCII - ASCII/ANSI is a fallback used for older servers

    .PARAMETER Server
    Server Name or IP Address to Connect

    .PARAMETER Username
    UserName for FTP Connection

    .PARAMETER Password
    Password for FTP Connection (cleartext)

    .PARAMETER Credential
    UserName and Password in form of Credentials

    .PARAMETER FirstOnly
    Returns first working profile

    .EXAMPLE
    # Login via UserName/Password
    $ProfileFtp1 = Request-FTPConfiguration -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'
    $ProfileFtp1 | Format-Table

    .EXAMPLE
    # Anonymous login
    $ProfileFtp2 = Request-FTPConfiguration -Server 'speedtest.tele2.net' -Verbose
    $ProfileFtp2 | Format-Table

    .NOTES
    General notes
    #>
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