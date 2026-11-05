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

    internal static async Task Run(bool write, Func<RobotSession, SafetyConfig, Task> func)
    {
        var config = SafetyConfig.Load(Environment.GetEnvironmentVariable, write)
            ?? throw new InvalidOperationException("Integration test is not enabled.");
        var session = new RobotSession(config);
        await ScratchOperations.WithCleanup(async () =>
        {
            session.ConnectAndOpen();
            await func(session, config);
        }, session.Dispose);
    }
}
