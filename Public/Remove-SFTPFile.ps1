function Remove-SFTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [string] $RemotePath,
        [switch] $Suppress
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $SftpClient.DeleteFile($RemotePath)
            $Status = [PSCustomObject] @{
                Action  = 'RemoveFile'
                Status  = $true
                Message = ""
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action  = 'RemoveFile'
                Status  = $false
                Message = "Error $($_.Exception.Message)"
            }
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Remove-SFTPFile - Error: $($_.Exception.Message)"
            }
        }
        if (-not $Suppress) {
            $Status
        }
    }
}