function Add-PrivateFTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath,
        [string] $LocalPath,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [switch] $CreateRemoteDirectory
    )
    try {
        $Message = $Client.UploadFile($LocalPath, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions)
        if ($Message -eq 'success') {
            $State = $true
        } else {
            if ($Message -eq 'Skipped') {
                $State = $true
            } else {
                $State = $false
            }
        }
        $Status = [PSCustomObject] @{
            Action          = 'UploadFile'
            Status          = $State
            IsSuccess       = if ($State) { $true } else { $false }
            IsSkipped       = $Message -eq 'Skipped'
            IsSkippedByRule = $false
            IsFailed        = if ($State) { $false } else { $true }
            LocalPath       = $LocalPath
            RemotePath      = $RemotePath
            Message         = $Message
        }
    } catch {
        $Status = [PSCustomObject] @{
            Action          = 'UploadFile'
            Status          = $false
            IsSuccess       = $false
            IsSkipped       = $false
            IsSkippedByRule = $false
            IsFailed        = $true
            LocalPath       = $LocalPath
            RemotePath      = $RemotePath
            Message         = "Error: $($_.Exception.Message)"
        }
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error $_
            return
        } else {
            Write-Warning "Add-PrivateFTPFile - Error: $($_.Exception.Message)"
        }
    }
    $Status
}