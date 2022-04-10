function Set-FTPTracing {
    <#
    .SYNOPSIS
    Allows enabling/disabling tracing ftp commands being sent and received during communucation with the server.

    .DESCRIPTION
    Allows enabling/disabling tracing ftp commands being sent and received during communucation with the server.

    .PARAMETER LogPath
    Set this to a file path to append all FTP communication to it. Default: false.

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

    .PARAMETER HideFunctions
    Hide high-level function calls in logs? Default: false.

    .PARAMETER DisplayConsole
    Should FTP communication be be logged to the console? Default: false.

    .EXAMPLE
    Set-FTPTracing -Enable -DisplayConsole

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [string] $LogPath,
        [switch] $Enable,
        [switch] $Disable,
        [switch] $ShowPassword,
        [switch] $HideUserName,
        [switch] $HideIP,
        [switch] $HideFunctions,
        [switch] $DisplayConsole
    )
    if ($Enable) {
        [FluentFTP.FtpTrace]::EnableTracing = $true
    }
    if ($Disable) {
        [FluentFTP.FtpTrace]::EnableTracing = $false
    }
    if ($LogPath) {
        [FluentFTP.FtpTrace]::LogToFile = $LogPath
    }

    if (-not $HideFunctions) {
        [FluentFTP.FtpTrace]::LogFunctions = $true
    }
    if ($DisplayConsole) {
        [FluentFTP.FtpTrace]::LogToConsole = $true
    }
    if ($HideUserName) {
        [FluentFTP.FtpTrace]::LogUserName = $false; # hide FTP user names
    }
    if ($ShowPassword) {
        [FluentFTP.FtpTrace]::LogPassword = $false; # hide FTP passwords
    }
    if (-not $HideIP) {
        [FluentFTP.FtpTrace]::LogIP = $true; # hide FTP server IP addresses
    }
}