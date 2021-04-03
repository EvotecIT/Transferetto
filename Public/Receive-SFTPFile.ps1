function Receive-SFTPFile {
    [alias('Get-SFTPFile')]
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [string] $RemotePath,
        [string] $LocalPath
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $FileStream = [System.IO.FileStream]::new($LocalPath, [System.IO.FileMode]::OpenOrCreate)
            $SftpClient.DownloadFile($RemotePath, $FileStream)
            $Status = [PSCustomObject] @{
                Action     = 'DownloadFile'
                Status     = $true
                LocalPath  = $LocalPath
                RemotePath = $RemotePath
                Message    = ""
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action     = 'DownloadFile'
                Status     = $false
                LocalPath  = $LocalPath
                RemotePath = $RemotePath
                Message    = "Error: $($_.Exception.Message)"
            }
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Receive-SFTPFile - Error: $($_.Exception.Message)"
            }
        } finally {
            $FileStream.Close()
            if ($Status.Status -eq $false) {
                Remove-Item -LiteralPath $LocalPath
            }
        }
        $Status
    }
}