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
    $ErrorFound = $null
    try {
        if ($LocalFile) {
            $Message = $Client.UploadFiles([System.IO.FileInfo[]] $LocalFile, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions, $ErrorHandling)
        } else {
            $Message = $Client.UploadFiles([string[]] $LocalPath, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions, $ErrorHandling)
        }
    } catch {
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Add-PrivateFTPFiles - Error: $($_.Exception.Message)"
        }
        $ErrorFound = $($_.Exception.Message)
    }
    if (-not $ErrorFound) {
        foreach ($M in $Message) {
            [PSCustomObject] @{
                Action          = 'UploadFile'
                Status          = $M.IsSuccess
                IsSuccess       = $M.IsSuccess
                IsSkipped       = $M.IsSkipped
                IsSkippedByRule = $M.IsSkippedByRule
                IsFailed        = $M.IsFailed
                LocalPath       = $M.LocalPath
                RemotePath      = $M.RemotePath
                Message         = if ($M.IsSkipped -eq $true) { "Skipped" } else { 'Success' }
            }
        }
    } else {
        [PSCustomObject] @{
            Action          = 'UploadFile'
            Status          = $false
            IsSuccess       = $false
            IsSkipped       = $false
            IsSkippedByRule = $false
            IsFailed        = $true
            LocalPath       = $LocalPath
            RemotePath      = $RemotePath
            Message         = $ErrorFound
        }
    }
}