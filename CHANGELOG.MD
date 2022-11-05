#### 0.0.16 - 2022.11.05
- Fixes `Connect-FTP` options due to new version change of FLuentFTP
- Upgraded `FluentFTP` to `40.0.0` to `42.0.0`
- Added additional tests to try and be proactive!

#### 0.0.15 - 2022.09.22
- Upgraded `FluentFTP` to `40.0.0`

#### 0.0.14 - 2022.08.04
- Fixes problem with missing function in PSGallery

#### 0.0.13 - 2022.07.11
- Upgraded `FluentFTP` to `37.0.6`

#### 0.0.12 - 2022.06.17
- Upgraded `SSH.NET` to `2020.0.2` which fixes weak private key generation - [SSH.NET Advisory](https://github.com/sshnet/SSH.NET/security/advisories/GHSA-72p8-v4hg-v45p)
- Upgraded `FluentFTP` to `37.0.3.0`
- Fixes `Set-FTPTracing`

#### 0.0.11 - 2022.04.10
- Small docs update to `Set-FTPTracing`
- Changed `ShowUsername` to `HideUsername` in `Set-FTPTracing`
- Added detection for `netframework` to be minimum 4.7.2
- Upgraded `FluentFTP` to `37.0.2.0`
#### 0.0.10 - 2021.12.22
- Fixes `Connect-SFTP` port functionality
- Fixes `Connect-SSH` port functionality
#### 0.0.9 - 2021.10.29
- Fixes `Rename-SFTPFile` - reported [#7](https://github.com/EvotecIT/Transferetto/issues/7)

#### 0.0.8 - 2021.09.15
- Added command `Test-FTPDirectory` - thank you [Sidewinder53](https://github.com/EvotecIT/Transferetto/pull/5)

#### 0.0.7 - 2021.09.15
- Republished module to PowerShell Gallery

#### 0.0.6 - 2021.09.14
- Added support for key authentication in `Connect-SFTP` - thank you [Szeraax!](https://github.com/EvotecIT/Transferetto/pull/3)
- Improved error handling of `Connect-SFTP`

#### 0.0.5 - 2021.04.04
- Small fixes

#### 0.0.4 - 2021.03.29
- First edition
