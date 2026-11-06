using System.Text;
using System.Text.RegularExpressions;
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
            Assert.NotNull(robot.GetCurrentProgram(1));
            Assert.True(robot.TaskCount > 0);
        });
    }

    [ReadProgramFact]
    [Trait("Category", "RobotReadOnly")]
    [Trait("Category", "SimulatorReadOnly")]
    public void ReadPreparedProgramFile()
    {
        var name = SafetyConfig.ReadProgramName(Environment.GetEnvironmentVariable);
        Integration.Run(false, (robot, _) =>
        {
            if (!Regex.IsMatch(robot.ProgramExtension, @"\A[A-Za-z0-9]{1,8}\z", RegexOptions.CultureInvariant))
                throw new InvalidOperationException("Unrecognized program extension from OPEN.");
            var filename = name + "." + robot.ProgramExtension;
            using var contents = robot.FileRead(filename);
            output.WriteLine($"Read {contents.Length} bytes from {filename}.");
            Assert.True(contents.Length > 0, $"Program file {filename} is empty.");
            foreach (var line in ProgramFilePreview.ReadLines(contents))
                output.WriteLine(line);
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    public void ReadCurrentCartesianPosition()
    {
        Integration.Run(false, (robot, _) =>
        {
            var position = robot.CurrentPositionP;
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
            var position = robot.CurrentPositionJ;
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
            var hands = robot.HandStates;
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
            var count = robot.OptionSlotCount;
            output.WriteLine($"Option slots reported: {count}");
            for (var slot = 1; slot <= count; slot++)
            {
                var information = robot.GetOptionStatus(slot);
                output.WriteLine($"Option slot {slot}: {information.Replace("\v", "<VT>")}");
            }
        });
    }

    [ReadFact]
    [Trait("Category", "RobotReadOnly")]
    [Trait("Category", "SimulatorReadOnly")]
    public void ReadCurrentPosition()
    {
        Integration.Run(false, (robot, _) =>
        {
            output.WriteLine($"P: {robot.CurrentPositionP}");
            output.WriteLine($"J: {robot.CurrentPositionJ}");
        });
    }

    [ReadFact(Skip = "Crash the simulator, temporarily disabled")]
    [Trait("Category", "RobotReadOnly")]
    [Trait("Category", "SimulatorCrashOnly")]
    public void ReadMaintainaceForecast()
    {
        Integration.Run(false, (robot, _) =>
        {
            var battery = robot.MaintenanceForecastResetDate(MaintenanceType.Battery);
            output.WriteLine($"MFTIME=1: {battery}");
            var grease = robot.MaintenanceForecastResetDate(MaintenanceType.Grease);
            output.WriteLine($"MFTIME=2: {grease}");
            var belt = robot.MaintenanceForecastResetDate(MaintenanceType.Belt);
            output.WriteLine($"MFTIME=3: {belt}");
        });
    }
}

internal static class ProgramFilePreview
{
    internal static IReadOnlyList<string> ReadLines(Stream contents)
    {
        using var reader = new StreamReader(contents, Encoding.Latin1, false, 1024, leaveOpen: true);
        var lines = new List<string>();
        var remaining = 0;
        while (reader.ReadLine() is { } line)
        {
            if (lines.Count < 10)
            {
                var formatted = new StringBuilder(line.Length);
                foreach (var character in line)
                    if (character < 0x80)
                        formatted.Append(character);
                    else
                        formatted.Append($"\\x{(int)character:X2}");
                lines.Add(formatted.ToString());
            }
            else
                remaining++;
        }
        if (remaining > 0)
            lines.Add($"... {remaining} more lines");
        return lines;
    }
}
