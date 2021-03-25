function Disconnect-SFTP {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $SftpClient.Disconnect()
        } catch {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Disconnect-SFTP - Error: $($_.Exception.Message)"
            }
        }
    }
}