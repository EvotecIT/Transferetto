function Rename-SFTPFile {
    <#
    .SYNOPSIS
    Allows renaming remote file over SFTP protocol

    .DESCRIPTION
    Allows renaming remote file over SFTP protocol

    .PARAMETER SftpClient
    Parameter that contains the SFTP client object

    .PARAMETER OldRemotePath
    Path to file that is going to be renamed

    .PARAMETER NewRemotePath
    New path to file, with new name

    .PARAMETER Suppress
    Suppress returning an object with information about the operation

    .EXAMPLE
    $SftpClient = Connect-SFTP -Server '192.168.240.29' -Username 'przemek' -Password 'Password'
    Rename-SFTPFile -SftpClient $SftpClient -OldRemotePath '/home/przemek/mmm.txt' -NewRemotePath '/home/przemek/mmm1.txt'
    Disconnect-SFTP -SftpClient $SftpClient

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [alias('OldPath')][string] $OldRemotePath,
        [alias('NewPaht')][string] $NewRemotePath,
        [switch] $Suppress
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $SftpClient.RenameFile($OldRemotePath, $NewRemotePath)
            $Status = [PSCustomObject] @{
                Action  = 'RenameFile'
                Status  = $true
                OldPath = $OldRemotePath
                NewPath = $NewRemotePath
                Message = ""
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action  = 'RenameFile'
                Status  = $false
                OldPath = $OldRemotePath
                NewPath = $NewRemotePath
                Message = "Error $($_.Exception.Message)"
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