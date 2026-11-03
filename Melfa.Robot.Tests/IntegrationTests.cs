using Xunit;

namespace Melfa.Robot.Tests;

[CollectionDefinition("Robot integration", DisableParallelization = true)]
public sealed class RobotIntegrationCollection;

internal static class Integration
{
    internal static void Run(bool write, Action<RobotSession, SafetyConfig> action)
    {
        var config = SafetyConfig.Load(Environment.GetEnvironmentVariable, write)
            ?? throw new InvalidOperationException("Integration test is not enabled.");
        var session = new RobotSession(config);
        ScratchOperations.WithCleanup(() =>
        {
            session.ConnectAndOpen();
            action(session, config);
        }, session.Dispose);
    }
}

[Collection("Robot integration")]
public class SimulatorWriteTests
{
    [Fact(Skip = "Scratch program creation is not yet verified.")]
    [Trait("Category", "SimulatorWrite")]
    public void EditScratchLineAndDiscard()
    {
        Integration.Run(true, (robot, config) =>
            ScratchOperations.WithScratch(robot, config, robot.TaskCount, robot.ProgramExtension,
                () => ScratchOperations.EditLine(robot, config.Line)));
    }

    [Fact(Skip = "Scratch program creation is not yet verified.")]
    [Trait("Category", "SimulatorWrite")]
    public void EditAndRestoreScratchJointVariable()
    {
        Integration.Run(true, (robot, config) =>
            ScratchOperations.WithScratch(robot, config, robot.TaskCount, robot.ProgramExtension,
                () =>
                {
                    robot.SetVariable(config.Joint!, new PositionJ(0, 0, 0, 0, 0, 0, 0, 0).ToString());
                    ScratchOperations.EditJoint(robot, config.Joint!);
                }));
    }
}
