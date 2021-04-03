function Set-FTPOption {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [nullable[int]] $RetryAttempts,
        [nullable[bool]] $DownloadZeroByteFiles
    )
    if ($RetryAttempts) {
        $Client.RetryAttempts = $RetryAttempts
    }
    if ($DownloadZeroByteFiles) {
        $Client.DownloadZeroByteFiles = $DownloadZeroByteFiles
    }
}