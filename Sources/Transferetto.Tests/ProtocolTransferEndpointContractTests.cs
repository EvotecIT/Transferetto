using System.Reflection;
using FluentFTP;
using Renci.SshNet;
using Transferetto.Core;
using Renci.SshNet.Common;

namespace Transferetto.Tests;

public sealed class ProtocolTransferEndpointContractTests {
    [Fact]
    public void SftpInspection_ReturnsNullWhenFileDisappearsDuringMetadataLookup() {
        TransferettoSftpAttributes? attributes = TransferettoClient.TryGetSftpFileAttributes(
            () => throw new SftpPathNotFoundException("The file disappeared."));

        Assert.Null(attributes);
    }

    [Fact]
    public void SftpInspection_UsesTheSingleFetchedFileSnapshot() {
        int calls = 0;
        TransferettoSftpAttributes expected = new() { IsRegularFile = true, Size = 42 };

        TransferettoSftpAttributes? attributes = TransferettoClient.TryGetSftpFileAttributes(() => {
            calls++;
            return expected;
        });

        Assert.Same(expected, attributes);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void FtpEndpoint_UsesProtocolPackageAndDoesNotExposeCredentials() {
        using FtpClient client = new("ftp.example.com") {
            Credentials = new System.Net.NetworkCredential("user", "secret")
        };
        using TransferettoFtpSession session = CreateFtpSession(client);
        using FtpTransferEndpoint endpoint = new(session, "incoming", ownsSession: false, workingDirectory: "/");

        Assert.Equal("Transferetto.Protocols", typeof(FtpTransferEndpoint).Assembly.GetName().Name);
        Assert.Equal("ftp://ftp.example.com/incoming", endpoint.DisplayName.TrimEnd('/'));
        Assert.DoesNotContain("user", endpoint.DisplayName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", endpoint.DisplayName, StringComparison.OrdinalIgnoreCase);
        Assert.False(endpoint.Capabilities.HasFlag(TransferEndpointCapabilities.Metadata));
    }

    [Fact]
    public void SftpEndpoint_UsesProtocolPackageAndDoesNotExposeCredentials() {
        using SftpClient client = new("sftp.example.com", "user", "secret");
        using TransferettoSftpSession session = CreateSftpSession(client);
        using SftpTransferEndpoint endpoint = new(
            session,
            "incoming",
            ownsSession: false,
            workingDirectory: "/srv/data");

        Assert.Equal("Transferetto.Protocols", typeof(SftpTransferEndpoint).Assembly.GetName().Name);
        Assert.Equal("sftp://sftp.example.com:22/srv/data/incoming", endpoint.DisplayName.TrimEnd('/'));
        Assert.DoesNotContain("user", endpoint.DisplayName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", endpoint.DisplayName, StringComparison.OrdinalIgnoreCase);
        Assert.False(endpoint.Capabilities.HasFlag(TransferEndpointCapabilities.Metadata));
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("root/../../outside")]
    public void ProtocolEndpoints_RejectPrefixesThatEscapeTheirRoot(string prefix) {
        using FtpClient ftpClient = new("ftp.example.com");
        using TransferettoFtpSession ftpSession = CreateFtpSession(ftpClient);
        using SftpClient sftpClient = new("sftp.example.com", "user", "secret");
        using TransferettoSftpSession sftpSession = CreateSftpSession(sftpClient);

        Assert.Throws<ArgumentException>(() =>
            new FtpTransferEndpoint(ftpSession, prefix, ownsSession: false, workingDirectory: "/home/user"));
        Assert.Throws<ArgumentException>(() =>
            new SftpTransferEndpoint(sftpSession, prefix, ownsSession: false, workingDirectory: "/home/user"));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("folder/..")]
    [InlineData("/absolute")]
    public async Task ProtocolEndpoints_RejectInvalidEndpointRelativePaths(string path) {
        using FtpClient client = new("ftp.example.com");
        using TransferettoFtpSession session = CreateFtpSession(client);
        using FtpTransferEndpoint endpoint = new(session, null, ownsSession: false, workingDirectory: "/");

        await Assert.ThrowsAsync<ArgumentException>(() => endpoint.GetItemAsync(path));
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(42, 42)]
    public void ProtocolEndpoints_NormalizeUnknownLengths(long providerLength, long expectedLength) {
        long? normalized = ProtocolTransferEndpointPath.NormalizeLength(providerLength);
        if (expectedLength < 0) {
            Assert.Null(normalized);
        } else {
            Assert.Equal(expectedLength, normalized);
        }
    }

    [Theory]
    [InlineData(null, "/home/user")]
    [InlineData("incoming", "/home/user/incoming")]
    [InlineData(@"incoming\archive", @"/home/user/incoming\archive")]
    [InlineData("/archive", "/archive")]
    public void ProtocolEndpoints_AnchorPrefixesToConstructionWorkingDirectory(
        string? prefix,
        string expectedRoot) {
        Assert.Equal(expectedRoot, ProtocolTransferEndpointPath.AnchorRoot(prefix, "/home/user"));
    }

    [Theory]
    [InlineData(" report.csv", " report.csv")]
    [InlineData("report.csv ", "report.csv ")]
    [InlineData(" ", " ")]
    [InlineData("folder/ report.csv ", "folder/ report.csv ")]
    [InlineData(@"report\final.csv", @"report\final.csv")]
    public void ProtocolEndpoints_PreserveLegalRelativeNameCharacters(string path, string expected) {
        Assert.Equal(expected, ProtocolTransferEndpointPath.NormalizeRelative(path));
    }

    [Fact]
    public void ProtocolEndpoints_PreserveLegalNameCharactersWhenCombiningListedNames() {
        Assert.Equal(
            "incoming/ report.csv ",
            ProtocolTransferEndpointPath.CombineRelative("incoming", " report.csv "));
        Assert.Equal(
            @"incoming/report\final.csv",
            ProtocolTransferEndpointPath.CombineRelative("incoming", @"report\final.csv"));
    }

    [Theory]
    [InlineData("report.csv", null)]
    [InlineData("/report.csv", "/")]
    [InlineData("/incoming/report.csv", "/incoming")]
    [InlineData("incoming/report.csv", "incoming")]
    public void ProtocolEndpoints_CreateBoundedTemporarySiblingNames(
        string destinationPath,
        string? expectedParent) {
        string temporaryPath = ProtocolTransferEndpointPath.CreateTemporaryPath(destinationPath);

        Assert.Equal(expectedParent, ProtocolTransferEndpointPath.GetParent(temporaryPath));
        string temporaryName = temporaryPath.Split('/').Last();
        Assert.StartsWith(".transferetto-", temporaryName, StringComparison.Ordinal);
        Assert.EndsWith(".tmp", temporaryName, StringComparison.Ordinal);
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(temporaryName + ".previous") <= 255);
    }

    [Fact]
    public void ProtocolEndpoints_DoNotExtendMaximumLengthDestinationNames() {
        string destinationName = new('x', 255);

        string temporaryPath = ProtocolTransferEndpointPath.CreateTemporaryPath("/incoming/" + destinationName);

        Assert.Equal("/incoming", ProtocolTransferEndpointPath.GetParent(temporaryPath));
        Assert.DoesNotContain(destinationName, temporaryPath, StringComparison.Ordinal);
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(temporaryPath.Split('/').Last() + ".previous") <= 255);
    }

    [Theory]
    [InlineData("/incoming ", "/incoming ")]
    [InlineData("/   ", "/   ")]
    [InlineData("/wild?card/", "/wild?card")]
    [InlineData("folder\\child ", "folder\\child ")]
    public void FtpEndpoint_PreservesExactDirectoryNames(string path, string expected) {
        Assert.Equal(expected, TransferettoClient.NormalizeExactDirectoryPath(path));
    }

    private static TransferettoFtpSession CreateFtpSession(FtpClient client) {
        ConstructorInfo constructor = typeof(TransferettoFtpSession)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoFtpSession) constructor.Invoke(new object?[] { client, null, null });
    }

    private static TransferettoSftpSession CreateSftpSession(SftpClient client) {
        ConstructorInfo constructor = typeof(TransferettoSftpSession)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (TransferettoSftpSession) constructor.Invoke(new object?[] { client, null });
    }
}
