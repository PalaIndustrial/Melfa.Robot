using System.Collections.Immutable;
using System.Diagnostics;

namespace Melfa.Robot.Tests;

internal interface IRobotCommands
{
    bool? FileExists(string name);
    string CurrentProgram(int slot);
    void OpenProgram(string name);
    void DiscardProgram();
    void SaveProgram();
    void DeleteFile(string name);
    string GetLine(int line);
    void EditLine(int line, string content);
    Type VariableType(string name);
    PositionJ GetJointVariable(string name);
    void SetVariable(string name, string value);
}

internal sealed class RobotSession : IRobotCommands, IDisposable
{
    private readonly MelfaRobot robot;
    private readonly int delay;
    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static readonly object CommandLock = new();
    private static long? lastCommandEnd;
    private bool openAttempted;
    internal int TaskCount { get; private set; }
    internal string ProgramExtension { get; private set; } = "";
    internal byte JointAxisPattern { get; private set; }

    internal RobotSession(SafetyConfig config)
    {
        robot = new MelfaRobot(config.Host, config.Port, config.Robot);
        delay = config.Delay;
    }

    internal void ConnectAndOpen()
    {
        robot.Connect();
        openAttempted = true;
        var info = Command(() => robot.Open("MELFATEST"));
        TaskCount = info.MaximumTaskCount;
        ProgramExtension = info.ProgramExtension;
        JointAxisPattern = info.AxisPatternJoint;
        if (TaskCount is < 1 or > 32)
            throw new InvalidOperationException("Unexpected task count; cannot establish scratch isolation.");
    }

    private T Command<T>(Func<T> action)
    {
        lock (CommandLock)
        {
            if (lastCommandEnd is long last)
            {
                var remaining = delay - (Clock.ElapsedMilliseconds - last);
                if (remaining > 0)
                    Thread.Sleep((int)remaining);
            }
            try { return action(); }
            finally { lastCommandEnd = Clock.ElapsedMilliseconds; }
        }
    }

    private void Command(Action action) => Command(() => { action(); return true; });
    public bool? FileExists(string name) => Command(() => robot.FileExists(name));
    public string CurrentProgram(int slot) => Command(() => robot.GetCurrentProgram(slot));
    internal PositionP CurrentPosition() => Command(() => robot.CurrentPositionP);
    internal PositionJ CurrentJointPosition() => Command(() => robot.CurrentPositionJ);
    internal ImmutableArray<MelfaRobot.HandState> HandStates() => Command(() => robot.HandStates);
    internal int OptionSlotCount() => Command(() => robot.OptionSlotCount);
    internal string OptionStatus(int slot) => Command(() => robot.GetOptionStatus(slot));
    internal Stream ReadFile(string filename) => Command(() => robot.FileRead(filename));
    public void OpenProgram(string name) => Command(() => robot.ProgramOpen(name));
    public void DiscardProgram() => Command(robot.ProgramNew);
    public void SaveProgram() => Command(robot.ProgramSave);
    public void DeleteFile(string name) => Command(() => robot.FileDelete(name));
    public string GetLine(int line) => Command(() => robot.ProgramGetLine(line));
    public void EditLine(int line, string content) => Command(() => robot.ProgramEdit(line, content));
    public Type VariableType(string name) => Command(() => robot.ProgramGetVariableType(name));
    public PositionJ GetJointVariable(string name) => Command(() => new PositionJ(robot.ReadVariable(name, VariableClass.JointPosition, TaskCount + 1)));
    public void SetVariable(string name, string value) => Command(() => robot.ProgramSetVariable(name, value));

    public void Dispose()
    {
        try
        {
            if (openAttempted)
                Command(robot.Close);
        }
        finally { robot.Dispose(); }
    }
}
