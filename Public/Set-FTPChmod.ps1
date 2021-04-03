function Set-FTPChmod {
    [cmdletBinding(DefaultParameterSetName = 'ByInt')]
    param(
        [Parameter(Mandatory, ParameterSetName = 'ByInt')]
        [Parameter(Mandatory, ParameterSetName = 'Explicit')]
        [FluentFTP.FtpClient] $Client,

        [Parameter(Mandatory, ParameterSetName = 'ByInt')]
        [Parameter(Mandatory, ParameterSetName = 'Explicit')]
        [string] $RemotePath,

        [Parameter(Mandatory, ParameterSetName = 'ByInt')]
        [nullable[int]] $Permissions,

        [Parameter(Mandatory, ParameterSetName = 'Explicit')]
        [FluentFTP.FtpPermission] $Owner,

        [Parameter(Mandatory, ParameterSetName = 'Explicit')]
        [FluentFTP.FtpPermission] $Group,

        [Parameter(Mandatory, ParameterSetName = 'Explicit')]
        [FluentFTP.FtpPermission] $Other
    )
    if ($Client -and $Client.IsConnected -and -not $Client.Error) {
        #void Chmod(string path, int permissions)
        #void Chmod(string path, FluentFTP.FtpPermission owner, FluentFTP.FtpPermission group, FluentFTP.FtpPermission other)
        if ($Permissions) {
            $Client.Chmod($RemotePath, $Permissions)
        } else {
            $Client.Chmod($RemotePath, $Owner, $Group, $Other)
        }
    }
}