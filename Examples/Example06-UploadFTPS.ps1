Import-Module .\Transferetto.psd1 -Force

$Client = Connect-FTP -Server '192.168.241.187' -Verbose -Username 'test' -Password 'BiPassword90A' -EncryptionMode Explicit -ValidateAnyCertificate
$List = Get-FTPList -Client $Client
$List | Format-Table
$List = Get-FTPList -Client $Client -Path '/Temporary'
$List | Format-Table

$ListFiles = Get-ChildItem -LiteralPath $PSScriptRoot\Upload
foreach ($File in $ListFiles) {
    Add-FTPFile -Client $Client -LocalPath $File.FullName -RemotePath "/Temporary/$($File.Name)" -RemoteExists Skip
    Add-FTPFile -Client $Client -LocalPath $File.FullName -RemotePath "/Temporary/CreateDir/$($File.Name)" -RemoteExists Skip -CreateRemoteDirectory
}

Disconnect-FTP -Client $Client