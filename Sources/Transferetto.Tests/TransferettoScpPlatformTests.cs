using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Transferetto.Core;

namespace Transferetto.Tests;

public sealed class TransferettoScpPlatformTests : IDisposable {
    private readonly string _parent = Path.Combine(Path.GetTempPath(), "Transferetto.Tests", Guid.NewGuid().ToString("N"));
    public TransferettoScpPlatformTests() { Directory.CreateDirectory(_parent); }

    [WindowsFact]
    public async Task ReplacingAnExistingFilePreservesItsExplicitWindowsAccessRules() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        string target = Path.Combine(selected, "target");
        File.WriteAllText(target, "original");
        FileSecurity security = new();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.FullControl, AccessControlType.Allow));
        new FileInfo(target).SetAccessControl(security);
        string expected = new FileInfo(target).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.Access);
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0E\n"));
        using MemoryStream acks = new();
        await new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None);
        Assert.Equal("new", File.ReadAllText(target));
        Assert.Equal(expected, new FileInfo(target).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.Access));
        Assert.Equal(new[] { selected }, Directory.GetFileSystemEntries(_parent));
    }

    [WindowsFact]
    public async Task NewFilesInheritTheProtectedDestinationDirectoryAccessRules() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        SetWindowsDirectoryRules(selected, broad: true);
        string child = Path.Combine(selected, "child");
        Directory.CreateDirectory(child);
        SetWindowsDirectoryRules(child, broad: false);
        string expectedFile = Path.Combine(child, "expected");
        File.WriteAllText(expectedFile, "control");
        string expected = new FileInfo(expectedFile).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.Access);
        File.Delete(expectedFile);
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nD0755 0 child\nC0644 3 target\nnew\0E\nE\n"));
        using MemoryStream acks = new();
        await new ScpDirectoryReceiver(records, acks, selected).ReceiveAsync(CancellationToken.None);
        string target = Path.Combine(child, "target");
        Assert.Equal("new", File.ReadAllText(target));
        Assert.Equal(expected, new FileInfo(target).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.Access));
    }

    [WindowsFact]
    public async Task IncompleteContentIsStagedWithoutTheRootsBroadReadRules() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        SetWindowsDirectoryRules(selected, broad: true);
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0E\n"));
        using MemoryStream acks = new();
        bool observed = false;
        ScpDirectoryReceiver receiver = new(records, acks, selected, (_, _, _) => {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
            string stage = Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*"));
            FileSecurity security = new FileInfo(Assert.Single(Directory.GetFiles(stage))).GetAccessControl();
            SecurityIdentifier broad = new(WellKnownSidType.BuiltinUsersSid, null);
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier))) {
                Assert.NotEqual(broad, rule.IdentityReference);
            }
            Assert.True(new DirectoryInfo(stage).GetAccessControl().AreAccessRulesProtected);
            observed = true;
        });
        await receiver.ReceiveAsync(CancellationToken.None);
        Assert.True(observed);
        Assert.Equal("new", File.ReadAllText(Path.Combine(selected, "target")));
        Assert.Single(Directory.GetFileSystemEntries(selected));
    }

    private static void SetWindowsDirectoryRules(string directory, bool broad) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        DirectorySecurity security = new();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        InheritanceFlags inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
        security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
        if (broad) {
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.ReadAndExecute, inheritance, PropagationFlags.None, AccessControlType.Allow));
        }
        new DirectoryInfo(directory).SetAccessControl(security);
    }

    [WindowsFact]
    public void ReplacementStagingGetsTheOriginalFilesAccessRulesBeforeContentIsWritten() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        SetWindowsDirectoryRules(_parent, broad: true);
        string target = Path.Combine(_parent, "target");
        string staged = Path.Combine(_parent, "staged");
        File.WriteAllText(target, "original");
        FileSecurity security = new();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.FullControl, AccessControlType.Allow));
        new FileInfo(target).SetAccessControl(security);
        using FileStream output = new(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        TransferFileSystem.PreserveStagingPermissions(staged, target);
        Assert.Equal(0, output.Length);
        FileSecurity stagedSecurity = new FileInfo(staged).GetAccessControl();
        Assert.True(stagedSecurity.AreAccessRulesProtected);
        FileSystemAccessRule expected = Assert.Single(new FileInfo(target).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>());
        FileSystemAccessRule actual = Assert.Single(stagedSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>());
        Assert.Equal(expected.IdentityReference, actual.IdentityReference);
        Assert.Equal(expected.FileSystemRights, actual.FileSystemRights);
        Assert.Equal(expected.AccessControlType, actual.AccessControlType);
        Assert.Equal("original", File.ReadAllText(target));
    }

