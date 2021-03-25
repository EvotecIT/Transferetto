function Add-SFTPItem {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [string] $RemotePath,
        [string] $LocalPath,
        [switch] $AllowOverride
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $FileStream = [System.IO.FileStream]::new($LocalPath, [System.IO.FileMode]::OpenOrCreate)
            $SftpClient.UploadFile($FileStream, $RemotePath, $AllowOverride)
        } catch {
            $Status = 'Error'
            if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                Write-Error $_
                return
            } else {
                Write-Warning "Add-SFTPItem - Error: $($_.Exception.Message)"
            }
        } finally {
            $FileStream.Close()
        }
        $Status
    }
}