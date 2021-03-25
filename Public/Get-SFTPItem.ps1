function Get-SFTPItem {
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
        } catch {
            $Status = 'Error'
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Get-SFTPItem - Error: $($_.Exception.Message)"
            }
        } finally {
            $FileStream.Close()
            if ($Status -eq 'Error') {
                Remove-Item -LiteralPath $LocalPath
            }
        }
        $Status
    }
}