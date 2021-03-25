function Get-FTPList {
    [cmdletBinding()]
    param(
        [alias('FtpPath')][string] $Path,
        [FluentFTP.FtpListOption] $Options,
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        try {
            if ($Path -and $Options) {
                $Client.GetListing($Path, $Options)
            } elseif ($Path) {
                $Client.GetListing($Path)
            } else {
                $Client.GetListing()
            }
        } catch {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-FTPList - Error: $($_.Exception.Message)"
            }
        }
    } else {
        Write-Warning "Get-FTPList - Skipped (IsConnected $($Client.IsConnected) / Error: $($Client.Error))"
    }
}