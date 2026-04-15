using System;

namespace Transferetto.Cli;

internal static class Program {
    private static void Main(string[] args) {
        Console.WriteLine("Transferetto.Cli is the future automation surface for SSH, FTP, and SFTP workflows.");
        Console.WriteLine("Planned command groups: ftp, sftp, ssh, server, and deploy.");
        Console.WriteLine($"Arguments received: {string.Join(' ', args)}");
    }
}
