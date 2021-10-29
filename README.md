# Transferetto - PowerShell Module

<p align="center">
  <a href="https://dev.azure.com/evotecpl/Transferetto/_build/results?buildId=latest"><img src="https://img.shields.io/azure-devops/build/evotecpl/39c74615-8f34-4af0-a835-68dc33f9214f/14?label=Azure%20Pipelines&style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/v/Transferetto.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/vpre/Transferetto.svg?label=powershell%20gallery%20preview&colorB=yellow&style=flat-square"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/license/EvotecIT/Transferetto.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/p/Transferetto.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/languages/top/evotecit/Transferetto.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/Transferetto"><img src="https://img.shields.io/github/languages/code-size/evotecit/Transferetto.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/Transferetto"><img src="https://img.shields.io/powershellgallery/dt/Transferetto.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=flat-square&logo=twitter"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg?style=flat-square"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn&style=flat-square"></a>
</p>

Transferetto is a PowerShell module that aims to provide FTP, FTPS, SFTP functionality. To find out more about it I've created blog post [Easy way to connect to FTPS and SFTP using PowerShell](https://evotec.xyz/easy-way-to-connect-to-ftps-and-sftp-using-powershell/).

It uses following .NET libraries to deliver this functionality:

- [FluentFTP](https://github.com/robinrodricks/FluentFTP)
- [SSH.NET](https://github.com/sshnet/SSH.NET/)

Both libraries are MIT license.

## Features

- FTPS/SFTP functionality
  - Connect to FTP, FTPS, SFTP
  - Upload/Download files from FTP/FTPS/SFTP
  - Rename SFTP files
  - Remove FTP/FTPS files
  - And some more

Please make sure to read blog post or check examples to see how to use it.

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