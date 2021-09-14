﻿function Connect-SSH {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [Parameter(Mandatory, ParameterSetName = 'Password')]
        [Parameter(Mandatory, ParameterSetName = 'PrivateKey')]
        [string] $Server,

        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [Parameter(Mandatory, ParameterSetName = 'PrivateKey')]
        [string] $Username,

        [Parameter(Mandatory, ParameterSetName = 'ClearText')]
        [string] $Password,

        [Parameter(Mandatory, ParameterSetName = 'Password')]
        [pscredential] $Credential,
        
        [Parameter(Mandatory, ParameterSetName = 'PrivateKey')]
        [string] $PrivateKey,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [Parameter(ParameterSetName = 'PrivateKey')]
        [int] $Port
    )

    if ($Username -and $Password) {
        $SshClient = [Renci.SshNet.SshClient]::new($Server, $Username, $Password)
    } elseif ($Credential) {
        $SshClient = [Renci.SshNet.SshClient]::new($Server, $Credential.Username, $Credential.GetNetworkCredential().Password)
    } elseif ($PrivateKey) {
        [string]$PrivateKey = Resolve-Path $PrivateKey | Select-Object -ExpandProperty ProviderPath
        $SshClient = [Renci.SshNet.SshClient]::new($Server, $Username, [Renci.SshNet.PrivateKeyFile]$PrivateKey )
    } else {
        throw 'Not implemented and unexpected.'
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
