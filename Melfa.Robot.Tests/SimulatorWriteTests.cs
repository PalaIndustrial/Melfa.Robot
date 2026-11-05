using Xunit;

namespace Melfa.Robot.Tests;

[Collection("Robot integration")]
public class SimulatorWriteTests
{
    [Fact]
    [Trait("Category", "SimulatorWrite")]
    public void EditScratchLineAndDiscard()
    {
        Integration.Run(true, (robot, config) =>
            ScratchOperations.WithScratch(robot, config, robot.TaskCount, robot.ProgramExtension,
                () => ScratchOperations.EditLine(robot, config.Line)));
    }

    [Fact]
    [Trait("Category", "SimulatorWrite")]
    public void EditAndRestoreScratchJointVariable()
    {
        Integration.Run(true, (robot, config) =>
            ScratchOperations.WithScratch(robot, config, robot.TaskCount, robot.ProgramExtension,
                () =>
                {
                    robot.ProgramSetVariable(config.Joint, new PositionJ(0, 0, 0, 0, 0, 0, 0, 0).ToString());
                    ScratchOperations.EditJoint(robot, config.Joint, robot.TaskCount + 1);
                }));
    }
}
