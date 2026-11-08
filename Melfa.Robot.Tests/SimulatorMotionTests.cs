using Microsoft.VisualBasic;
using Xunit;
using Xunit.Abstractions;

namespace Melfa.Robot.Tests;

[Collection("Robot integration")]
public class SimulatorMotionTests
{
    private readonly ITestOutputHelper output;

    public SimulatorMotionTests(ITestOutputHelper output) => this.output = output;

    [WriteFact(Skip = "Motion tests disabled")]
    public Task ServoOnOff()
    {
        return Integration.Run(true, async (robot, _) =>
        {
            robot.OperationEnabled = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) == RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));

            robot.ServoOn = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) == RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            robot.ServoOn = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) != RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            robot.OperationEnabled = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) != RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));

        });
    }

    [WriteFact(Skip = "Motion tests disabled")]
    public Task MotionSquare()
    {
        return Integration.Run(true, async (robot, _) =>
        {
            robot.OperationEnabled = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) == RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));

            robot.ResetAlarm();

            robot.ServoOn = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) == RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            var current = robot.CurrentPositionP;
            var target = new PositionP(current.X + 50, current.Y, current.Z, current.A, current.B, current.C, current.L1, current.L2, current.FLG1, current.FLG2);
            output.WriteLine($"Current Cartesian position: {current}");
            output.WriteLine($"Target Cartesian position: {target}");

            robot.Execute($"MOV {target}");
            await AssertEventually(() => robot.DestinationPositionP.Equals(target), TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) == RunStatus.Running, TimeSpan.FromMilliseconds(5000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) != RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            output.WriteLine($"Current Cartesian position: {robot.CurrentPositionP}");

            robot.Execute($"MOV {current}");
            await AssertEventually(() => robot.DestinationPositionP.Equals(current), TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) == RunStatus.Running, TimeSpan.FromMilliseconds(5000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) != RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            output.WriteLine($"Current Cartesian position: {robot.CurrentPositionP}");

            robot.ServoOn = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) != RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            robot.OperationEnabled = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) != RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));
        });
    }

    [Fact(Skip = "Motion tests disabled")]
    public void JointMoveWithExecuteBlocking() => Integration.Run(true, (robot, _) =>
    {
        var joints = (PositionJ[])
        [
            // Phase 1: Baseline and individual joint movements
            new PositionJ(   0,  -60,  90,    0,   60,    0),
            new PositionJ(  45,  -60,  90,    0,   60,    0),
            new PositionJ( -45,  -60,  90,    0,   60,    0),
            new PositionJ(   0,  -30,  90,    0,   60,    0),
            new PositionJ(   0,  -90,  90,    0,   60,    0),
            new PositionJ(   0,  -60,  55,    0,   60,    0),
            new PositionJ(   0,  -60, 125,    0,   60,    0),
            new PositionJ(   0,  -60,  90,   60,   60,    0),
            new PositionJ(   0,  -60,  90,  -60,   60,    0),
            new PositionJ(   0,  -60,  90,    0,   25,    0),
            new PositionJ(   0,  -60,  90,    0,   95,    0),
            new PositionJ(   0,  -60,  90,    0,   60,   90),
            new PositionJ(   0,  -60,  90,    0,   60,  -90),

            // Phase 2: Combined joint movements
            new PositionJ(  35,  -45, 110,   45,   45,  -60),
            new PositionJ( -35,  -75,  70,  -45,   75,   60),
            new PositionJ(  80,  -50, 100,   90,   30,  120),
            new PositionJ( -80,  -40, 120,  -90,   90, -120),
            new PositionJ( 120,  -80,  80,   30,  -30,  180),
            new PositionJ(-120,  -20,  60, -120,   45,  240),
            new PositionJ(  20, -100, 130,  150,  -60, -240),
        ];

        robot.OperationEnabled = true;
        robot.ResetAlarm();
        Thread.Sleep(500);
        robot.ServoOn = true;
        Thread.Sleep(500);

        for (var i = 0; i < joints.Length; i++)
        {
            var j = joints[i];

            output.WriteLine($"#{i + 1:D2}");

            try
            {
                var cmd = $"DEF J1={j};P1=JtoP(J1);";
                robot.ExecuteBlocking(cmd);
                var p = robot.ReadVariable("P_POS", VariableClass.CartesianPosition);

                output.WriteLine($"J: {j}");
                output.WriteLine($"P: {p}");

                //output.WriteLine($"J: {robot.CurrentPositionJ}");
                //output.WriteLine($"P: {robot.CurrentPositionP}");
                if (robot.Error is not null)
                {
                    output.WriteLine($"{robot.Error.Message}");

                    robot.ResetAlarm();
                    Thread.Sleep(500);
                    robot.ServoOn = true;
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"FAILED: {ex.Message}");
            }
        }

        robot.ServoOn = false;
        robot.OperationEnabled = false;
    });

    internal static async Task AssertEventually(Func<bool> condition, TimeSpan timeout, TimeSpan? interval = null)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;

            await Task.Delay(interval ?? TimeSpan.FromMilliseconds(100));
        }

        Assert.True(condition(), "Condition was not satisfied before timeout.");
    }
}
