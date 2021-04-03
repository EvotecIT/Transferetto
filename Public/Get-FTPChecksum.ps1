function Get-FTPChecksum {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpHashAlgorithm] $HashAlgorithm = [FluentFTP.FtpHashAlgorithm]::MD5
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.GetChecksum($RemotePath, $HashAlgorithm)
    }
}