#if NET8_0_OR_GREATER
    [UnixScpFact]
    public async Task OpenSshSourceCompletesTheDirectoryHandshakeAndPreservesLiteralNames() {
        string source = Path.Combine(_parent, "source");
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(Path.Combine(source, "empty"));
        byte[] content = { 0, 10, 255 };
        File.WriteAllBytes(Path.Combine(source, @"literal\name.bin"), content);
        Directory.CreateDirectory(selected);
        File.WriteAllText(Path.Combine(selected, "unrelated"), "keep");
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(30));
        ProcessStartInfo start = new("/usr/bin/scp") {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false
        };
        start.ArgumentList.Add("-prf");
        start.ArgumentList.Add(source);
        using Process process = Process.Start(start)!;
        Task<string> error = process.StandardError.ReadToEndAsync(deadline.Token);
        try {
            ScpDirectoryReceiver receiver = new(process.StandardOutput.BaseStream, process.StandardInput.BaseStream, selected);
            await receiver.ReceiveAsync(deadline.Token, async () => {
                process.StandardInput.Close();
                await process.WaitForExitAsync(deadline.Token);
                Assert.True(process.ExitCode == 0, await error);
            });
            Assert.Equal(content, File.ReadAllBytes(Path.Combine(selected, @"literal\name.bin")));
            Assert.Equal("keep", File.ReadAllText(Path.Combine(selected, "unrelated")));
            Assert.Empty(Directory.GetFileSystemEntries(Path.Combine(selected, "empty")));
            Assert.Equal(3, Directory.GetFileSystemEntries(selected).Length);
            Assert.Equal(2, Directory.GetFileSystemEntries(_parent).Length);
        } finally {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
        }
    }

    [UnixFact]
    public async Task ReplacementPreservesRestrictiveUnixPermissions() {
        if (OperatingSystem.IsWindows()) { throw new PlatformNotSupportedException(); }
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        string target = Path.Combine(selected, "target");
        File.WriteAllText(target, "original");
        File.SetUnixFileMode(target, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        using MemoryStream records = new(Encoding.UTF8.GetBytes("D0755 0 root\nC0644 3 target\nnew\0E\n"));
        using MemoryStream acks = new();
        await new ScpDirectoryReceiver(records, acks, selected, (_, _, _) => {
            if (OperatingSystem.IsWindows()) { throw new PlatformNotSupportedException(); }
            string stage = Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*"));
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(stage));
        }).ReceiveAsync(CancellationToken.None);
        Assert.Equal("new", File.ReadAllText(target));
        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(target));
    }

    private class UnixFactAttribute : FactAttribute {
        public UnixFactAttribute() { if (OperatingSystem.IsWindows()) { Skip = "Requires a Unix filesystem."; } }
    }
    private sealed class UnixScpFactAttribute : UnixFactAttribute {
        public UnixScpFactAttribute() { if (!File.Exists("/usr/bin/scp")) { Skip = "Requires the system OpenSSH SCP source for protocol proof."; } }
    }
#endif

    [WindowsFact]
    public async Task RollbackMetadataFailurePreservesThePublicationErrorAndRecoveryDirectory() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { throw new PlatformNotSupportedException(); }
        string selected = Path.Combine(_parent, "download");
        Directory.CreateDirectory(selected);
        string target = Path.Combine(selected, "target");
        File.WriteAllText(target, "original");
        DirectoryInfo directory = new(selected);
        DirectorySecurity originalRules = directory.GetAccessControl();
        DirectorySecurity deniedRules = directory.GetAccessControl();
        // Real filesystem failure: implicit content updates remain allowed, but both
        // publication and rollback are denied explicit directory timestamp updates.
        deniedRules.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!,
            FileSystemRights.WriteAttributes, AccessControlType.Deny));
        using ScpDirectoryTransaction transaction = new(selected);
        transaction.AddDirectory("", DateTime.UtcNow.AddDays(-1), null);
        using MemoryStream content = new(Encoding.UTF8.GetBytes("new"));
        await transaction.AddFileAsync("target", content, content.Length, null, null, CancellationToken.None);
        try {
            directory.SetAccessControl(deniedRules);
            UnauthorizedAccessException actual = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => transaction.CommitAsync(CancellationToken.None));
            Assert.IsType<UnauthorizedAccessException>(actual.Data["Transferetto.RollbackException"]);
            string recovery = Assert.IsType<string>(actual.Data["Transferetto.RecoveryDirectory"]);
            Assert.Equal(Assert.Single(Directory.GetDirectories(selected, ".transferetto-scp-*")), recovery);
            transaction.Dispose();
            Assert.True(Directory.Exists(recovery));
            Assert.Equal("original", File.ReadAllText(target));
        } finally {
            directory.SetAccessControl(originalRules);
        }
    }

    private sealed class WindowsFactAttribute : FactAttribute {
        public WindowsFactAttribute() { if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { Skip = "Requires Windows file access rules."; } }
    }

    public void Dispose() { Directory.Delete(_parent, recursive: true); }
}
