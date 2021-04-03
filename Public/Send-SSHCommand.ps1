function Send-SSHCommand {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][Renci.SshNet.SshClient] $SshClient,
        [scriptblock] $Command,
        [switch] $Status
    )
    if ($SshClient -and $SshClient.IsConnected -and -not $SshClient.Error) {
        if ($Command) {
            $CommandsToExecute = & $Command
            [string] $SendCommand = foreach ($C in $CommandsToExecute) {
                if ($C.Trim().EndsWith(';')) {
                    $C
                } else {
                    "$C;"
                }
            }
            try {
                Write-Verbose -Message "Send-SSHCommand - Executing command: $SendCommand"
                if ($Status) {
                    [PSCustomObject] @{
                        Status = $true
                        Output = $SshClient.CreateCommand($SendCommand).Execute()
                        Error  = $null
                    }
                } else {
                    $SshClient.CreateCommand($SendCommand).Execute()
                }
            } catch {
                if ($PSBoundParameters.ErrorAction -eq 'Stop') {
                    Write-Error $_
                    return
                } else {
                    Write-Warning "Send-SSHCommand - Error: $($_.Exception.Message)"
                }
                if ($Status) {
                    [PSCustomObject] @{
                        Status = $false
                        Output = ''
                        Error  = "Error: $($_.Exception.Message)"
                    }
                }
            }
        }
    }
}