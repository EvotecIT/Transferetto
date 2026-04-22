Describe 'Public SSH/SFTP/SCP integration' -Tag LiveSSH {
    BeforeAll {
        $script:Server = 'test.rebex.net'
        $script:UserName = 'demo'
        $script:Password = 'password'
        $script:RemoteDirectory = '/pub/example'
        $script:RemoteFile = '/pub/example/readme.txt'
    }

    It 'connects to SFTP, lists files, and downloads a sample file' {
        $sftpClient = $null
        $downloadPath = Join-Path $TestDrive 'rebex-sftp-readme.txt'

        try {
            $sftpClient = Connect-SFTP -Server $script:Server -Username $script:UserName -Password $script:Password -AcceptAnyHostKey
            $sftpClient.Host | Should -Be $script:Server
            $sftpClient.IsConnected | Should -Be $true

            $list = Get-SFTPList -SftpClient $sftpClient -Path $script:RemoteDirectory
            $list.Count | Should -BeGreaterThan 2
            ($list | Where-Object FullName -EQ $script:RemoteFile).Count | Should -Be 1

            $download = Receive-SFTPFile -SftpClient $sftpClient -RemotePath $script:RemoteFile -LocalPath $downloadPath
            $download.Status | Should -Be $true
            Test-Path -LiteralPath $downloadPath | Should -Be $true
            (Get-Item -LiteralPath $downloadPath).Length | Should -BeGreaterThan 100
            (Get-Content -LiteralPath $downloadPath -TotalCount 1) | Should -BeLike 'Welcome to test.rebex.net!*'
        } finally {
            if ($sftpClient) {
                Disconnect-SFTP -SftpClient $sftpClient
                $sftpClient.IsConnected | Should -Be $false
            }
        }
    }

    It 'connects to SCP and downloads a sample file' {
        $scpClient = $null
        $downloadPath = Join-Path $TestDrive 'rebex-scp-readme.txt'

        try {
            $scpClient = Connect-SCP -Server $script:Server -Username $script:UserName -Password $script:Password -AcceptAnyHostKey
            $scpClient.Host | Should -Be $script:Server
            $scpClient.IsConnected | Should -Be $true

            $download = Receive-SCPFile -ScpClient $scpClient -RemotePath $script:RemoteFile -LocalPath $downloadPath
            $download.Status | Should -Be $true
            Test-Path -LiteralPath $downloadPath | Should -Be $true
            (Get-Item -LiteralPath $downloadPath).Length | Should -BeGreaterThan 100
            (Get-Content -LiteralPath $downloadPath -TotalCount 1) | Should -BeLike 'Welcome to test.rebex.net!*'
        } finally {
            if ($scpClient) {
                Disconnect-SCP -ScpClient $scpClient
                $scpClient.IsConnected | Should -Be $false
            }
        }
    }

    It 'connects to SSH and runs a simple command' {
        $sshClient = $null

        try {
            $sshClient = Connect-SSH -Server $script:Server -Username $script:UserName -Password $script:Password -AcceptAnyHostKey
            $sshClient.Host | Should -Be $script:Server
            $sshClient.IsConnected | Should -Be $true

            $result = Send-SSHCommand -SshClient $sshClient -Command { 'pwd' } -Status
            $result.Status | Should -Be $true
            $result.Output | Should -Match '(?m)^/$'
        } finally {
            if ($sshClient) {
                Disconnect-SSH -SshClient $sshClient
                $sshClient.IsConnected | Should -Be $false
            }
        }
    }
}
