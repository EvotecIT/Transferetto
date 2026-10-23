using System;
using System.Collections.Generic;
using FluentFTP;

namespace Transferetto;

/// <summary>The existing FTP library owns listing, transfer, resume and checksum behavior.</summary>
internal interface IFtpDirectoryDownloadOperations {
    bool DirectoryExists(string path);
    IReadOnlyList<FtpListItem> ListDirectory(string path);
    FtpStatus DownloadFile(string localPath, string remotePath, FtpLocalExists existsMode,
        FtpVerify verifyOptions, Action<FtpProgress> progress);
    bool DeleteExcluded { get; }
    bool DownloadZeroByteFiles { get; }
    FtpDataType DownloadDataType { get; }
}

internal sealed class FtpDirectoryDownloadOperations : IFtpDirectoryDownloadOperations {
    private readonly TransferettoFtpSession _session;
    internal FtpDirectoryDownloadOperations(TransferettoFtpSession session) { _session = session; }
    public bool DirectoryExists(string path) => _session.Client.DirectoryExists(path);
    public IReadOnlyList<FtpListItem> ListDirectory(string path) => TransferettoClient.GetFtpDirectoryListingItemsExact(_session, path);
    public FtpStatus DownloadFile(string localPath, string remotePath, FtpLocalExists existsMode,
        FtpVerify verifyOptions, Action<FtpProgress> progress) =>
        _session.Client.DownloadFile(localPath, remotePath, existsMode, verifyOptions, progress);
    public bool DeleteExcluded => _session.Client.Config.DownloadDirectoryDeleteExcluded;
    public bool DownloadZeroByteFiles => _session.Client.Config.DownloadZeroByteFiles;
    public FtpDataType DownloadDataType => _session.Client.Config.DownloadDataType;
}
