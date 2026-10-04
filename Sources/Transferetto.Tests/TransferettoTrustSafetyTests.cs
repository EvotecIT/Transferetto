using System.Reflection;
using FluentFTP;

namespace Transferetto.Tests;

public sealed class TransferettoTrustSafetyTests {
    [Theory]
    [InlineData("SHA256:", true)]
    [InlineData("", true)]
    [InlineData("SHA256:", false)]
    public void Sha256PinsCompareDecodedCaseSensitiveValues(string prefix, bool padded) {
        string digest = Convert.ToBase64String(new byte[32]);
        if (!padded) { digest = digest.TrimEnd('='); }
        TransferettoSshHostKeyInfo host = new() { FingerPrintSHA256 = prefix + digest };
        Assert.True(Matches(new[] { "SHA256:" + digest }, host));
        Assert.False(Matches(new[] { "SHA256:a" + digest.Substring(1) }, host));
    }

    [Fact]
    public void Md5PinsRetainHexadecimalCaseAndSeparatorCompatibility() {
        TransferettoSshHostKeyInfo host = new() { FingerPrintMD5 = "aa:bb:cc:dd:ee:ff:00:11:22:33:44:55:66:77:88:99" };
        Assert.True(Matches(new[] { "MD5:AA-BB-CC-DD-EE-FF-00-11-22-33-44-55-66-77-88-99" }, host));
    }

    [Theory]
    [InlineData(false, "server\tinvalid-port\tincomplete\n")]
    [InlineData(true, "server\tinvalid-port\tincomplete\n")]
    [InlineData(false, "\n")]
    [InlineData(true, "\n")]
    [InlineData(false, "  \n\t\n")]
    [InlineData(true, "  \n\t\n")]
    [InlineData(false, "# store comment\n")]
    [InlineData(true, "# store comment\n")]
    [InlineData(false, "  # store comment\n")]
    [InlineData(true, "  # store comment\n")]
    [InlineData(false, "\t# store comment\n")]
    [InlineData(true, "\t# store comment\n")]
    public void MalformedExistingStoreFailsClosedWithoutChangingItsBytes(bool ssh, string malformed) {
        WithStore(path => {
            File.WriteAllText(path, malformed);
            TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => Trust(path, "server", ssh));
            Assert.IsType<InvalidDataException>(error.InnerException);
            Assert.Equal(malformed, File.ReadAllText(path));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConcurrentFirstUseRetainsEveryEndpointAndReusesItsIdentity(bool ssh) {
        WithStore(path => {
            Parallel.For(0, 24, index => Assert.True(Trust(path, "host" + index, ssh)));
            Assert.Equal(24, File.ReadAllLines(path).Length);
            for (int index = 0; index < 24; index++) { Assert.True(Trust(path, "host" + index, ssh, firstUse: false)); }
            Assert.Equal(24, File.ReadAllLines(path).Length);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RacingIdentitiesForOneEndpointAcceptOnlyOneFirstUse(bool ssh) {
        WithStore(path => {
            bool[] trusted = new bool[2];
            Parallel.For(0, 2, index => trusted[index] = Trust(path, "server", ssh, identity: index));
            Assert.Single(trusted, value => value);
            Assert.Single(File.ReadAllLines(path));
        });
    }

    private static bool Matches(IEnumerable<string> pins, TransferettoSshHostKeyInfo host) => (bool)typeof(TransferettoClient)
        .GetMethod("FingerprintMatches", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new object[] { pins, host })!;

    private static bool Trust(string path, string host, bool ssh, bool firstUse = true, int identity = 0) {
        if (ssh) {
            byte[] digest = new byte[32]; digest[0] = (byte)identity;
            TransferettoSshHostKeyInfo key = new() { HostKeyName = "ssh-ed25519", KeyLength = 256, FingerPrintSHA256 = Convert.ToBase64String(digest) };
            TransferettoSshConnectionOptions options = new() { Server = host, Port = 2222, KnownHostsPath = path };
            return ((TransferettoSshHostKeyInfo)typeof(TransferettoClient).GetMethod("EvaluateKnownHostsTrust", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, new object[] { options, key, firstUse })!).CanTrust;
        }
        using FtpClient client = new(host, 990);
        TransferettoFtpConnectionOptions ftpOptions = new() { Server = host, Port = 990, KnownCertificatesPath = path };
        TransferettoFtpCertificateInfo certificate = new() { ThumbprintSHA256 = new string(identity == 0 ? 'A' : 'B', 64) };
        return ((TransferettoFtpCertificateInfo)typeof(TransferettoClient).GetMethod("EvaluateKnownCertificateTrust", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, new object[] { ftpOptions, client, certificate, firstUse })!).CanTrust;
    }

    private static void WithStore(Action<string> action) {
        string directory = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(Path.Combine(directory, "trust.tsv")); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
