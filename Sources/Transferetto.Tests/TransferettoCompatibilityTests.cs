namespace Transferetto.Tests;

public sealed class TransferettoCompatibilityTests {
    private static readonly string[] ForwardedProtocolTypes = {
        "Transferetto.TransferettoClient",
        "Transferetto.TransferettoFtpCertificateInfo",
        "Transferetto.TransferettoFtpCertificatePolicy",
        "Transferetto.TransferettoFtpCertificateTrustSource",
        "Transferetto.TransferettoFtpConnectionOptions",
        "Transferetto.TransferettoFtpProxyOptions",
        "Transferetto.TransferettoFtpProxyType",
        "Transferetto.TransferettoFtpSession",
        "Transferetto.TransferettoFtpStreamMode",
        "Transferetto.TransferettoFtpStreamReadResult",
        "Transferetto.TransferettoFtpStreamSession",
        "Transferetto.TransferettoFtpStreamWriteResult",
        "Transferetto.TransferettoFtpTraceOptions",
        "Transferetto.TransferettoFxpPreflightResult",
        "Transferetto.TransferettoFxpTransferKind",
        "Transferetto.TransferettoOperationResult",
        "Transferetto.TransferettoRemoteItem",
        "Transferetto.TransferettoRuntimeSettings",
        "Transferetto.TransferettoScpSession",
        "Transferetto.TransferettoSftpAttributes",
        "Transferetto.TransferettoSftpConnectionOptions",
        "Transferetto.TransferettoSftpItem",
        "Transferetto.TransferettoSftpSession",
        "Transferetto.TransferettoSftpStreamMode",
        "Transferetto.TransferettoSftpStreamReadResult",
        "Transferetto.TransferettoSftpStreamSession",
        "Transferetto.TransferettoSftpStreamWriteResult",
        "Transferetto.TransferettoSshCommandOptions",
        "Transferetto.TransferettoSshCommandOutputChunk",
        "Transferetto.TransferettoSshCommandOutputStream",
        "Transferetto.TransferettoSshCommandResult",
        "Transferetto.TransferettoSshConnectionOptions",
        "Transferetto.TransferettoSshHostKeyInfo",
        "Transferetto.TransferettoSshHostKeyPolicy",
        "Transferetto.TransferettoSshHostKeyTrustSource",
        "Transferetto.TransferettoSshProxyType",
        "Transferetto.TransferettoSshSession",
        "Transferetto.TransferettoSshShellCommandResult",
        "Transferetto.TransferettoSshShellControlKey",
        "Transferetto.TransferettoSshShellExpectResult",
        "Transferetto.TransferettoSshShellExpectStep",
        "Transferetto.TransferettoSshShellExpectStepResult",
        "Transferetto.TransferettoSshShellOptions",
        "Transferetto.TransferettoSshShellOutputChunk",
        "Transferetto.TransferettoSshShellPromptPreset",
        "Transferetto.TransferettoSshShellReadOptions",
        "Transferetto.TransferettoSshShellRecipeKind",
        "Transferetto.TransferettoSshShellRecipeOptions",
        "Transferetto.TransferettoSshShellRecipeResult",
        "Transferetto.TransferettoSshShellSession",
        "Transferetto.TransferettoSshShellTranscriptDirection",
        "Transferetto.TransferettoSshShellTranscriptEntry",
        "Transferetto.TransferettoSshShellTranscriptSnapshot",
        "Transferetto.TransferettoSshTunnelSession",
        "Transferetto.TransferettoSshTunnelType",
        "Transferetto.TransferettoSyncAction",
        "Transferetto.TransferettoSyncComparison",
        "Transferetto.TransferettoSyncDirection",
        "Transferetto.TransferettoSyncEntry",
        "Transferetto.TransferettoSyncMode",
        "Transferetto.TransferettoSyncOptions",
        "Transferetto.TransferettoSyncPlanItem",
        "Transferetto.TransferettoSyncPlanner",
        "Transferetto.TransferettoSyncResult",
        "Transferetto.TransferettoTransferDirection",
        "Transferetto.TransferettoTransferOptions",
        "Transferetto.TransferettoTransferProgress",
        "Transferetto.TransferettoTransferResult"
    };

    [Fact]
    public void CompatibilityAssembly_ForwardsRepresentativeLegacyTypes() {
        foreach (string typeName in ForwardedProtocolTypes) {
            Type forwarded = Type.GetType($"{typeName}, Transferetto", throwOnError: true)!;

            Assert.Equal("Transferetto.Protocols", forwarded.Assembly.GetName().Name);
        }
    }
}
