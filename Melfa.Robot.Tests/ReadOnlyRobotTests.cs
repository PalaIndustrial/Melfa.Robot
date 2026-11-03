using Xunit;
using Xunit.Abstractions;

namespace Melfa.Robot.Tests;

[Collection("Robot integration")]
public class ReadOnlyRobotTests
{
    private readonly ITestOutputHelper output;

    public ReadOnlyRobotTests(ITestOutputHelper output) => this.output = output;

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    public void OpenAndReadSelectedProgram()
    {
        Integration.Run(false, (robot, _) =>
        {
            Assert.NotNull(robot.CurrentProgram(1));
            Assert.True(robot.TaskCount > 0);
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    public void ReadCurrentCartesianPosition()
    {
        Integration.Run(false, (robot, _) =>
        {
            var position = robot.CurrentPosition();
            output.WriteLine($"Current Cartesian position: {position}");
            Assert.True(double.IsFinite(position.X), "X must be finite.");
            Assert.True(double.IsFinite(position.Y), "Y must be finite.");
            Assert.True(double.IsFinite(position.Z), "Z must be finite.");
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    public void ReadCurrentJointPosition()
    {
        Integration.Run(false, (robot, _) =>
        {
            var position = robot.CurrentJointPosition();
            output.WriteLine($"Current joint position: {position}");
            var joints = new[] { position.J1, position.J2, position.J3, position.J4,
                                 position.J5, position.J6, position.J7, position.J8 };
            Assert.NotEqual(0, robot.JointAxisPattern);
            for (var i = 0; i < joints.Length; i++)
                if ((robot.JointAxisPattern & (1 << i)) != 0)
                    Assert.True(double.IsFinite(joints[i]), $"J{i + 1} must be finite.");
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    public void ReadHandStatus()
    {
        Integration.Run(false, (robot, _) =>
        {
            var hands = robot.HandStates();
            Assert.Equal(8, hands.Length);
            for (var i = 0; i < hands.Length; i++)
            {
                var hand = hands[i];
                var state = hand.State switch { true => "open", false => "closed", null => "unknown" };
                output.WriteLine($"Hand {i + 1}: signal={hand.SignalNumber}, state={state}, type={hand.HandType}");
                Assert.True(hand.SignalNumber >= -1, $"Hand {i + 1} has an invalid signal number.");
            }
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    [Trait("Category", "SimulatorReadOnly")]
    public void ReadOptionStatuses()
    {
        Integration.Run(false, (robot, _) =>
        {
            var count = robot.OptionSlotCount();
            var all = robot.OptionStatuses();
            output.WriteLine($"Option slots reported: {count}; OPSTSRD fields: {all.Length}");
            for (var i = 0; i < all.Length; i++)
                output.WriteLine($"All-slots field {i + 1}: {all[i]}");

            if (count == 0)
            {
                Assert.Equal(new[] { "" }, all);
                return;
            }

            var indexed = new string[count];
            for (var i = 0; i < count; i++)
            {
                indexed[i] = robot.OptionStatus(i + 1);
                output.WriteLine($"Option slot {i + 1}: {indexed[i].Replace("\v", "<VT>")}");
            }
            Assert.Equal(count, all.Length);
            Assert.Equal(indexed, all);
        });
    }
}
