function Compare-FTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [Parameter(Mandatory)][string] $LocalPath,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpCompareOption] $CompareOption = [FluentFTP.FtpCompareOption]::Auto

    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        $Client.CompareFile($LocalPath, $RemotePath, $CompareOption)
    }
}