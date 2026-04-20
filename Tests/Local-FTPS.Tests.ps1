$script:LocalFtpsEnabled = $env:TRANSFERETTO_RUN_LOCAL_FTPS_TESTS -eq '1'

Describe 'Local FTPS integration' -Tag LocalFTPS {
    It 'connects to a local explicit FTPS server and lists the upload directory' -Skip:(-not $script:LocalFtpsEnabled) {
        $LocalFtpsServer = if ($env:TRANSFERETTO_LOCAL_FTPS_SERVER) { $env:TRANSFERETTO_LOCAL_FTPS_SERVER } else { '127.0.0.1' }
        $LocalFtpsPort = if ([string]::IsNullOrWhiteSpace($env:TRANSFERETTO_LOCAL_FTPS_PORT)) { 2121 } else { [int] $env:TRANSFERETTO_LOCAL_FTPS_PORT }
        $LocalFtpsUserName = if ($env:TRANSFERETTO_LOCAL_FTPS_USERNAME) { $env:TRANSFERETTO_LOCAL_FTPS_USERNAME } else { 'transferettoftps' }
        $LocalFtpsPassword = if ($env:TRANSFERETTO_LOCAL_FTPS_PASSWORD) { $env:TRANSFERETTO_LOCAL_FTPS_PASSWORD } else { 'Transferetto123!' }

        $Client = Connect-FTP -Server $LocalFtpsServer `
            -Port $LocalFtpsPort `
            -Username $LocalFtpsUserName `
            -Password $LocalFtpsPassword `
            -EncryptionMode Explicit `
            -DataConnectionType AutoPassive `
            -ValidateAnyCertificate `
            -ErrorAction Stop

        try {
            $Client.IsConnected | Should -Be $true
            $List = Get-FTPList -Client $Client -Path '/' -ErrorAction Stop
            $List.Name | Should -Contain 'upload'
        } finally {
            Disconnect-FTP -Client $Client
        }
    }

    It 'uploads and downloads a file over explicit FTPS without reporting a false failure' -Skip:(-not $script:LocalFtpsEnabled) {
        $LocalFtpsServer = if ($env:TRANSFERETTO_LOCAL_FTPS_SERVER) { $env:TRANSFERETTO_LOCAL_FTPS_SERVER } else { '127.0.0.1' }
        $LocalFtpsPort = if ([string]::IsNullOrWhiteSpace($env:TRANSFERETTO_LOCAL_FTPS_PORT)) { 2121 } else { [int] $env:TRANSFERETTO_LOCAL_FTPS_PORT }
        $LocalFtpsUserName = if ($env:TRANSFERETTO_LOCAL_FTPS_USERNAME) { $env:TRANSFERETTO_LOCAL_FTPS_USERNAME } else { 'transferettoftps' }
        $LocalFtpsPassword = if ($env:TRANSFERETTO_LOCAL_FTPS_PASSWORD) { $env:TRANSFERETTO_LOCAL_FTPS_PASSWORD } else { 'Transferetto123!' }

        $Client = Connect-FTP -Server $LocalFtpsServer `
            -Port $LocalFtpsPort `
            -Username $LocalFtpsUserName `
            -Password $LocalFtpsPassword `
            -EncryptionMode Explicit `
            -DataConnectionType AutoPassive `
            -ValidateAnyCertificate `
            -ErrorAction Stop

        try {
            $UploadPath = Join-Path $TestDrive 'upload.txt'
            $DownloadPath = Join-Path $TestDrive 'download.txt'
            $RemotePath = '/upload/upload.txt'
            $ExpectedContent = 'transferetto-ftps-test'
            Set-Content -Path $UploadPath -Value $ExpectedContent -NoNewline

            $UploadResult = Send-FTPFile -Client $Client -LocalPath $UploadPath -RemotePath $RemotePath -RemoteExists Overwrite -ErrorAction Stop
            $UploadResult.Status | Should -Be $true
            $UploadResult.IsFailed | Should -Be $false

            Test-FTPFile -Client $Client -RemotePath $RemotePath -ErrorAction Stop | Should -Be $true

            $DownloadResult = Receive-FTPFile -Client $Client -RemotePath $RemotePath -LocalPath $DownloadPath -LocalExists Overwrite -ErrorAction Stop
            $DownloadResult.Status | Should -Be $true
            $DownloadResult.IsFailed | Should -Be $false
            Get-Content -Path $DownloadPath -Raw | Should -Be $ExpectedContent
        } finally {
            Disconnect-FTP -Client $Client
        }
    }
}
