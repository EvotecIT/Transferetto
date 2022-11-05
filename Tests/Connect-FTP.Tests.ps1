Describe 'Connect-FTP / Disconnect-FTP' {
    It 'Given no login and password it should connect to FTP and list' {
        # Anonymous login
        $Client = $Null
        $Client = Connect-FTP -Server 'ftp.gnu.org' -Verbose
        $Client.Host | Should -Be 'ftp.gnu.org'
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
        $Client = $Null
        $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -EncryptionMode Auto -SslBuffering Auto -SocketKeepAlive
        $Client.Host | Should -Be 'test.rebex.net'
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
        $Client = $Null
        $Client = Connect-FTP -Server 'ftp.dlptest.com' -Verbose -Username 'dlpuser' -Password 'rNrKYTX9g7z3RgJRmxWuGHbeu'
        $Client.Host | Should -Be 'ftp.dlptest.com'
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
    It 'Given login and password, and proxy Socks 5 it should conntect and list' {
        # Anonymous login
        $Client = $Null
        $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -EncryptionMode Auto -SslBuffering Auto -SocketKeepAlive -ProxyHost '192.252.216.81' -ProxyPort 4145 -ProxyType FtpClientSocks5Proxy
        $Client.Host | Should -Be 'test.rebex.net'
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

    # It 'Given login and password, and proxy Socks 4a it should conntect and list' {
    #     # Anonymous login
    #     $Client = $Null
    #     $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password' -EncryptionMode Auto -SslBuffering Auto -SocketKeepAlive -ProxyHost '104.37.135.145' -ProxyPort 4145 -ProxyType FtpClientSocks4aProxy
    #     $Client.Host | Should -Be 'test.rebex.net'
    #     $Client.IsConnected | Should -Be $True
    #     $List = Get-FTPList -Client $Client
    #     $List.Count | Should -Be 2
    #     $Names = 'Name', 'Type', 'FullName', 'Modified', 'Created'
    #     foreach ($Name in  @($Names)) {
    #         $List[0].PSObject.Properties.Name | Should -Contain $Name
    #     }
    #     Disconnect-FTP -Client $Client
    #     $Client.IsConnected | Should -Be $false
    # }

}