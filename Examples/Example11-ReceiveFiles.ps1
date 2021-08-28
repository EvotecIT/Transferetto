Import-Module .\Transferetto.psd1 -Force

# Connect to FTP
$Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'

# Get list of files on FTP
$List = Get-FTPList -Client $Client -Path '/pub/example'

# Find latest file on FTP server
$FindLatestFile = $List | Where-Object { $_.Type -eq 'File' } | Sort-Object -Property Modified -Descending | Select-Object -First 2

# Download that file
foreach ($RemoteFile in $FindLatestFile) {
    Receive-FTPFile -Client $Client -RemoteFile $RemoteFile -LocalPath "$PSScriptRoot\Download\$($RemoteFile.Name)" -LocalExists Overwrite -VerifyOptions Retry, None
}
# Download multiple files into directory
Receive-FTPFile -Client $Client -RemoteFile $FindLatestFile -LocalPath "$PSScriptRoot\Download" -LocalExists Overwrite -VerifyOptions Retry, None

# Disconnect
Disconnect-FTP -Client $Client