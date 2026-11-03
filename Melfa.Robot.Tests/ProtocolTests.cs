using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Melfa.Robot;
using Xunit;

namespace Melfa.Robot.Tests;

public class ProtocolTests
{
    [Fact]
    public async Task ReadsHandStatus()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;HNDSTS", await ReadCommand(stream));
            await Send(stream, "QoK900;1;0;902;2;1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1\r");
        });
        using var robot = server.Connect();
        var hands = robot.HandStates;
        Assert.Equal(8, hands.Length);
        Assert.Equal(900, hands[0].SignalNumber);
        Assert.True(hands[0].State);
        Assert.Equal(HandType.Single, hands[0].HandType);
        Assert.Equal(902, hands[1].SignalNumber);
        Assert.False(hands[1].State);
        Assert.Equal(HandType.Double, hands[1].HandType);
        Assert.Equal(-1, hands[2].SignalNumber);
        Assert.Null(hands[2].State);
        Assert.Equal(HandType.NotUsed, hands[2].HandType);
        await server.Complete();
    }

    [Fact]
    public async Task FormatsDirectMotionCommand()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXECMOV P1", await ReadCommand(stream));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        robot.Execute("MOV P1");
        await server.Complete();
    }

    [Fact]
    public async Task FormatsBlockingMotionCommandAndWaitsForResponse()
    {
        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXEC2=MOV P1", await ReadCommand(stream));
            received.SetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        var command = Task.Run(() => robot.ExecuteBlocking("MOV P1"));
        try
        {
            await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(command.IsCompleted);
        }
        finally { release.TrySetResult(true); }
        await command.WaitAsync(TimeSpan.FromSeconds(5));
        await server.Complete();
    }

    [Fact]
    public async Task BlockingMotionCommandSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXEC2=MOV P1", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => robot.ExecuteBlocking("MOV P1"));
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsCurrentJointPosition()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;JPOSF", await ReadCommand(stream));
            await Send(stream, "QoKJ1;+1.25;J2;-2.50;J3;3;J4;.5;J5;5.0;J6;6.0;;****,****;100;0.00;00000000\r");
        });
        using var robot = server.Connect();
        var position = robot.CurrentPositionJ;
        Assert.Equal(1.25, position.J1);
        Assert.Equal(-2.50, position.J2);
        Assert.Equal(6.0, position.J6);
        Assert.True(double.IsNaN(position.J7));
        Assert.True(double.IsNaN(position.J8));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsCurrentCartesianPosition()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PPOSF", await ReadCommand(stream));
            await Send(stream, "QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000\r");
        });
        using var robot = server.Connect();
        var position = robot.CurrentPositionP;
        Assert.Equal(290.62, position.X);
        Assert.Equal(-0.09, position.Y);
        Assert.Equal(11.26, position.Z);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsNamedVariableAndRejectsMismatchedResponse()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ1=(0,0,0,0,0,0,0,0)\r");
            Assert.Equal("1;1;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ2=(0,0,0,0,0,0,0,0)\r");
            Assert.Equal("1;9;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ1=(1,0,0,0,0,0,0,0)\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.ReadVariable("J1;STOP", 'J'));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.ReadVariable("J1", 'X'));
        Assert.Equal("(0,0,0,0,0,0,0,0)", robot.ReadVariable("J1", 'j'));
        Assert.Throws<InvalidDataException>(() => robot.ReadVariable("J1", 'J'));
        Assert.Equal("(1,0,0,0,0,0,0,0)", robot.ReadVariable("J1", 'J', 9));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsUnterminatedControllerResponses()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;OPEN=TEST", await ReadCommand(stream));
            await Send(stream, "QoK3F;3F;7,0;3,5,A,1E,32,46,64;MB6;PRM;RV-7FR-D;CR8xx-D;MELFA;17-11-29;Ver.A1dC;ENG;COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;5;8;");
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "QoKTEST.MB6");
            Assert.Equal("1;1;CLOSE", await ReadCommand(stream));
            await Send(stream, "QoK");
        });
        using var robot = server.Connect(requireTerminator: false);
        Assert.False(robot.RequireResponseTerminator);
        var info = robot.Open("TEST");
        Assert.Equal("RV-7FR-D", info.RobotType);
        Assert.Equal("MB6", info.ProgramExtension);
        Assert.Equal(8, info.MaximumTaskCount);
        Assert.Equal("TEST.MB6", robot.GetCurrentProgram());
        robot.Close();
        await server.Complete();
    }

    [Fact]
    public async Task ShortReadModeWaitsForCompleteResponsePrefixAndErrorNumber()
    {
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            await Send(stream, "Q");
            await Task.Delay(30);
            await Send(stream, "eR12");
            await Task.Delay(30);
            await Send(stream, "34");
        });
        using var robot = server.Connect(requireTerminator: false);
        var error = Assert.Throws<MelfaRobotException>(() => robot.GetCurrentProgram());
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

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

        public MelfaRobot Connect(bool requireTerminator = true)
        {
            var robot = new MelfaRobot("127.0.0.1", ((IPEndPoint)_listener.LocalEndpoint).Port);
            if (requireTerminator)
                robot.RequireResponseTerminator = true;
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
