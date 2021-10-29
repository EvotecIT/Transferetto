function Rename-SFTPFile {
    <#
    .SYNOPSIS
    Allows renaming remote file over SFTP protocol

    .DESCRIPTION
    Allows renaming remote file over SFTP protocol

    .PARAMETER SftpClient
    Parameter that contains the SFTP client object

    .PARAMETER SourcePath
    Path to file that is going to be renamed

    .PARAMETER DestinationPath
    New path to file, with new name

    .PARAMETER Suppress
    Suppress returning an object with information about the operation

    .EXAMPLE
    $SftpClient = Connect-SFTP -Server '192.168.240.29' -Username 'przemek' -Password 'Password'
    Rename-SFTPFile -SftpClient $SftpClient -SourcePath '/home/przemek/mmm.txt' -DestinationPath '/home/przemek/mmm1.txt'
    Disconnect-SFTP -SftpClient $SftpClient

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SftpClient] $SftpClient,
        [alias('OldPath')][string] $SourcePath,
        [alias('NewPath')][string] $DestinationPath,
        [switch] $Suppress
    )
    if ($SftpClient -and $SftpClient.IsConnected) {
        try {
            $SftpClient.RenameFile($SourcePath, $DestinationPath)
            $Status = [PSCustomObject] @{
                Action  = 'RenameFile'
                Status  = $true
                OldPath = $SourcePath
                NewPath = $DestinationPath
                Message = ""
            }
        } catch {
            $Status = [PSCustomObject] @{
                Action  = 'RenameFile'
                Status  = $false
                OldPath = $SourcePath
                NewPath = $DestinationPath
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