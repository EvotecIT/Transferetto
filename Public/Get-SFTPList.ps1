function Get-SFTPList {
    [cmdletBinding()]
    param(
        [alias('FtpPath')][string] $Path,
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient
    )
    if ($SftpClient -and $SftpClient.IsConnected -and -not $SftpClient.Error) {
        try {
            if ($Path) {
                $SftpClient.ListDirectory($Path)
            } else {
                $SftpClient.ListDirectory('')
            }
        } catch {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-SFTPList - Error: $($_.Exception.Message)"
            }
        }
    } else {
        Write-Warning "Get-SFTPList - Skipped (IsConnected $($SftpClient.IsConnected) / Error: $($SftpClient.Error))"
    }
}