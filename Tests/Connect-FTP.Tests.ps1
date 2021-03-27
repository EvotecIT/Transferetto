Describe 'Connect-FTP / Disconnect-FTP' {
    It 'Given no login and password it should connect to FTP and list' {
        # Anonymous login
        $Client = Connect-FTP -Server 'speedtest.tele2.net' -Verbose
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
    It 'Given login and password it should connect to FTP' {
        # Login via UserName/Password
        $Client = Connect-FTP -Server 'test.rebex.net' -Verbose -Username 'demo' -Password 'password'
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