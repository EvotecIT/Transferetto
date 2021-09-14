function Connect-SFTP {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [Parameter(ParameterSetName = 'PrivateKey')]
        [string] $Server,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'PrivateKey')]
        [string] $Username,

        [Parameter(ParameterSetName = 'ClearText')]
        [string] $Password,

        [Parameter(ParameterSetName = 'Password')]
        [pscredential] $Credential,
        
        [Parameter(Mandatory, ParameterSetName = 'PrivateKey')]
        [string] $PrivateKey,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [Parameter(ParameterSetName = 'PrivateKey')]
        [int] $Port
    )

    if ($Username -and $Password) {
        $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Username, $Password)
    } elseif ($Credential) {
        $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Credential.Username, $Credential.GetNetworkCredential().Password)
    } elseif ($PrivateKey) {
        [string]$PrivateKey = Resolve-Path $PrivateKey | Select-Object -ExpandProperty ProviderPath
        $SftpClient = [Renci.SshNet.SshClient]::new($Server, $Username, [Renci.SshNet.PrivateKeyFile]$PrivateKey )
    } else {
        throw 'Not implemented and unexepected.'
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
