function Disconnect-FTP {
    [cmdletBinding()]
    param(
        [FluentFTP.FtpClient] $Client
    )
    if ($Client -and $Client.IsConnected) {
        try {
            $Client.Disconnect()
        } catch {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Disconnect-FTP - Error: $($_.Exception.Message)"
            }
        }
    }
}