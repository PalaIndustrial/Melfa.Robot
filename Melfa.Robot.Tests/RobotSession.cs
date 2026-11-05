using System.Collections.Immutable;
using System.Diagnostics;

namespace Melfa.Robot.Tests;

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
        Connect();
        openAttempted = true;
        var info = Open("MELFATEST");
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

    #region Connection
    public string Host => robot.Host;
    public int Port => robot.Port;
    public int RobotNumber => robot.RobotNumber;
    public int CommandTimeout
    {
        get => robot.CommandTimeout;
        set => robot.CommandTimeout = value;
    }
    public bool RequireResponseTerminator
    {
        get => robot.RequireResponseTerminator;
        set => robot.RequireResponseTerminator = value;
    }
    public bool IsConnected => robot.IsConnected;
    public void Connect() => robot.Connect();
    public void Disconnect() => robot.Disconnect();
    #endregion

    #region Communication
    public MelfaRobot.RobotInformation Open(string name, string lang = null) => Command(() => robot.Open(name, lang));
    public void Close() => Command(robot.Close);
    public bool OperationEnabled { set => Command(() => robot.OperationEnabled = value); }
    #endregion

    #region Program
    public void ProgramOpen(string filename) => Command(() => robot.ProgramOpen(filename));
    public void ProgramSave() => Command(robot.ProgramSave);
    public void ProgramNew() => Command(robot.ProgramNew);
    public void ProgramEdit(int line, string content) => Command(() => robot.ProgramEdit(line, content));
    public void ProgramInsert(int line, string content) => Command(() => robot.ProgramInsert(line, content));
    public void ProgramSetVariable(string name, string value) => Command(() => robot.ProgramSetVariable(name, value));
    public string ProgramGetLine(LocationType type) => Command(() => robot.ProgramGetLine(type));
    public string ProgramGetLine(int line) => Command(() => robot.ProgramGetLine(line));
    public IPosition ProgramGetPosition(LocationType type) => Command(() => robot.ProgramGetPosition(type));
    public IPosition ProgramGetPosition(string name) => Command(() => robot.ProgramGetPosition(name));
    public Type ProgramGetVariableType(string name) => Command(() => robot.ProgramGetVariableType(name));
    public int ProgramCountLines(int start = 0, int end = int.MaxValue) => Command(() => robot.ProgramCountLines(start, end));
    public void Execute(string command) => Command(() => robot.Execute(command));
    public void ExecuteBlocking(string command) => Command(() => robot.ExecuteBlocking(command));
    public void ProgramStep(StepMethod method) => Command(() => robot.ProgramStep(method));
    public void ProgramClear() => Command(() => robot.ProgramClear());
    public void ProgramDelete(int start, int end) => Command(() => robot.ProgramDelete(start, end));
    public void ProgramRenumber(int newLine, int oldLineStart, int step, int oldLineEnd) =>
        Command(() => robot.ProgramRenumber(newLine, oldLineStart, step, oldLineEnd));
    #endregion

    #region File
    public bool? FileExists(string filename) => Command(() => robot.FileExists(filename));
    public string FileFullPath(string filename) => Command(() => robot.FileFullPath(filename));
    public void FileCopy(string source, string destination) => Command(() => robot.FileCopy(source, destination));
    public void FileDelete(string filename) => Command(() => robot.FileDelete(filename));
    public void FileRename(string source, string destination) => Command(() => robot.FileRename(source, destination));
    public void FileAttribute(string filename, bool? readOnlyLines, bool? readOnlyVariable) =>
        Command(() => robot.FileAttribute(filename, readOnlyLines, readOnlyVariable));
    public Stream FileRead(string filename) => Command(() => robot.FileRead(filename));
    public void FileWrite(string filename, Stream stream) => Command(() => robot.FileWrite(filename, stream));
    public (int Total, int Used) FilesystemFree() => Command(robot.FilesystemFree);
    #endregion

    #region Run control
    public void LoadProgram(string program, int slot = 1) => Command(() => robot.LoadProgram(program, slot));
    public void ProgramUp() => Command(robot.ProgramUp);
    public void ProgramDown() => Command(robot.ProgramDown);
    public string GetCurrentProgram(int slot = 1) => Command(() => robot.GetCurrentProgram(slot));
    public void JumpToProgram(int line, int slot = 1) => Command(() => robot.JumpToProgram(line, slot));
    public void GetProgramLineNumber(int slot = 1) => Command(() => robot.GetProgramLineNumber(slot));
    public string GetProgramLineContent(int slot = 1) => Command(() => robot.GetProgramLineContent(slot));
    public bool ServoOn { set => Command(() => robot.ServoOn = value); }
    public byte Override
    {
        get => Command(() => robot.Override);
        set => Command(() => robot.Override = value);
    }
    public void RunProgram(string program, TaskMode mode = TaskMode.Repeat, int slot = 1) => Command(() => robot.RunProgram(program, mode, slot));
    public void StopProgram(int slot = 1) => Command(() => robot.StopProgram(slot));
    public void BlockProgramStart(bool block, int slot = 1) => Command(() => robot.BlockProgramStart(block, slot));
    public void CycleStopProgram(int slot = 1) => Command(() => robot.CycleStopProgram(slot));
    public void ResetAlarm() => Command(robot.ResetAlarm);
    public void ResetAllPrograms() => Command(robot.ResetAllPrograms);
    public void ResetProgram(int slot = 1) => Command(() => robot.ResetProgram(slot));
    public void ResetIO() => Command(robot.ResetIO);
    public bool MachineLock { set => Command(() => robot.MachineLock = value); }
    #endregion

    #region Hand and motion
    public void SetHand(int index, bool state) => Command(() => robot.SetHand(index, state));
    public void HandAlign() => Command(robot.HandAlign);
    public void MoveToSafePosition() => Command(robot.MoveToSafePosition);
    public void Jog(JogMode mode, byte negative, byte positive, InchingMode inching = InchingMode.Off) =>
        Command(() => robot.Jog(mode, negative, positive, inching));
    public void Jog(ElectricHandJogMode mode, byte speed, byte force, byte point) => Command(() => robot.Jog(mode, speed, force, point));
    public bool LimitSwitch { set => Command(() => robot.LimitSwitch = value); }
    public bool ProgramStart
    {
        get => Command(() => robot.ProgramStart);
        set => Command(() => robot.ProgramStart = value);
    }
    #endregion

    #region Breakpoints, tool and safety
    public void SetBreakpoint(string program, int line, BreakpointType type) => Command(() => robot.SetBreakpoint(program, line, type));
    public void ClearBreakpoint(string program, int line = 0) => Command(() => robot.ClearBreakpoint(program, line));
    public MelfaRobot.Breakpoint GetBreakpoint(string program, int breakpointNumber, int startLine, int endLine) =>
        Command(() => robot.GetBreakpoint(program, breakpointNumber, startLine, endLine));
    public int Tool
    {
        get => Command(() => robot.Tool);
        set => Command(() => robot.Tool = value);
    }
    public bool LowSpeedMode { set => Command(() => robot.LowSpeedMode = value); }
    #endregion

    #region State
    public MelfaRobot.RunState GetState(int slot = 1) => Command(() => robot.GetState(slot));
    public MelfaRobot.DetailedState GetDetailedState(int slot = 1) => Command(() => robot.GetDetailedState(slot));
    public MelfaRobot.CalibrationInfo Calibration => Command(() => robot.Calibration);
    #endregion

    #region I/O
    public (ushort Input, ushort Output) ReadInputOutput(int input, int output) => Command(() => robot.ReadInputOutput(input, output));
    public ushort ReadInput(int index) => Command(() => robot.ReadInput(index));
    public ushort ReadOutput(int index) => Command(() => robot.ReadOutput(index));
    public void WriteOutput(int index, ushort value) => Command(() => robot.WriteOutput(index, value));
    public IEnumerable<ushort> ReadCCLinkInput(int index) => Command(() => robot.ReadCCLinkInput(index).ToList());
    public IEnumerable<ushort> ReadCCLinkOutput(int index) => Command(() => robot.ReadCCLinkOutput(index).ToList());
    public void WriteCCLinkOutput(int index, ushort value) => Command(() => robot.WriteCCLinkOutput(index, value));
    public bool DummyInput { set => Command(() => robot.DummyInput = value); }
    public void WriteInput(int index, ushort value) => Command(() => robot.WriteInput(index, value));
    public void WriteCCLinkInput(int index, ushort value) => Command(() => robot.WriteCCLinkInput(index, value));
    public MelfaRobot.StopSignalState StopSignals => Command(() => robot.StopSignals);
    public ImmutableArray<MelfaRobot.HandState> HandStates => Command(() => robot.HandStates);
    #endregion

    #region Positions and clock
    public PositionJ CurrentPositionJ => Command(() => robot.CurrentPositionJ);
    public PositionP CurrentPositionP => Command(() => robot.CurrentPositionP);
    public PositionJ DestinationPositionJ => Command(() => robot.DestinationPositionJ);
    public PositionP DestinationPositionP => Command(() => robot.DestinationPositionP);
    public (string Date, string Time) ControllerClock => Command(() => robot.ControllerClock);
    public void SetControllerClock(string date, string time) => Command(() => robot.SetControllerClock(date, time));
    #endregion

    #region Hour meter and cycle time
    public MelfaRobot.HourMeterData HourMeter => Command(() => robot.HourMeter);
    public void ClearHourMeter(HourMeterKind kind) => Command(() => robot.ClearHourMeter(kind));
    public MelfaRobot.CycleTimeData GetCycleTime(string program) => Command(() => robot.GetCycleTime(program));
    public void CycleTimeClear(string program) => Command(() => robot.CycleTimeClear(program));
    #endregion

    #region Errors
    public MelfaRobotException Error => Command(() => robot.Error);
    public void ThrowIfError() => Command(robot.ThrowIfError);
    public string GetErrorMessage(int errorNumber) => Command(() => robot.GetErrorMessage(errorNumber));
    public MelfaRobot.ErrorHistoryEntry GetErrorHistory(LocationType position) => Command(() => robot.GetErrorHistory(position));
    public MelfaRobot.ErrorHistoryEntry GetFilteredErrorHistory(LocationType position, string detailedErrorNumber, ErrorSeverity? severity = null) =>
        Command(() => robot.GetFilteredErrorHistory(position, detailedErrorNumber, severity));
    public void ClearErrorLog(ErrorSeverity severity) => Command(() => robot.ClearErrorLog(severity));
    public MelfaRobot.ErrorSummary GetErrorSummary(int index) => Command(() => robot.GetErrorSummary(index));
    public MelfaRobot.ErrorSummary GetErrorSummaryWithLastOccurrence(int index) => Command(() => robot.GetErrorSummaryWithLastOccurrence(index));
    public void ClearErrorSummary() => Command(robot.ClearErrorSummary);
    public MelfaRobot.ErrorTimestamp ErrorLogStartDate => Command(() => robot.ErrorLogStartDate);
    public MelfaRobot.ErrorTimestamp ErrorSummaryStartDate => Command(() => robot.ErrorSummaryStartDate);
    #endregion

    #region Variables
    public string ReadVariable(string name, VariableClass variableClass, int slot = 1) => Command(() => robot.ReadVariable(name, variableClass, slot));
    public MelfaRobot.VariablePage ReadVariables(LocationType position, VariableClass variableClass, int slot = 1) =>
        Command(() => robot.ReadVariables(position, variableClass, slot));
    public string ReadGlobalVariable(string name, VariableClass variableClass, int slot = 1) =>
        Command(() => robot.ReadGlobalVariable(name, variableClass, slot));
    public KeyValuePair<string, string> ReadGlobalVariable(LocationType position, VariableClass variableClass, int slot = 1) =>
        Command(() => robot.ReadGlobalVariable(position, variableClass, slot));
    public MelfaRobot.VariablePage ReadGlobalVariables(LocationType position, VariableClass variableClass, int slot = 1) =>
        Command(() => robot.ReadGlobalVariables(position, variableClass, slot));
    public void WriteVariable(string program, string variable, string value) => Command(() => robot.WriteVariable(program, variable, value));
    #endregion

    #region Controller monitoring
    public int OptionSlotCount => Command(() => robot.OptionSlotCount);
    public string GetOptionStatus(int optionSlot) => Command(() => robot.GetOptionStatus(optionSlot));
    public double ControllerTemperature => Command(() => robot.ControllerTemperature);
    public ImmutableArray<double> EncoderTemperatures => Command(() => robot.EncoderTemperatures);
    public ImmutableArray<double> MaximumEncoderTemperatures => Command(() => robot.MaximumEncoderTemperatures);
    public ImmutableArray<int> EncoderMissCounts => Command(() => robot.EncoderMissCounts);
    public ImmutableArray<MelfaRobot.ServoEncoderAxis> ServoEncoder => Command(() => robot.ServoEncoder);
    public ImmutableArray<MelfaRobot.ServoDroopAxis> ServoDroop => Command(() => robot.ServoDroop);
    public ImmutableArray<MelfaRobot.ServoSpeedAxis> ServoSpeed => Command(() => robot.ServoSpeed);
    public ImmutableArray<MelfaRobot.ServoCurrentCommandAxis> ServoCurrentCommands => Command(() => robot.ServoCurrentCommands);
    public ImmutableArray<MelfaRobot.ServoCurrentFeedbackAxis> ServoCurrentFeedback => Command(() => robot.ServoCurrentFeedback);
    public ImmutableArray<MelfaRobot.ServoLoadCurrentAxis> ServoLoadCurrent => Command(() => robot.ServoLoadCurrent);
    public MelfaRobot.ServoVoltageInfo ServoVoltage => Command(() => robot.ServoVoltage);
    public void ServoMonitorReset() => Command(robot.ServoMonitorReset);
    #endregion

    #region Parameters
    public void FactoryReset() => Command(() => robot.FactoryReset());
    public void FactoryReset(string name, int mech = 0) => Command(() => robot.FactoryReset(name, mech));
    public string GetParameter(string name) => Command(() => robot.GetParameter(name));
    public void SetParameter(string name, string value) => Command(() => robot.SetParameter(name, value));
    public string ReadParameter(string name) => Command(() => robot.ReadParameter(name));
    public void WriteParameter(string name, string value) => Command(() => robot.WriteParameter(name, value));
    public void WriteParameterAndReboot(string name, string value) => Command(() => robot.WriteParameterAndReboot(name, value));
    public IEnumerable<string> ListChangedParameters() => Command(() => robot.ListChangedParameters().ToList());
    public string ParameterKeyword { set => Command(() => robot.ParameterKeyword = value); }
    #endregion

    #region Slots and maintenance
    public MelfaRobot.TaskSlotInfo SlotGet(int slot) => Command(() => robot.SlotGet(slot));
    public void SlotSet(int slot, string program, TaskMode mode, TaskCondition condition, int priority) =>
        Command(() => robot.SlotSet(slot, program, mode, condition, priority));
    public (int PowerOnHour, int RemainingHour) BatteryRemainingTime => Command(() => robot.BatteryRemainingTime);
    public void ReleaseBrake(byte axis) => Command(() => robot.ReleaseBrake(axis));
    public void ForceReleaseBrake(byte axis) => Command(() => robot.ForceReleaseBrake(axis));
    public void SetOrigin(OriginType type, byte axis) => Command(() => robot.SetOrigin(type, axis));
    public void InstallAdditionalAxisData() => Command(robot.InstallAdditionalAxisData);
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string checksum) =>
        Command(() => robot.InstallAxisOriginData(j1, j2, j3, j4, j5, j6, checksum));
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string j7, string j8, string checksum) =>
        Command(() => robot.InstallAxisOriginData(j1, j2, j3, j4, j5, j6, j7, j8, checksum));
    public (IEnumerable<string> OriginData, string Checksum) ReadAxisOriginData() => Command(() =>
    {
        var (originData, checksum) = robot.ReadAxisOriginData();
        return ((IEnumerable<string>)originData.ToList(), checksum);
    });
    public void ResetPower() => Command(robot.ResetPower);
    public bool CanResetPower => Command(() => robot.CanResetPower);
    public void MaintenanceForecastReset(byte axis, MaintenanceResetTarget target) => Command(() => robot.MaintenanceForecastReset(axis, target));
    public ImmutableArray<int> MaintenanceForecastHour(MaintenanceType type) => Command(() => robot.MaintenanceForecastHour(type));
    public MelfaRobot.MaintenanceResetData MaintenanceForecastResetDate(MaintenanceType type) => Command(() => robot.MaintenanceForecastResetDate(type));
    #endregion

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
