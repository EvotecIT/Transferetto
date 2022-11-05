Describe 'Connect-FTP / Disconnect-FTP' {
    It 'Given FTP server should be able to download at least one file' {
        # Anonymous login
        $Client = $Null
        $Client = Connect-FTP -Server 'ftp.gnu.org' -Verbose
        $Client.IsConnected | Should -Be $True
        $List = Get-FTPList -Client $Client
        $List.Count | Should -BeGreaterThan 5
        $ListVideo = Get-FTPList -Client $Client -Path "/video"
        $ListVideo.Count | Should -BeGreaterThan 20
        $TestPath = "$($TestDrive)\$($ListVideo[0].Name)"

        Receive-FTPFile -Client $Client -RemotePath $ListVideo[0].FullName -LocalPath $TestPath

        Test-Path -LiteralPath $TestPath | Should -Be $True
        (Get-Item -LiteralPath $TestPath).Length | Should -BeGreaterThan 100000

        Disconnect-FTP -Client $Client
        $Client.IsConnected | Should -Be $false
    }
}