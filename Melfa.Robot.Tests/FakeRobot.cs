using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Melfa.Robot.Tests;

/// <summary>
/// In-memory <see cref="IRobotCommands"/> used to verify scratch safety logic without a controller.
/// Every member records its name in <see cref="Calls"/>; members the scratch workflow does not use
/// throw <see cref="NotSupportedException"/> so unexpected commands fail loudly.
/// </summary>
internal sealed class FakeRobot : IRobotCommands
{
    public readonly List<string> Calls = new();
    public bool? Exists = false;
    public string CheckedFile;
    public string Selected = "";
    public Dictionary<int, string> SelectedBySlot;
    public string Fail;
    public bool FailFirstSet;
    public bool IgnoreFirstSet;
    public readonly List<string> VariableNames = new();
    public readonly List<int> VariableSlots = new();
    public PositionJ Position = new(1, 2, 3, 4, 5, 6, 7, 8);

    private void Call([CallerMemberName] string name = null)
    {
        Calls.Add(name);
        if (Fail == name) throw new InvalidOperationException(name);
    }

    private T Unsupported<T>([CallerMemberName] string name = null)
    {
        Call(name);
        throw new NotSupportedException($"{nameof(FakeRobot)} does not support {name}.");
    }

    private void Unsupported([CallerMemberName] string name = null) => Unsupported<bool>(name);

    #region Scratch workflow
    public bool? FileExists(string filename) { Call(); CheckedFile = filename; return Exists; }
    public string GetCurrentProgram(int slot = 1) { Call(); return SelectedBySlot?.GetValueOrDefault(slot) ?? Selected; }
    public void ProgramOpen(string filename) => Call();
    public void ProgramNew() => Call();
    public void ProgramSave() { Call(); Exists = true; }
    public void FileDelete(string filename) { Call(); Exists = false; }
    public string ProgramGetLine(int line) { Call(); return $"{line} End"; }
    public void ProgramEdit(int line, string content) => Call();
    public Type ProgramGetVariableType(string variableName) { VariableNames.Add(variableName); Call(); return typeof(PositionJ); }

