using Microsoft.VisualBasic;
using Xunit;
using Xunit.Abstractions;

namespace Melfa.Robot.Tests;

[Collection("Robot integration")]
public class SimulatorMotionTests
{
    private readonly ITestOutputHelper output;

    public SimulatorMotionTests(ITestOutputHelper output) => this.output = output;

    [Fact(Skip = "Motion tests disabled")]
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

    [Fact(Skip = "Motion tests disabled")]
    public Task MotionSquare()
    {
        return Integration.Run(true, async (robot, _) =>
        {
            robot.OperationEnabled = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) == RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));

            robot.ServoOn = true;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) == RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            var current = robot.CurrentPositionP;
            var target = new PositionP(current.X + 50, current.Y, current.Z, current.A, current.B, current.C, current.L1, current.L2, current.FLG1, current.FLG2);
            output.WriteLine($"Current Cartesian position: {current}");

            robot.Execute($"MOV {target}");
            await AssertEventually(() => robot.DestinationPositionP.Equals(target), TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) == RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) != RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            output.WriteLine($"Current Cartesian position: {robot.CurrentPositionP}");

            robot.Execute($"MOV {current}");
            await AssertEventually(() => robot.DestinationPositionP.Equals(current), TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) == RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.Running) != RunStatus.Running, TimeSpan.FromMilliseconds(1000));
            output.WriteLine($"Current Cartesian position: {robot.CurrentPositionP}");

            robot.ServoOn = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.ServoOn) != RunStatus.ServoOn, TimeSpan.FromMilliseconds(1000));

            robot.OperationEnabled = false;
            await AssertEventually(() => (robot.GetState().RunStatus & RunStatus.OperationEnabled) != RunStatus.OperationEnabled, TimeSpan.FromMilliseconds(1000));
        });
    }

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