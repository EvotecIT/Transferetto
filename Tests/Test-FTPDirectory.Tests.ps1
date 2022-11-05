Describe 'Test-FTPDirectory' {
    It 'Given an existing path it should return $True' {
        # Anonymous login
        $Client = $Null
        $Client = Connect-FTP -Server 'ftp.gnu.org' -Verbose
        $Client.IsConnected | Should -Be $True
        $ExistingDirectory = Test-FTPDirectory -RemotePath '/tmp' -Client $Client | Should -Be $true
        $NonExistingDirectory = Test-FTPDirectory -RemotePath '/foobar1' -Client $Client | Should -Be $false
        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
}