    public string ReadVariable(string name, VariableClass variableClass, int slot = 1)
    {
        VariableNames.Add(name);
        VariableSlots.Add(slot);
        Call();
        if (variableClass != VariableClass.JointPosition)
            throw new NotSupportedException($"{nameof(FakeRobot)} only stores joint variables.");
        // Full precision, so values that PositionJ.ToString() would round are still visible to the caller.
        var p = Position;
        return "(" + string.Join(",", new[] { p.J1, p.J2, p.J3, p.J4, p.J5, p.J6, p.J7, p.J8 }
            .Select(v => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture))) + ")";
    }

    public void ProgramSetVariable(string name, string value)
    {
        VariableNames.Add(name);
        Call();
        if (IgnoreFirstSet) { IgnoreFirstSet = false; return; }
        Position = new PositionJ(value);
        if (FailFirstSet) { FailFirstSet = false; throw new InvalidOperationException("uncertain write"); }
    }
    #endregion

    #region Connection
    public string Host => "fake";
    public int Port => MelfaRobot.DEFAULT_PORT;
    public int RobotNumber => 1;
    public int CommandTimeout { get => Unsupported<int>(); set => Unsupported(); }
    public bool RequireResponseTerminator { get => Unsupported<bool>(); set => Unsupported(); }
    public bool IsConnected => Unsupported<bool>();
    public void Connect() => Unsupported();
    public void Disconnect() => Unsupported();
    public void Dispose() { }
    #endregion

    #region Unsupported
    public (int PowerOnHour, int RemainingHour) BatteryRemainingTime => Unsupported<(int, int)>();
    public MelfaRobot.CalibrationInfo Calibration => Unsupported<MelfaRobot.CalibrationInfo>();
    public bool CanResetPower => Unsupported<bool>();
    public (string Date, string Time) ControllerClock => Unsupported<(string, string)>();
    public double ControllerTemperature => Unsupported<double>();
    public PositionJ CurrentPositionJ => Unsupported<PositionJ>();
    public PositionP CurrentPositionP => Unsupported<PositionP>();
    public PositionJ DestinationPositionJ => Unsupported<PositionJ>();
    public PositionP DestinationPositionP => Unsupported<PositionP>();
    public bool DummyInput { set => Unsupported(); }
    public ImmutableArray<int> EncoderMissCounts => Unsupported<ImmutableArray<int>>();
    public ImmutableArray<double> EncoderTemperatures => Unsupported<ImmutableArray<double>>();
    public MelfaRobotException Error => Unsupported<MelfaRobotException>();
    public MelfaRobot.ErrorTimestamp ErrorLogStartDate => Unsupported<MelfaRobot.ErrorTimestamp>();
    public MelfaRobot.ErrorTimestamp ErrorSummaryStartDate => Unsupported<MelfaRobot.ErrorTimestamp>();
    public ImmutableArray<MelfaRobot.HandState> HandStates => Unsupported<ImmutableArray<MelfaRobot.HandState>>();
    public MelfaRobot.HourMeterData HourMeter => Unsupported<MelfaRobot.HourMeterData>();
    public bool LimitSwitch { set => Unsupported(); }
    public bool LowSpeedMode { set => Unsupported(); }
    public bool MachineLock { set => Unsupported(); }
    public ImmutableArray<double> MaximumEncoderTemperatures => Unsupported<ImmutableArray<double>>();
    public bool OperationEnabled { set => Unsupported(); }
    public int OptionSlotCount => Unsupported<int>();
    public byte Override { get => Unsupported<byte>(); set => Unsupported(); }
    public string ParameterKeyword { set => Unsupported(); }
    public bool ProgramStart { get => Unsupported<bool>(); set => Unsupported(); }
    public ImmutableArray<MelfaRobot.ServoCurrentCommandAxis> ServoCurrentCommands => Unsupported<ImmutableArray<MelfaRobot.ServoCurrentCommandAxis>>();
    public ImmutableArray<MelfaRobot.ServoCurrentFeedbackAxis> ServoCurrentFeedback => Unsupported<ImmutableArray<MelfaRobot.ServoCurrentFeedbackAxis>>();
    public ImmutableArray<MelfaRobot.ServoDroopAxis> ServoDroop => Unsupported<ImmutableArray<MelfaRobot.ServoDroopAxis>>();
    public ImmutableArray<MelfaRobot.ServoEncoderAxis> ServoEncoder => Unsupported<ImmutableArray<MelfaRobot.ServoEncoderAxis>>();
    public ImmutableArray<MelfaRobot.ServoLoadCurrentAxis> ServoLoadCurrent => Unsupported<ImmutableArray<MelfaRobot.ServoLoadCurrentAxis>>();
    public bool ServoOn { set => Unsupported(); }
    public ImmutableArray<MelfaRobot.ServoSpeedAxis> ServoSpeed => Unsupported<ImmutableArray<MelfaRobot.ServoSpeedAxis>>();
    public MelfaRobot.ServoVoltageInfo ServoVoltage => Unsupported<MelfaRobot.ServoVoltageInfo>();
    public MelfaRobot.StopSignalState StopSignals => Unsupported<MelfaRobot.StopSignalState>();
    public int Tool { get => Unsupported<int>(); set => Unsupported(); }

    public void BlockProgramStart(bool block, int slot = 1) => Unsupported();
    public void ClearBreakpoint(string program, int line = 0) => Unsupported();
    public void ClearErrorLog(ErrorSeverity severity) => Unsupported();
    public void ClearErrorSummary() => Unsupported();
    public void ClearHourMeter(HourMeterKind kind) => Unsupported();
    public void Close() => Unsupported();
    public void CycleStopProgram(int slot = 1) => Unsupported();
    public void CycleTimeClear(string program) => Unsupported();
    public void Execute(string instruction) => Unsupported();
    public void ExecuteBlocking(string instruction) => Unsupported();
    public void FactoryReset() => Unsupported();
    public void FactoryReset(string name, int mech = 0) => Unsupported();
    public void FileAttribute(string filename, bool? readOnlyLines, bool? readOnlyVariable) => Unsupported();
    public void FileCopy(string source, string destination) => Unsupported();
    public string FileFullPath(string filename) => Unsupported<string>();
    public Stream FileRead(string filename) => Unsupported<Stream>();
    public void FileRename(string source, string destination) => Unsupported();
    public (int Total, int Used) FilesystemFree() => Unsupported<(int, int)>();
    public void FileWrite(string filename, Stream stream) => Unsupported();
    public void ForceReleaseBrake(byte axis) => Unsupported();
    public MelfaRobot.Breakpoint GetBreakpoint(string program, int breakpointNumber, int startLine, int endLine) => Unsupported<MelfaRobot.Breakpoint>();
    public MelfaRobot.CycleTimeData GetCycleTime(string program) => Unsupported<MelfaRobot.CycleTimeData>();
    public MelfaRobot.DetailedState GetDetailedState(int slot = 1) => Unsupported<MelfaRobot.DetailedState>();
    public MelfaRobot.ErrorHistoryEntry GetErrorHistory(LocationType position) => Unsupported<MelfaRobot.ErrorHistoryEntry>();
    public string GetErrorMessage(int errorNumber) => Unsupported<string>();
    public MelfaRobot.ErrorSummary GetErrorSummary(int index) => Unsupported<MelfaRobot.ErrorSummary>();
    public MelfaRobot.ErrorSummary GetErrorSummaryWithLastOccurrence(int index) => Unsupported<MelfaRobot.ErrorSummary>();
    public MelfaRobot.ErrorHistoryEntry GetFilteredErrorHistory(LocationType position, string detailedErrorNumber, ErrorSeverity? severity = null) =>
        Unsupported<MelfaRobot.ErrorHistoryEntry>();
    public string GetOptionStatus(int optionSlot) => Unsupported<string>();
    public string GetParameter(string name) => Unsupported<string>();
    public string GetProgramLineContent(int slot = 1) => Unsupported<string>();
    public void GetProgramLineNumber(int slot = 1) => Unsupported();
    public MelfaRobot.RunState GetState(int slot = 1) => Unsupported<MelfaRobot.RunState>();
    public void HandAlign() => Unsupported();
    public void InstallAdditionalAxisData() => Unsupported();
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string checksum) => Unsupported();
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string j7, string j8, string checksum) => Unsupported();
    public void Jog(ElectricHandJogMode mode, byte speed, byte force, byte point) => Unsupported();
    public void Jog(JogMode mode, byte negative, byte positive, InchingMode inching = InchingMode.Off) => Unsupported();
    public void JumpToProgram(int line, int slot = 1) => Unsupported();
    public IEnumerable<string> ListChangedParameters() => Unsupported<IEnumerable<string>>();
    public void LoadProgram(string program, int slot = 1) => Unsupported();
    public ImmutableArray<int> MaintenanceForecastHour(MaintenanceType type) => Unsupported<ImmutableArray<int>>();
    public MelfaRobot.MaintenanceResetData MaintenanceForecastResetDate(MaintenanceType type) => Unsupported<MelfaRobot.MaintenanceResetData>();
    public void MaintenanceForecastReset(byte axis, MaintenanceResetTarget target) => Unsupported();
    public void MoveToSafePosition() => Unsupported();
    public MelfaRobot.RobotInformation Open(string name, string lang = null) => Unsupported<MelfaRobot.RobotInformation>();
    public void ProgramClear() => Unsupported();
    public int ProgramCountLines(int start = 0, int end = int.MaxValue) => Unsupported<int>();
    public void ProgramDelete(int start, int end) => Unsupported();
    public void ProgramDown() => Unsupported();
    public string ProgramGetLine(LocationType type) => Unsupported<string>();
    public IPosition ProgramGetPosition(LocationType type) => Unsupported<IPosition>();
    public IPosition ProgramGetPosition(string positionName) => Unsupported<IPosition>();
    public void ProgramInsert(int line, string content) => Call();
    public void ProgramRenumber(int newLine, int oldLineStart, int step, int oldLineEnd) => Unsupported();
    public void ProgramStep(StepMethod method) => Unsupported();
    public void ProgramUp() => Unsupported();
    public (IEnumerable<string> OriginData, string Checksum) ReadAxisOriginData() => Unsupported<(IEnumerable<string>, string)>();
    public IEnumerable<ushort> ReadCCLinkInput(int index) => Unsupported<IEnumerable<ushort>>();
    public IEnumerable<ushort> ReadCCLinkOutput(int index) => Unsupported<IEnumerable<ushort>>();
    public KeyValuePair<string, string> ReadGlobalVariable(LocationType position, VariableClass variableClass, int slot = 1) =>
        Unsupported<KeyValuePair<string, string>>();
    public string ReadGlobalVariable(string name, VariableClass variableClass, int slot = 1) => Unsupported<string>();
    public MelfaRobot.VariablePage ReadGlobalVariables(LocationType position, VariableClass variableClass, int slot = 1) => Unsupported<MelfaRobot.VariablePage>();
    public ushort ReadInput(int index) => Unsupported<ushort>();
    public (ushort Input, ushort Output) ReadInputOutput(int input, int output) => Unsupported<(ushort, ushort)>();
    public ushort ReadOutput(int index) => Unsupported<ushort>();
    public string ReadParameter(string name) => Unsupported<string>();
    public MelfaRobot.VariablePage ReadVariables(LocationType position, VariableClass variableClass, int slot = 1) => Unsupported<MelfaRobot.VariablePage>();
    public void ReleaseBrake(byte axis) => Unsupported();
    public void ResetAlarm() => Unsupported();
    public void ResetAllPrograms() => Unsupported();
    public void ResetIO() => Unsupported();
    public void ResetPower() => Unsupported();
    public void ResetProgram(int slot = 1) => Unsupported();
    public void RunProgram(string program, TaskMode mode = TaskMode.Repeat, int slot = 1) => Unsupported();
    public void ServoMonitorReset() => Unsupported();
    public void SetBreakpoint(string program, int line, BreakpointType type) => Unsupported();
    public void SetControllerClock(string date, string time) => Unsupported();
    public void SetHand(int index, bool state) => Unsupported();
    public void SetOrigin(OriginType type, byte axis) => Unsupported();
    public void SetParameter(string name, string value) => Unsupported();
    public MelfaRobot.TaskSlotInfo SlotGet(int slot) => Unsupported<MelfaRobot.TaskSlotInfo>();
    public void SlotSet(int slot, string program, TaskMode mode, TaskCondition condition, int priority) => Unsupported();
    public void StopProgram(int slot = 1) => Unsupported();
    public void ThrowIfError() => Unsupported();
    public void WriteCCLinkInput(int index, ushort value) => Unsupported();
    public void WriteCCLinkOutput(int index, ushort value) => Unsupported();
    public void WriteInput(int index, ushort value) => Unsupported();
    public void WriteOutput(int index, ushort value) => Unsupported();
    public void WriteParameter(string name, string value) => Unsupported();
    public void WriteParameterAndReboot(string name, string value) => Unsupported();
    public void WriteVariable(string program, string variable, string value) => Unsupported();
    #endregion
}
