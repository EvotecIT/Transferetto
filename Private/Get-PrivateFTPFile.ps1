function Get-PrivateFTPFile {
    [cmdletBinding()]
    param(
        [string] $LocalPath,
        [FluentFTP.FtpListItem]  $RemoteFile,
        [string] $RemotePath,
        [FluentFTP.FtpLocalExists] $LocalExists,
        [FluentFTP.FtpVerify[]] $VerifyOptions,
        [FluentFTP.FtpError] $FtpError
    )
    if ($RemoteFile) {
        if ($RemoteFile.Type -eq 'File') {
            $FileToDownload = $RemoteFile.FullName
        } else {
            if (-not $Suppress) {
                return [PSCustomObject] @{
                    Action     = 'DownloadFile'
                    Status     = $false
                    LocalPath  = $LocalPath
                    RemotePath = $RemoteFile.FullName
                    Message    = "Get-FTPFile - Given path $($RemoteFile.FullName) is $($RemoteFile.Type). Skipping."
                }
            } else {
                Write-Warning "Get-FTPFile - Given path $($RemoteFile.FullName) is a directory. Skipping."
                return
            }
        }
    } else {
        $FileToDownload = $RemotePath
    }
    try {
        $Message = $Client.DownloadFile($LocalPath, $FileToDownload, $LocalExists, $VerifyOptions)
        if ($Message -eq 'success') {
            $State = $true
        } else {
            $State = $false
        }
        $Status = [PSCustomObject] @{
            Action     = 'DownloadFile'
            Status     = $State
            LocalPath  = $LocalPath
            RemotePath = $FileToDownload
            Message    = $Message
        }
    } catch {
        $Status = [PSCustomObject] @{
            Action     = 'DownloadFile'
            Status     = $false
            LocalPath  = $LocalPath
            RemotePath = $FileToDownload
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