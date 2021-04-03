Import-Module .\Transferetto.psd1 -Force

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
# List files
$List = Get-FTPList -Client $Client
$List | Format-Table
# List files within Temporary directory
$List = Get-FTPList -Client $Client -Path '/Temporary'
$List | Format-Table

# Get local files
$ListFiles = Get-ChildItem -LiteralPath $PSScriptRoot\Upload

# Upload file by file
foreach ($File in $ListFiles) {
    # To temporary
    Send-FTPFile -Client $Client -LocalPath $File.FullName -RemotePath "/Temporary/$($File.Name)" -RemoteExists Overwrite
    # to directory within Temporary that may not exists
    Send-FTPFile -Client $Client -LocalPath $File.FullName -RemotePath "/Temporary/CreateDir/$($File.Name)" -RemoteExists Skip -CreateRemoteDirectory
}

# Upload all files at once to FTP
Send-FTPFile -Client $Client -LocalPath $ListFiles.FullName -RemotePath "/Temporary" -RemoteExists Overwrite

Disconnect-FTP -Client $Client