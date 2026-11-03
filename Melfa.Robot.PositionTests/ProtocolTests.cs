using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Melfa.Robot;
using Xunit;

namespace Melfa.Robot.PositionTests;

public class ProtocolTests
{
    [Fact]
    public async Task ReadsFragmentedResponsesLargerThanTheOldBuffer()
    {
        var payload = new string('A', 1000);
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "Qo");
            await Task.Delay(30);
            await Send(stream, "K" + payload[..500]);
            await Task.Delay(30);
            await Send(stream, payload[500..] + "\r");
        });
        using var robot = server.Connect();
        Assert.Equal(payload, robot.GetCurrentProgram());
        await server.Complete();
    }

    [Fact]
    public async Task TimeoutInvalidatesConnection()
    {
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            await Send(stream, "QoKpartial");
            await Task.Delay(500);
        });
        using var robot = server.Connect();
        robot.CommandTimeout = 100;
        Assert.ThrowsAny<IOException>(() => robot.GetCurrentProgram());
        Assert.False(robot.IsConnected);
        await server.Complete();
    }

    [Fact]
    public async Task RejectsInvalidCommandsWithoutSendingOrDisconnecting()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "QoKREADY\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.Execute("MOV P1\rSTOP"));
        Assert.Throws<ArgumentException>(() => robot.Execute("MOV P1\nSTOP"));
        Assert.Throws<EncoderFallbackException>(() => robot.Execute("\u00e9"));
        Assert.Throws<InvalidOperationException>(() => robot.Execute(new string('A', 255)));
        Assert.Equal("READY", robot.GetCurrentProgram());
        await server.Complete();
    }

    [Fact]
    public async Task DisconnectInterruptsPendingRead()
    {
        var requestReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            requestReceived.SetResult(true);
            await Task.Delay(500);
        });
        using var robot = server.Connect();
        var pending = Task.Run(() => Record.Exception(() => robot.GetCurrentProgram()));
        await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        robot.Disconnect();
        var exception = await pending.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.NotNull(exception);
        Assert.False(robot.IsConnected);
        await server.Complete();
    }

    [Fact]
    public async Task FormatsCommandsAndReadsStartEligibility()
    {
        string[] commands =
        {
            "1;1;EMDAT10 MOV P1\u000b20 MOV P2",
            "1;1;JOG01;00;01;02;00",
            "1;1;BRKPTSET=TEST;20;1",
            "1;1;FOPENTEST.MB5;w",
            "1;1;FWRITE2;0102",
            "1;1;FCLOSE",
            "1;1;ATENA",
            "1;1;ATENA",
            "1;1;LISTPP1"
        };
        using var server = new Controller(async stream =>
        {
            for (var i = 0; i < commands.Length; i++)
            {
                Assert.Equal(commands[i], await ReadCommand(stream));
                var response = i switch
                {
                    6 => "QoK0\r",
                    7 => "QoK1\r",
                    8 => "QoKP1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)\r",
                    _ => "QoK\r"
                };
                await Send(stream, response);
            }
        });
        using var robot = server.Connect();
        robot.ProgramEdit(10, "MOV P1\r\n20 MOV P2");
        robot.Jog(JogMode.XYZ, 1, 2);
        robot.SetBreakpoint("TEST", 20, BreakpointType.OneTime);
        using var data = new MemoryStream(new byte[] { 1, 2 });
        robot.FileWrite("TEST.MB5", data);
        Assert.True(robot.ProgramStart);
        Assert.False(robot.ProgramStart);
        Assert.Equal(400.13, Assert.IsType<PositionP>(robot.ProgramGetPosition("P1")).X);
        await server.Complete();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListsAllParameterPages(bool multiplePages)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PMR=0", await ReadCommand(stream));
            await Send(stream, multiplePages ? "QoK1;1;FIRST\r" : "QoK0;1;ONLY\r");
            if (multiplePages)
            {
                Assert.Equal("1;1;PMR=1", await ReadCommand(stream));
                await Send(stream, "QoK0;1;LAST\r");
            }
        });
        using var robot = server.Connect();
        Assert.Equal(multiplePages ? new[] { "FIRST", "LAST" } : new[] { "ONLY" }, robot.ListChangedParameters().ToArray());
        await server.Complete();
    }

    [Fact]
    public async Task FileTransfersCannotOpenAnotherFileWhileAStreamIsBeingRead()
    {
        var firstRead = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new ManualResetEventSlim();
        var openedSecond = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<string>();
        using var server = new Controller(async stream =>
        {
            for (var i = 0; i < 6; i++)
            {
                var command = await ReadCommand(stream);
                commands.Enqueue(command);
                if (command == "1;1;FOPENSECOND;w")
                    openedSecond.TrySetResult(true);
                await Send(stream, "QoK\r");
            }
        });
        using var robot = server.Connect();
        using var first = new BlockingStream(firstRead, release);
        using var second = new MemoryStream(new byte[] { 2 });
        var writeFirst = Task.Run(() => robot.FileWrite("FIRST", first));
        try
        {
            await firstRead.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var writeSecond = Task.Run(() => robot.FileWrite("SECOND", second));
            Assert.NotSame(openedSecond.Task, await Task.WhenAny(openedSecond.Task, Task.Delay(200)));
            release.Set();
            await Task.WhenAll(writeFirst, writeSecond).WaitAsync(TimeSpan.FromSeconds(5));
            await server.Complete();
            Assert.Equal(new[]
            {
                "1;1;FOPENFIRST;w", "1;1;FWRITE1;01", "1;1;FCLOSE",
                "1;1;FOPENSECOND;w", "1;1;FWRITE1;02", "1;1;FCLOSE"
            }, commands.ToArray());
        }
        finally { release.Set(); }
    }

    private static async Task<string> ReadCommand(NetworkStream stream)
    {
        using var bytes = new MemoryStream();
        var buffer = new byte[1];
        while (await stream.ReadAsync(buffer) != 0)
        {
            if (buffer[0] == '\r')
                return Encoding.ASCII.GetString(bytes.ToArray());
            bytes.WriteByte(buffer[0]);
        }
        throw new EndOfStreamException("Command was not terminated.");
    }

    private static async Task Send(NetworkStream stream, string response) =>
        await stream.WriteAsync(Encoding.ASCII.GetBytes(response));

    private sealed class Controller : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly Task _server;
        private TcpClient? _client;

        public Controller(Func<NetworkStream, Task> script)
        {
            _listener.Start();
            _server = Task.Run(async () =>
            {
                using var client = await _listener.AcceptTcpClientAsync();
                _client = client;
                await script(client.GetStream());
            });
        }

        public MelfaRobot Connect()
        {
            var robot = new MelfaRobot("127.0.0.1", ((IPEndPoint)_listener.LocalEndpoint).Port);
            robot.Connect();
            return robot;
        }

        public Task Complete() => _server.WaitAsync(TimeSpan.FromSeconds(5));

        public void Dispose()
        {
            _client?.Dispose();
            _listener.Stop();
        }
    }

    private sealed class BlockingStream : MemoryStream
    {
        private readonly TaskCompletionSource<bool> _entered;
        private readonly ManualResetEventSlim _release;

        public BlockingStream(TaskCompletionSource<bool> entered, ManualResetEventSlim release) : base(new byte[] { 1 })
        {
            _entered = entered;
            _release = release;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _entered.TrySetResult(true);
            if (!_release.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("Timed out waiting to release the input stream.");
            return base.Read(buffer, offset, count);
        }
    }
}
