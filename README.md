<p align="center">
  <a href="https://dev.azure.com/evotecpl/Transferetto/_build/results?buildId=latest"><img src="https://dev.azure.com/evotecpl/Transferetto/_apis/build/status/EvotecIT.Transferetto"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/v/Transferetto.svg"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/vpre/Transferetto.svg?label=powershell%20gallery%20preview&colorB=yellow"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/license/EvotecIT/Transferetto.svg"></a>
</p>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/p/Transferetto.svg"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/languages/top/evotecit/Transferetto.svg"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/languages/code-size/evotecit/Transferetto.svg"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/dt/Transferetto.svg"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=social"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn"></a>
</p>

# Transferetto - PowerShell Module

Transferetto is a PowerShell module that aims to provide FTP, FTPS, SFTP and maybe other features. It uses following .NET libraries to deliver this functionality:

- [FluentFTP](https://github.com/robinrodricks/FluentFTP)
- [SSH.NET](https://github.com/sshnet/SSH.NET/)

Both libraries are MIT license.

## Features

- FTP functionality
  - [x] Connect to FTP, FTPS, SFTP
  - [x] Upload/Download files from FTP/FTPS/SFTP
  - [x] Rename SFTP files

## Documentation

## To install

```powershell
Install-Module -Name Transferetto -AllowClobber -Force
```

Force and AllowClobber aren't necessary, but they do skip errors in case some appear.

## And to update

```powershell
Update-Module -Name Transferetto
```

That's it. Whenever there's a new version, you run the command, and you can enjoy it. Remember that you may need to close, reopen PowerShell session if you have already used module before updating it.

**The essential thing** is if something works for you on production, keep using it till you test the new version on a test computer. I do changes that may not be big, but big enough that auto-update may break your code. For example, a small rename to a parameter, and your code stops working! Be responsible!

## Changelog

- 0.0.1 - 29.03.2021
  - [x] First edition
