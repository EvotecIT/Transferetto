function Add-SFTPItem {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [string] $RemotePath,
        [string] $LocalPath,
        [switch] $AllowOverride
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        if (Test-Path -LiteralPath $LocalPath) {
            try {
                $FileStream = [System.IO.FileStream]::new($LocalPath, [System.IO.FileMode]::OpenOrCreate)
                $SftpClient.UploadFile($FileStream, $RemotePath, $AllowOverride)
                $Status = [PSCustomObject] @{
                    Action     = 'UploadFile'
                    Status     = $true
                    LocalPath  = $LocalPath
                    RemotePath = $RemotePath
                    Message    = "Error: $($_.Exception.Message)"
                }
            } catch {
                $Status = [PSCustomObject] @{
                    Action     = 'UploadFile'
                    Status     = $false
                    LocalPath  = $LocalPath
                    RemotePath = $RemotePath
                    Message    = "Error: $($_.Exception.Message)"
                }
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    Write-Error $_
                    return
                } else {
                    Write-Warning "Add-SFTPItem - Error: $($_.Exception.Message)"
                }
            } finally {
                $FileStream.Close()
            }
        } else {
            Write-Warning "Add-SFTPItem - File $LocalPath doesn't exists."
            $Status = [PSCustomObject] @{
                Action     = 'UploadFile'
                Status     = $false
                LocalPath  = $LocalPath
                RemotePath = $RemotePath
                Message    = "LocalPath doesn't exists $LocalPath"
            }
        }
        $Status
    }
}