function Send-FTPDirectory {
    <#
    .SYNOPSIS
    Uploads a directory to an FTP server.

    .DESCRIPTION
    Uploads a directory to an FTP server.

    .PARAMETER Client
    The Client to use for connection.

    .PARAMETER LocalPath
    Path on the local machine to upload to FTP Server

    .PARAMETER RemotePath
    Path on the FTP Server where to upload the content

    .PARAMETER FolderSyncMode
    Update - upload a folder and all its files
    Mirror - upload a folder and all its files, and delete extra files on the server

    .PARAMETER RemoteExists
    Provide decision what to do when file on the server exits.
    Options available: Append, AppendNoChek, NoCheck, Skip, , Overwrite
    Default: Skip.

    .PARAMETER VerifyOptions
    Provide options for verification of files on the remote server.
    Options available: Delete, OnlyChecksum, None, Retry
    Default: None.

    .PARAMETER Rules
    Provide rules and conditions for uploading files.

    .EXAMPLE
    Set-FTPTracing -Enable -DisplayConsole

    $Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
    $Upload = Send-FTPDirectory -Client $Client -LocalPath $PSScriptRoot\Upload -RemotePath '/Temporary' -Verbose -FolderSyncMode Update
    $Upload | Format-Table *

    Disconnect-FTP -Client $Client

    Set-FTPTracing -Disable

    .NOTES
    General notes
    #>
    [alias('Add-FTPDirectory')]
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][FluentFTP.FtpClient] $Client,
        [string] $LocalPath,
        [Parameter(Mandatory)][string] $RemotePath,
        [FluentFTP.FtpFolderSyncMode] $FolderSyncMode = [FluentFTP.FtpFolderSyncMode]::Update,
        [FluentFTP.FtpRemoteExists] $RemoteExists = [FluentFTP.FtpRemoteExists]::Skip,
        [FluentFTP.FtpVerify] $VerifyOptions = [FluentFTP.FtpVerify]::None,
        [FluentFTP.Rules.FtpRule[]] $Rules
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        #$Client.UploadDirectory($LocalPath, $RemotePath, $FolderSyncMode)
        $Client.UploadDirectory($LocalPath, $RemotePath, $FolderSyncMode, $RemoteExists, $VerifyOptions, @($Rules))
    }
}