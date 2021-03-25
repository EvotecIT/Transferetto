function Connect-SFTP {
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
        [int] $Port
    )

    if ($Username -and $Password) {
        $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Username, $Password)
    } elseif ($Credential) {
        $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Credential.Username, $Credential.GetNetworkCredential().Password)
    } else {
        throw 'Not implemented. Add certificate'
    }
    if ($Port) {
        $SftpClient.Port = $Port
    }

    try {
        $SftpClient.Connect()
        $SftpClient | Add-Member -Name 'Error' -Value $null -Force -MemberType NoteProperty
    } catch {
        $SftpClient | Add-Member -Name 'Error' -Value $($_.Exception.Message) -Force -MemberType NoteProperty
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Connect-SFTP - Error: $($_.Exception.Message)"
        }
    }
    $SftpClient
}