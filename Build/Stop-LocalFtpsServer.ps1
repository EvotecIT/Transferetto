[CmdletBinding()]
param(
    [string] $Distribution = 'Ubuntu',
    [string] $PidPath = '/var/run/vsftpd-transferetto.pid'
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

$bashPidPath = ConvertTo-BashSingleQuotedString -Value $PidPath
$bashScript = @'
set -euo pipefail

PID_PATH=__PID_PATH__

if [ -f "$PID_PATH" ]; then
  PID=$(cat "$PID_PATH")
  if [ -n "$PID" ] && ps -p "$PID" -o pid= >/dev/null 2>&1; then
    kill "$PID" || true
    sleep 1
  fi
  rm -f "$PID_PATH"
fi

pkill vsftpd || true
'@

$bashScript = $bashScript.Replace('__PID_PATH__', $bashPidPath)

 $temporaryScriptPath = Join-Path ([System.IO.Path]::GetTempPath()) ("transferetto-stop-ftps-" + [guid]::NewGuid().ToString('N') + ".sh")
try {
    [System.IO.File]::WriteAllText($temporaryScriptPath, $bashScript, [System.Text.UTF8Encoding]::new($false))
    $wslTemporaryScriptPath = ConvertTo-WslPath -Path $temporaryScriptPath
    & wsl.exe -d $Distribution -u root -- bash $wslTemporaryScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "Could not stop local FTPS server in WSL distribution '$Distribution'."
    }
} finally {
    Remove-Item -LiteralPath $temporaryScriptPath -Force -ErrorAction SilentlyContinue
}

[pscustomobject] @{
    Distribution = $Distribution
    PidPath = $PidPath
    Stopped = $true
}
