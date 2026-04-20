[CmdletBinding()]
param(
    [string] $Distribution = 'Ubuntu',
    [string] $Server = '127.0.0.1',
    [int] $Port = 2121,
    [int] $PassivePortStart = 22100,
    [int] $PassivePortEnd = 22110,
    [string] $UserName = 'transferettoftps',
    [string] $Password = 'Transferetto123!',
    [string] $RootPath = '/srv/transferetto-ftps',
    [string] $UploadDirectoryName = 'upload',
    [string] $ConfigPath = '/etc/vsftpd-transferetto.conf',
    [string] $PidPath = '/var/run/vsftpd-transferetto.pid',
    [string] $LogPath = '/var/log/vsftpd-transferetto.log',
    [string] $StdOutLogPath = '/var/log/vsftpd-transferetto.stdout'
)

function ConvertTo-BashSingleQuotedString {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Value
    )

    $escapedSingleQuote = [string]::Concat("'", '"', "'", '"', "'")
    $escapedValue = $Value.Replace("'", $escapedSingleQuote)
    return "'" + $escapedValue + "'"
}

function ConvertTo-WslPath {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $resolvedPath = (Resolve-Path -LiteralPath $Path).ProviderPath
    $drive = [char]::ToLowerInvariant($resolvedPath[0])
    $suffix = $resolvedPath.Substring(2).Replace('\', '/')
    return "/mnt/$drive$suffix"
}

$bashPort = $Port.ToString([System.Globalization.CultureInfo]::InvariantCulture)
$bashPassivePortStart = $PassivePortStart.ToString([System.Globalization.CultureInfo]::InvariantCulture)
$bashPassivePortEnd = $PassivePortEnd.ToString([System.Globalization.CultureInfo]::InvariantCulture)
$bashDistribution = ConvertTo-BashSingleQuotedString -Value $Distribution
$bashServer = ConvertTo-BashSingleQuotedString -Value $Server
$bashUserName = ConvertTo-BashSingleQuotedString -Value $UserName
$bashPassword = ConvertTo-BashSingleQuotedString -Value $Password
$bashRootPath = ConvertTo-BashSingleQuotedString -Value $RootPath
$bashUploadDirectoryName = ConvertTo-BashSingleQuotedString -Value $UploadDirectoryName
$bashConfigPath = ConvertTo-BashSingleQuotedString -Value $ConfigPath
$bashPidPath = ConvertTo-BashSingleQuotedString -Value $PidPath
$bashLogPath = ConvertTo-BashSingleQuotedString -Value $LogPath
$bashStdOutLogPath = ConvertTo-BashSingleQuotedString -Value $StdOutLogPath

$bashScript = @'
set -euo pipefail

ROOT_PATH=__ROOT_PATH__
UPLOAD_DIRECTORY_NAME=__UPLOAD_DIRECTORY_NAME__
USER_NAME=__USER_NAME__
PASSWORD=__PASSWORD__
CONFIG_PATH=__CONFIG_PATH__
PID_PATH=__PID_PATH__
LOG_PATH=__LOG_PATH__
STDOUT_LOG_PATH=__STDOUT_LOG_PATH__
LISTEN_PORT=__LISTEN_PORT__
PASSIVE_PORT_START=__PASSIVE_PORT_START__
PASSIVE_PORT_END=__PASSIVE_PORT_END__
PASSIVE_ADDRESS=__PASSIVE_ADDRESS__

mkdir -p "$ROOT_PATH/$UPLOAD_DIRECTORY_NAME"

if ! id -u "$USER_NAME" >/dev/null 2>&1; then
  useradd -d "$ROOT_PATH" -s /bin/bash "$USER_NAME"
else
  usermod -d "$ROOT_PATH" -s /bin/bash "$USER_NAME"
fi

echo "$USER_NAME:$PASSWORD" | chpasswd
chown -R "$USER_NAME:$USER_NAME" "$ROOT_PATH"

cat >"$CONFIG_PATH" <<EOF
listen=YES
listen_ipv6=NO
listen_port=__LISTEN_PORT__
anonymous_enable=NO
local_enable=YES
write_enable=YES
local_umask=022
use_localtime=YES
xferlog_enable=YES
connect_from_port_20=NO
secure_chroot_dir=/var/run/vsftpd/empty
pam_service_name=vsftpd
local_root=__LOCAL_ROOT__
chroot_local_user=YES
allow_writeable_chroot=YES
ssl_enable=YES
allow_anon_ssl=NO
force_local_logins_ssl=YES
force_local_data_ssl=YES
require_ssl_reuse=YES
rsa_cert_file=/etc/ssl/certs/ssl-cert-snakeoil.pem
rsa_private_key_file=/etc/ssl/private/ssl-cert-snakeoil.key
pasv_enable=YES
pasv_min_port=__PASSIVE_PORT_START__
pasv_max_port=__PASSIVE_PORT_END__
pasv_address=__SERVER__
port_enable=NO
vsftpd_log_file=__LOG_PATH_RAW__
dual_log_enable=YES
seccomp_sandbox=NO
EOF

pkill vsftpd || true
rm -f "$PID_PATH"

nohup /usr/sbin/vsftpd "$CONFIG_PATH" >"$STDOUT_LOG_PATH" 2>&1 < /dev/null &
echo $! >"$PID_PATH"
sleep 2

if ! ps -p "$(cat "$PID_PATH")" -o pid= >/dev/null 2>&1; then
  echo "vsftpd failed to start." >&2
  cat "$STDOUT_LOG_PATH" >&2 || true
  exit 1
fi
'@

$bashScript = $bashScript.Replace('__ROOT_PATH__', $bashRootPath)
$bashScript = $bashScript.Replace('__UPLOAD_DIRECTORY_NAME__', $bashUploadDirectoryName)
$bashScript = $bashScript.Replace('__USER_NAME__', $bashUserName)
$bashScript = $bashScript.Replace('__PASSWORD__', $bashPassword)
$bashScript = $bashScript.Replace('__CONFIG_PATH__', $bashConfigPath)
$bashScript = $bashScript.Replace('__PID_PATH__', $bashPidPath)
$bashScript = $bashScript.Replace('__LOG_PATH__', $bashLogPath)
$bashScript = $bashScript.Replace('__STDOUT_LOG_PATH__', $bashStdOutLogPath)
$bashScript = $bashScript.Replace('__LISTEN_PORT__', $bashPort)
$bashScript = $bashScript.Replace('__PASSIVE_PORT_START__', $bashPassivePortStart)
$bashScript = $bashScript.Replace('__PASSIVE_PORT_END__', $bashPassivePortEnd)
$bashScript = $bashScript.Replace('__PASSIVE_ADDRESS__', $bashServer)
$bashScript = $bashScript.Replace('__LOCAL_ROOT__', $RootPath)
$bashScript = $bashScript.Replace('__SERVER__', $Server)
$bashScript = $bashScript.Replace('__LOG_PATH_RAW__', $LogPath)

 $temporaryScriptPath = Join-Path ([System.IO.Path]::GetTempPath()) ("transferetto-start-ftps-" + [guid]::NewGuid().ToString('N') + ".sh")
try {
    [System.IO.File]::WriteAllText($temporaryScriptPath, $bashScript, [System.Text.UTF8Encoding]::new($false))
    $wslTemporaryScriptPath = ConvertTo-WslPath -Path $temporaryScriptPath
    & wsl.exe -d $Distribution -u root -- bash $wslTemporaryScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "Could not start local FTPS server in WSL distribution '$Distribution'."
    }
} finally {
    Remove-Item -LiteralPath $temporaryScriptPath -Force -ErrorAction SilentlyContinue
}

[pscustomobject] @{
    Distribution = $Distribution
    Server = $Server
    Port = $Port
    UserName = $UserName
    Password = $Password
    RootPath = $RootPath
    UploadDirectory = "/$UploadDirectoryName"
    ConfigPath = $ConfigPath
    PidPath = $PidPath
    LogPath = $LogPath
    StdOutLogPath = $StdOutLogPath
    RequireSslReuse = $true
    SampleConnectCommand = "Connect-FTP -Server '$Server' -Port $Port -Username '$UserName' -Password '$Password' -EncryptionMode Explicit -DataConnectionType AutoPassive -ValidateAnyCertificate"
}
