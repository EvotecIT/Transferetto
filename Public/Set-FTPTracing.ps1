function Set-FTPTracing {
    <#
    .SYNOPSIS
    Allows enabling/disabling tracing ftp commands being sent and received during communucation with the server.

    .DESCRIPTION
    Allows enabling/disabling tracing ftp commands being sent and received during communucation with the server.

    .PARAMETER Enable
    Enable tracing

    .PARAMETER Disable
    Disable tracing

    .PARAMETER ShowPassword
    Include FTP passwords in logs? Default: false.

    .PARAMETER HideUserName
    Hide FTP usernames in logs? Default: false.

    .PARAMETER HideIP
    Hide server IP addresses in logs? Default: true.

    .EXAMPLE
    Set-FTPTracing -Enable

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [switch] $Enable,
        [switch] $Disable,
        [switch] $ShowPassword,
        [switch] $HideUserName,
        [switch] $HideIP
    )
    $Script:GlobalFTPLogging = [ordered] @{}
    if ($Enable) {
        $Script:GlobalFTPLogging.LogToConsole = $true
    } elseif ($Disable) {
        #$Script:GlobalFTPLogging.LogToConsole = $false
        $Script:GlobalFTPLogging = $null
        return
    } else {
        Write-Warning -Message 'Please specify either -Enable or -Disable'
        return
    }
    if ($HideUserName) {
        $Script:GlobalFTPLogging.LogUserName = $false; # hide FTP user names
    } else {
        $Script:GlobalFTPLogging.LogUserName = $true; # show FTP user names
    }
    if ($ShowPassword) {
        $Script:GlobalFTPLogging.LogPassword = $false; # hide FTP passwords
    } else {
        $Script:GlobalFTPLogging.LogPassword = $true; # show FTP passwords
    }
    if (-not $HideIP) {
        $Script:GlobalFTPLogging.LogHost = $true; # hide FTP server IP addresses
    } else {
        $Script:GlobalFTPLogging.LogHost = $false; # show FTP server IP addresses
    }
}