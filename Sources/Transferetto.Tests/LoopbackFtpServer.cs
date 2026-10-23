using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Transferetto.Tests;

/// <summary>A task-owned loopback FTP peer: one real file and a subsequent unavailable file.</summary>
internal sealed class LoopbackFtpServer : IDisposable {
    private readonly TcpListener _control = new(IPAddress.Loopback, 0);
    private readonly Task _run;
    private TcpClient? _client;
    private TcpListener? _data;
    private readonly bool _failSecond;
    private readonly bool _shortSecond;
    private readonly bool _emptySecond;
    private readonly string _shortContent;
    private readonly bool _knownSize;
    private int _restart;
    internal ConcurrentQueue<string> Commands { get; } = new();
    internal int Port { get; }

    internal LoopbackFtpServer(bool failSecond = true, bool shortSecond = false, bool emptySecond = false,
        string shortContent = "n", bool knownSize = true) {
        _failSecond = failSecond;
        _shortSecond = shortSecond;
        _emptySecond = emptySecond;
        _shortContent = shortContent;
        _knownSize = knownSize;
        _control.Start();
        Port = ((IPEndPoint)_control.LocalEndpoint).Port;
        _run = RunAsync();
    }

    private async Task RunAsync() {
        try {
            using TcpClient client = await _control.AcceptTcpClientAsync();
            _client = client;
            using NetworkStream stream = client.GetStream();
            using StreamReader input = new(stream, Encoding.UTF8, false, 1024, leaveOpen: true);
            using StreamWriter output = new(stream, new UTF8Encoding(false), 1024, leaveOpen: true) { AutoFlush = true, NewLine = "\r\n" };
            await output.WriteLineAsync("220 Loopback transfer regression");
            string directory = "/root";
            while (await input.ReadLineAsync() is string command) {
                string[] parts = command.Split(new[] { ' ' }, 2);
                string verb = parts[0].ToUpperInvariant();
                string argument = parts.Length == 2 ? parts[1] : "";
                Commands.Enqueue(verb == "PASS" ? "PASS [synthetic]" : command);
                switch (verb) {
                    case "USER": await output.WriteLineAsync("331 Password required"); break;
                    case "PASS": await output.WriteLineAsync("230 Logged in"); break;
                    case "SYST": await output.WriteLineAsync("215 UNIX Type: L8"); break;
                    case "FEAT":
                        await output.WriteLineAsync("211-Features\r\n MLST type*;size*;modify*;\r\n UTF8\r\n SIZE\r\n211 End"); break;
                    case "PWD": await output.WriteLineAsync("257 \"" + directory + "\" is current directory"); break;
                    case "CWD": directory = argument.TrimEnd('/'); await output.WriteLineAsync("250 Directory changed"); break;
                    case "TYPE": case "OPTS": case "NOOP": await output.WriteLineAsync("200 Accepted"); break;
                    case "REST": _restart = int.Parse(argument, System.Globalization.CultureInfo.InvariantCulture);
                        await output.WriteLineAsync("350 Restart accepted"); break;
                    case "SIZE": await output.WriteLineAsync(!_knownSize ? "550 Size unavailable" :
                        _emptySecond && argument.EndsWith("second", StringComparison.Ordinal) ? "213 0" : "213 3"); break;
                    case "EPSV": case "PASV":
                        _data?.Stop();
                        _data = new TcpListener(IPAddress.Loopback, 0); _data.Start();
                        int port = ((IPEndPoint)_data.LocalEndpoint).Port;
                        await output.WriteLineAsync(verb == "EPSV" ? "229 Entering Extended Passive Mode (|||" + port + "|)"
                            : "227 Entering Passive Mode (127,0,0,1," + port / 256 + "," + port % 256 + ")"); break;
                    case "MLSD": case "LIST":
                        await SendDataAsync(output, verb == "MLSD"
                            ? "type=file;size=3;modify=20200102030405; first\r\ntype=file;size=" + (_emptySecond ? "0" : "3") + ";modify=20200102030405; second\r\n"
                            : "-rw-r--r-- 1 user group 3 Jan 02 2020 first\r\n-rw-r--r-- 1 user group " + (_emptySecond ? "0" : "3") + " Jan 02 2020 second\r\n"); break;
                    case "RETR":
                        if (_failSecond && argument.EndsWith("second", StringComparison.Ordinal)) { _data?.Stop(); await output.WriteLineAsync("550 File unavailable"); }
                        else {
                            bool second = argument.EndsWith("second", StringComparison.Ordinal);
                            string content = second && _emptySecond ? "" : second && _shortSecond ? _shortContent : "new";
                            content = content.Substring(Math.Min(_restart, content.Length));
                            _restart = 0; await SendDataAsync(output, content);
                        }
                        break;
                    case "QUIT": await output.WriteLineAsync("221 Goodbye"); return;
                    default: await output.WriteLineAsync("502 Unsupported command"); break;
                }
            }
        } catch (Exception exception) when (exception is IOException || exception is SocketException || exception is ObjectDisposedException) {
            // Closing only our sockets releases pending accepts/reads during teardown.
        }
    }

    private async Task SendDataAsync(StreamWriter output, string content) {
        await output.WriteLineAsync("150 Opening data connection");
        using TcpClient data = await _data!.AcceptTcpClientAsync();
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        using (NetworkStream stream = data.GetStream()) { await stream.WriteAsync(bytes, 0, bytes.Length); }
        _data.Stop();
        await output.WriteLineAsync("226 Transfer complete");
    }

    public void Dispose() {
        _data?.Stop(); _client?.Dispose(); _control.Stop();
        if (!_run.Wait(TimeSpan.FromSeconds(5))) { throw new TimeoutException("The owned FTP peer did not exit."); }
    }
}
