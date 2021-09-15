Describe 'Test-FTPDirectory' {
    It 'Given an existing path it should return $True' {
        # Anonymous login
        $Client = Connect-FTP -Server 'speedtest.tele2.net' -Verbose
        $Client.IsConnected | Should -Be $True
        $ExistingDirectory = Test-FTPDirectory -RemotePath '/upload' -Client $Client | Should -Be $true
        $NonExistingDirectory = Test-FTPDirectory -RemotePath '/foobar' -Client $Client | Should -Be $false
        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
}