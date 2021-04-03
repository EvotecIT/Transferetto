function Set-FTPTracing {
    [cmdletBinding()]
    param(
        [string] $LogPath,
        [switch] $Enable,
        [switch] $Disable,
        [switch] $ShowPassword,
        [switch] $ShowUsername,
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
    if ($ShowUsername) {
        [FluentFTP.FtpTrace]::LogUserName = $true; # hide FTP user names
    }
    if ($ShowPassword) {
        [FluentFTP.FtpTrace]::LogPassword = $false; # hide FTP passwords
    }
    if (-not $HideIP) {
        [FluentFTP.FtpTrace]::LogIP = $true; # hide FTP server IP addresses
    }
}