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
            $Status = $Client.DownloadFile($LocalPath, $RemotePath, $LocalExists, $VerifyOptions)
        } catch {
            $Status = 'Error'
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-FTPFile - Error: $($_.Exception.Message)"
            }
        }
        $Status
        <#
        if ($Status -eq 'Success') {
            $true
        } elseif ($Status -eq 'Failed') {
            $false
        } elseif ($Status -eq 'Error') {
            $false
        } else {
            $Status
        }
        #>
    }
}