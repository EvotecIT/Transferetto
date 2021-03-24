function Set-FTPTracing {
    [cmdletBinding()]
    param(
        [string] $LogPath,
        [switch] $Enable,
        [switch] $Disable
    )
    if ($Enable) {
        [FluentFTP.FtpTrace]::EnableTracing = $true;
    }
    if ($Disable) {
        [FluentFTP.FtpTrace]::EnableTracing = $false;
    }
    if ($LogPath) {
        [FluentFTP.FtpTrace]::LogToFile = "log_file.txt";
    }

    [FluentFTP.FtpTrace]::LogFunctions = $true
    [FluentFTP.FtpTrace]::LogToConsole = $true
    [FluentFTP.FtpTrace]::LogUserName = $true; # hide FTP user names
    [FluentFTP.FtpTrace]::LogPassword = $false; # hide FTP passwords
    [FluentFTP.FtpTrace]::LogIP = $true; # hide FTP server IP addresses
}