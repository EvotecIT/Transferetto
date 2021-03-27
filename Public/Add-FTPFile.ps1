﻿function Add-FTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $RemotePath,
        [string] $LocalPath,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [switch] $CreateRemoteDirectory
    )
    if ($Client -and $Client.IsConnected) {
        try {
            $Message = $Client.UploadFile($LocalPath, $RemotePath, $RemoteExists, $CreateRemoteDirectory.IsPresent, $VerifyOptions)
            if ($Message -eq 'success') {
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
                Write-Warning "Get-FTPFile - Error: $($_.Exception.Message)"
            }
        }
        $Status
    }
}