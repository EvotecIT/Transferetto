using System.Management.Automation;
using System.Net;
using System.Security;

namespace Transferetto.PowerShell;

internal static class SecureStringValue {
    internal static string Reveal(SecureString value) =>
        new NetworkCredential(string.Empty, value).Password;

    internal static string? RevealOptional(SecureString? value) =>
        value == null ? null : Reveal(value);

    internal static string SecretFrom(PSCredential credential) =>
        new NetworkCredential(credential.UserName, credential.Password).Password;
}
