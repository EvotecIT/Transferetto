Describe 'Connect-FTP / Disconnect-FTP' {
    It 'Given no login and password it should connect to FTP and list' {
        # Anonymous login
        $Client = Connect-FTP -Server 'ftp.gnu.org' -Verbose
        $Client.IsConnected | Should -Be $True
        $List = Get-FTPList -Client $Client
        $List.Count | Should -BeGreaterThan 5
        $Names = 'Name', 'Type', 'FullName', 'Modified', 'Created'
        foreach ($Name in  @($Names)) {
            $List[0].PSObject.Properties.Name | Should -Contain $Name
        }
        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
    It 'Given login and password and Encryption Mode it should connect to FTP and list' {
        # Anonymous login
        $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -EncryptionMode Auto -SslBuffering Auto -SocketKeepAlive
        $Client.IsConnected | Should -Be $True
        $List = Get-FTPList -Client $Client
        $List.Count | Should -Be 2
        $Names = 'Name', 'Type', 'FullName', 'Modified', 'Created'
        foreach ($Name in  @($Names)) {
            $List[0].PSObject.Properties.Name | Should -Contain $Name
        }
        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
    It 'Given login and password it should connect to FTP' {
        # Login via UserName/Password
        $Client = Connect-FTP -Server 'ftp.dlptest.com' -Verbose -Username 'dlpuser' -Password 'rNrKYTX9g7z3RgJRmxWuGHbeu'
        $Client.IsConnected | Should -Be $True
        $List = Get-FTPList -Client $Client
        $List.Count | Should -BeGreaterThan 1
        $Names = 'Name', 'Type', 'FullName', 'Modified', 'Created'
        foreach ($Name in  @($Names)) {
            $List[0].PSObject.Properties.Name | Should -Contain $Name
        }
        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
}