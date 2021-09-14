function Connect-SFTP {
    [cmdletBinding(DefaultParameterSetName = 'Password')]
    param(
        [Parameter(ParameterSetName = 'ClearText', Mandatory)]
        [Parameter(ParameterSetName = 'Password', Mandatory)]
        [Parameter(ParameterSetName = 'PrivateKey', Mandatory)]
        [string] $Server,

        [Parameter(ParameterSetName = 'ClearText', Mandatory)]
        [Parameter(ParameterSetName = 'PrivateKey', Mandatory)]
        [string] $Username,

        [Parameter(ParameterSetName = 'ClearText', Mandatory)]
        [string] $Password,

        [Parameter(ParameterSetName = 'Password', Mandatory)]
        [pscredential] $Credential,

        [Parameter(Mandatory, ParameterSetName = 'PrivateKey')]
        [string] $PrivateKey,

        [Parameter(ParameterSetName = 'ClearText')]
        [Parameter(ParameterSetName = 'Password')]
        [Parameter(ParameterSetName = 'PrivateKey')]
        [int] $Port
    )
    try {
        if ($Username -and $Password) {
            $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Username, $Password)
        } elseif ($Credential) {
            $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Credential.Username, $Credential.GetNetworkCredential().Password)
        } elseif ($PrivateKey) {
            if (Test-Path -LiteralPath $PrivateKey) {
                [string]$PrivateKey = Resolve-Path -LiteralPath $PrivateKey -ErrorAction Stop | Select-Object -ExpandProperty ProviderPath
                $SftpClient = [Renci.SshNet.SftpClient]::new($Server, $Username, [Renci.SshNet.PrivateKeyFile]$PrivateKey )
            } else {
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    throw "PrivateKey $PrivateKey doesn't exists."
                    return
                } else {
                    Write-Warning "Connect-SFTP - PrivateKey $PrivateKey doesn't exists."
                    return
                }
            }
        } else {
            throw 'Not implemented and unexepected.'
            return
        }
    } catch {
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Connect-SFTP - Error: $($_.Exception.Message)"
        }
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
