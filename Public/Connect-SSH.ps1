function Connect-SSH {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [Parameter(Mandatory, ParameterSetName = 'Password')]
        [string] $Server,

        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [string] $Username,

        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [string] $Password,

        [Parameter(Mandatory, ParameterSetName = 'Password')]
        [pscredential] $Credential,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [int] $Port
    )

    if ($Username -and $Password) {
        $SshClient = [Renci.SshNet.SshClient]::new($Server, $Username, $Password)
    } elseif ($Credential) {
        $SshClient = [Renci.SshNet.SshClient]::new($Server, $Credential.Username, $Credential.GetNetworkCredential().Password)
    } else {
        throw 'Not implemented. Add certificate'
    }
    if ($Port) {
        $SshClient.Port = $Port
    }

    try {
        $SshClient.Connect()
        $SshClient | Add-Member -Name 'Error' -Value $null -Force -MemberType NoteProperty
    } catch {
        $SshClient | Add-Member -Name 'Error' -Value $($_.Exception.Message) -Force -MemberType NoteProperty
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Connect-SSH - Error: $($_.Exception.Message)"
        }
    }
    $SshClient
}