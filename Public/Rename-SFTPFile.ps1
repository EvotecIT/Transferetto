function Rename-SFTPFile {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [string] $RemotePath,
        [switch] $Suppress
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $SftpClient.RenameFile($RemotePath)
            $Status = [PSCustomObject] @{
                Action     = 'RenameFile'
                LocalPath  = ''
                RemotePath = $RemotePath
                Status     = $true
                Message    = ""
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action     = 'RenameFile'
                Status     = $false
                LocalPath  = ''
                RemotePath = $RemotePath
                Message    = "Error $($_.Exception.Message)"
            }
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Rename-SFTPFile - Error: $($_.Exception.Message)"
            }
        }
        if (-not $Suppress) {
            $Status
        }
    }
}