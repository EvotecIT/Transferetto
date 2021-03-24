function Get-FTPItem {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath,
        [string] $LocalPath,
        [FluentFTP.FtpLocalExists] $LocalExists = [FluentFTP.FtpLocalExists]::Overwrite,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [FluentFTP.FtpError] $FtpError = [FluentFTP.FtpError]::Stop
    )

    if ($Client -and $Client.IsConnected) {



        try {
            #FluentFTP.FtpStatus DownloadFile(string localPath, string remotePath, FluentFTP.FtpLocalExists existsMode, FluentFTP.FtpVerify verifyOptions, System.Action[FluentFTP.FtpProgress] progress)
            #int DownloadFiles(string localDir, System.Collections.Generic.IEnumerable[string] remotePaths, FluentFTP.FtpLocalExists existsMode, FluentFTP.FtpVerify verifyOptions, FluentFTP.FtpError errorHandling, System.Action[FluentFTP.FtpProgress] progress)
            $Client.DownloadFile($LocalPath, $RemotePath, $LocalExists, $VerifyOptions)
        } catch {
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-FTPItem - Error: $($_.Exception.Message)"
            }
        }
    }
}