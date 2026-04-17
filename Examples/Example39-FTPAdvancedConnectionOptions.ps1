Import-Module .\Transferetto.psd1 -Force

$Credential = Get-Credential

# Use these knobs when a server or network path needs explicit runtime tuning.
# Timeouts are milliseconds; rate limits are kilobytes per second.
$FtpClient = Connect-FTP -Server 'ftp.example.com' `
    -Credential $Credential `
    -EncryptionMode Explicit `
    -DataConnectionType AutoPassive `
    -ConnectTimeout 15000 `
    -ReadTimeout 15000 `
    -DataConnectionConnectTimeout 20000 `
    -DataConnectionReadTimeout 20000 `
    -RetryAttempts 3 `
    -TransferChunkSize 65536 `
    -LocalFileBufferSize 65536 `
    -InternetProtocolVersions ANY `
    -UploadRateLimit 0 `
    -DownloadRateLimit 0 `
    -UploadDataType Binary `
    -DownloadDataType Binary `
    -ListingDataType ASCII `
    -FXPDataType Binary `
    -FXPProgressInterval 1000 `
    -PassiveBlockedPorts 50000, 50001 `
    -PassiveMaxAttempts 5 `
    -EncodingName 'utf-8'

Get-FTPList -Client $FtpClient -Path '/'

Disconnect-FTP -Client $FtpClient
