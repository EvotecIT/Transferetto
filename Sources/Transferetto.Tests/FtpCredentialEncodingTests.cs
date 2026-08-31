using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Transferetto.Tests;

public sealed class FtpCredentialEncodingTests {
    [Fact]
    public async Task ConnectFtpUsesConfiguredEncodingForPasswordCommand() {
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        try {
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            TaskCompletionSource<byte[]> passwordCommand = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Task serverTask = ServeFtpSessionAsync(listener, passwordCommand);
            TransferettoFtpConnectionOptions options = new() {
                Server = IPAddress.Loopback.ToString(),
                Port = port,
                Credential = new NetworkCredential("someuser", "Some§Password"),
                EncodingName = "windows-1252"
            };

            using (TransferettoFtpSession session = TransferettoClient.ConnectFtp(options)) {
                byte[] actualPasswordBytes = await AwaitWithTimeoutAsync(passwordCommand.Task);
                byte[] expectedPasswordBytes = Encoding.GetEncoding("windows-1252").GetBytes("Some§Password");

                Assert.True(session.IsConnected);
                Assert.Equal(expectedPasswordBytes, actualPasswordBytes);
                Assert.Contains((byte) 0xA7, actualPasswordBytes);
                Assert.DoesNotContain((byte) '?', actualPasswordBytes);
            }

            await AwaitWithTimeoutAsync(serverTask);
        } finally {
            listener.Stop();
        }
    }

    private static async Task ServeFtpSessionAsync(
        TcpListener listener,
        TaskCompletionSource<byte[]> passwordCommand) {
        using TcpClient connection = await listener.AcceptTcpClientAsync();
        using NetworkStream stream = connection.GetStream();
        await WriteResponseAsync(stream, "220 Transferetto test server ready\r\n");

        while (true) {
            byte[]? commandBytes = await ReadCommandAsync(stream);
            if (commandBytes is null) {
                return;
            }

            string command = Encoding.ASCII.GetString(commandBytes);
            if (command.StartsWith("USER ", StringComparison.Ordinal)) {
                await WriteResponseAsync(stream, "331 Password required\r\n");
            } else if (command.StartsWith("PASS ", StringComparison.Ordinal)) {
                passwordCommand.TrySetResult(commandBytes.Skip(5).ToArray());
                await WriteResponseAsync(stream, "230 Logged in\r\n");
            } else if (command.Equals("FEAT", StringComparison.Ordinal)) {
                await WriteResponseAsync(stream, "211 No features\r\n");
            } else if (command.Equals("SYST", StringComparison.Ordinal)) {
                await WriteResponseAsync(stream, "215 UNIX Type: L8\r\n");
            } else if (command.Equals("PWD", StringComparison.Ordinal)) {
                await WriteResponseAsync(stream, "257 \"/\" is current directory\r\n");
            } else if (command.Equals("QUIT", StringComparison.Ordinal)) {
                await WriteResponseAsync(stream, "221 Goodbye\r\n");
                return;
            } else {
                await WriteResponseAsync(stream, "200 OK\r\n");
            }
        }
    }

    private static async Task<byte[]?> ReadCommandAsync(NetworkStream stream) {
        using MemoryStream command = new();
        byte[] buffer = new byte[1];

        while (true) {
            int count = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (count == 0) {
                return command.Length == 0 ? null : command.ToArray();
            }

            if (buffer[0] == (byte) '\n') {
                byte[] bytes = command.ToArray();
                if (bytes.Length > 0 && bytes[bytes.Length - 1] == (byte) '\r') {
                    Array.Resize(ref bytes, bytes.Length - 1);
                }

                return bytes;
            }

            command.WriteByte(buffer[0]);
        }
    }

    private static Task WriteResponseAsync(NetworkStream stream, string response) {
        byte[] bytes = Encoding.ASCII.GetBytes(response);
        return stream.WriteAsync(bytes, 0, bytes.Length);
    }

    private static async Task AwaitWithTimeoutAsync(Task task) {
        Task completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        if (!ReferenceEquals(completedTask, task)) {
            throw new TimeoutException("The loopback FTP exchange did not complete within 10 seconds.");
        }

        await task;
    }

    private static async Task<T> AwaitWithTimeoutAsync<T>(Task<T> task) {
        Task completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        if (!ReferenceEquals(completedTask, task)) {
            throw new TimeoutException("The loopback FTP exchange did not complete within 10 seconds.");
        }

        return await task;
    }
}
