function Add-PrivateFTPFiles {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath,
        [string[]] $LocalPath,
        [System.IO.FileInfo[]] $LocalFile,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [FluentFTP.FtpError] $ErrorHandling = [FluentFTP.FtpError]::None,
        [switch] $CreateRemoteDirectory
    )
    try {
        #int UploadFiles(System.Collections.Generic.IEnumerable[string] localPaths, string remoteDir, FluentFTP.FtpRemoteExists existsMode, bool createRemoteDir, FluentFTP.FtpVerify verifyOptions, FluentFTP.FtpError errorHandling, System.Action[FluentFTP.FtpProgress] progress)
        #int UploadFiles(System.Collections.Generic.IEnumerable[System.IO.FileInfo] localFiles, string remoteDir, FluentFTP.FtpRemoteExists existsMode, bool createRemoteDir, FluentFTP.FtpVerify verifyOptions, FluentFTP.FtpError errorHandling, System.Action[FluentFTP.FtpProgress] progress)
        if ($LocalFile) {
            $Message = $Client.UploadFiles([System.IO.FileInfo[]] $LocalFile, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions, $ErrorHandling)
        } else {
            $Message = $Client.UploadFiles([string[]] $LocalPath, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions, $ErrorHandling)
        }
        if ($Message -gt 1) {
            $State = $true
        } else {
            $State = $false
        }
        $Status = [PSCustomObject] @{
            Action     = 'UploadFile'
            LocalPath  = $LocalPath
            RemotePath = $RemotePath
            Status     = $State
            Message    = $Message
        }
    } catch {
        $Status = [PSCustomObject] @{
            Action     = 'UploadFile'
            LocalPath  = $LocalPath
            RemotePath = $RemotePath
            Status     = $false
            Message    = "Error: $($_.Exception.Message)"
        }
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Add-PrivateFTPFiles - Error: $($_.Exception.Message)"
        }
    }
    $Status
}