using System.Collections.Immutable;

namespace Melfa.Robot;

public interface IRobotCommands
{
    (int PowerOnHour, int RemainingHour) BatteryRemainingTime { get; }
    MelfaRobot.CalibrationInfo Calibration { get; }
    bool CanResetPower { get; }
    int CommandTimeout { get; set; }
    (string Date, string Time) ControllerClock { get; }
    double ControllerTemperature { get; }
    PositionJ CurrentPositionJ { get; }
    PositionP CurrentPositionP { get; }
    PositionJ DestinationPositionJ { get; }
    PositionP DestinationPositionP { get; }
    bool DummyInput { set; }
    ImmutableArray<int> EncoderMissCounts { get; }
    ImmutableArray<double> EncoderTemperatures { get; }
    MelfaRobotException Error { get; }
    MelfaRobot.ErrorTimestamp ErrorLogStartDate { get; }
    MelfaRobot.ErrorTimestamp ErrorSummaryStartDate { get; }
    ImmutableArray<MelfaRobot.HandState> HandStates { get; }
    string Host { get; }
    MelfaRobot.HourMeterData HourMeter { get; }
    bool IsConnected { get; }
    bool LimitSwitch { set; }
    bool LowSpeedMode { set; }
    bool MachineLock { set; }
    ImmutableArray<double> MaximumEncoderTemperatures { get; }
    bool OperationEnabled { set; }
    int OptionSlotCount { get; }
    byte Override { get; set; }
    string ParameterKeyword { set; }
    int Port { get; }
    bool ProgramStart { get; set; }
    bool RequireResponseTerminator { get; set; }
    int RobotNumber { get; }
    ImmutableArray<MelfaRobot.ServoCurrentCommandAxis> ServoCurrentCommands { get; }
    ImmutableArray<MelfaRobot.ServoCurrentFeedbackAxis> ServoCurrentFeedback { get; }
    ImmutableArray<MelfaRobot.ServoDroopAxis> ServoDroop { get; }
    ImmutableArray<MelfaRobot.ServoEncoderAxis> ServoEncoder { get; }
    ImmutableArray<MelfaRobot.ServoLoadCurrentAxis> ServoLoadCurrent { get; }
    bool ServoOn { set; }
    ImmutableArray<MelfaRobot.ServoSpeedAxis> ServoSpeed { get; }
    MelfaRobot.ServoVoltageInfo ServoVoltage { get; }
    MelfaRobot.StopSignalState StopSignals { get; }
    int Tool { get; set; }

    void BlockProgramStart(bool block, int slot = 1);
    void ClearBreakpoint(string program, int line = 0);
    void ClearErrorLog(ErrorSeverity severity);
    void ClearErrorSummary();
    void ClearHourMeter(HourMeterKind kind);
    void Close();
    void Connect();
    void CycleStopProgram(int slot = 1);
    void CycleTimeClear(string program);
    void Disconnect();
    void Dispose();
    void Execute(string instruction);
    void ExecuteBlocking(string instruction);
    void FactoryReset();
    void FactoryReset(string name, int mech = 0);
    void FileAttribute(string filename, bool? readOnlyLines, bool? readOnlyVariable);
    void FileCopy(string source, string destination);
    void FileDelete(string filename);
    bool? FileExists(string filename);
    string FileFullPath(string filename);
    Stream FileRead(string filename);
    void FileRename(string source, string destination);
    (int Total, int Used) FilesystemFree();
    void FileWrite(string filename, Stream stream);
    void ForceReleaseBrake(byte axis);
    MelfaRobot.Breakpoint GetBreakpoint(string program, int breakpointNumber, int startLine, int endLine);
    string GetCurrentProgram(int slot = 1);
    MelfaRobot.CycleTimeData GetCycleTime(string program);
    MelfaRobot.DetailedState GetDetailedState(int slot = 1);
    MelfaRobot.ErrorHistoryEntry GetErrorHistory(LocationType position);
    string GetErrorMessage(int errorNumber);
    MelfaRobot.ErrorSummary GetErrorSummary(int index);
    MelfaRobot.ErrorSummary GetErrorSummaryWithLastOccurrence(int index);
    MelfaRobot.ErrorHistoryEntry GetFilteredErrorHistory(LocationType position, string detailedErrorNumber, ErrorSeverity? severity = null);
    string GetOptionStatus(int optionSlot);
    string GetParameter(string name);
    string GetProgramLineContent(int slot = 1);
    void GetProgramLineNumber(int slot = 1);
    MelfaRobot.RunState GetState(int slot = 1);
    void HandAlign();
    void InstallAdditionalAxisData();
    void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string checksum);
    void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string j7, string j8, string checksum);
    void Jog(ElectricHandJogMode mode, byte speed, byte force, byte point);
    void Jog(JogMode mode, byte negative, byte positive, InchingMode inching = InchingMode.Off);
    void JumpToProgram(int line, int slot = 1);
    IEnumerable<string> ListChangedParameters();
    void LoadProgram(string program, int slot = 1);
    ImmutableArray<int> MaintenanceForecastHour(MaintenanceType type);
    MelfaRobot.MaintenanceResetData MaintenanceForecastResetDate(MaintenanceType type);
    void MaintenanceForecastReset(byte axis, MaintenanceResetTarget target);
    void MoveToSafePosition();
    MelfaRobot.RobotInformation Open(string name, string lang = null);
    void ProgramClear();
    int ProgramCountLines(int start = 0, int end = int.MaxValue);
    void ProgramDelete(int start, int end);
    void ProgramDown();
    void ProgramEdit(int line, string content);
    string ProgramGetLine(int line);
    string ProgramGetLine(LocationType type);
    IPosition ProgramGetPosition(LocationType type);
    IPosition ProgramGetPosition(string positionName);
    Type ProgramGetVariableType(string variableName);
    void ProgramInsert(int line, string content);
    void ProgramNew();
    void ProgramOpen(string filename);
    void ProgramRenumber(int newLine, int oldLineStart, int step, int oldLineEnd);
    void ProgramSave();
    void ProgramSetVariable(string name, string value);
    void ProgramStep(StepMethod method);
    void ProgramUp();
    (IEnumerable<string> OriginData, string Checksum) ReadAxisOriginData();
    IEnumerable<ushort> ReadCCLinkInput(int index);
    IEnumerable<ushort> ReadCCLinkOutput(int index);
    KeyValuePair<string, string> ReadGlobalVariable(LocationType position, VariableClass variableClass, int slot = 1);
    string ReadGlobalVariable(string name, VariableClass variableClass, int slot = 1);
    MelfaRobot.VariablePage ReadGlobalVariables(LocationType position, VariableClass variableClass, int slot = 1);
    ushort ReadInput(int index);
    (ushort Input, ushort Output) ReadInputOutput(int input, int output);
    ushort ReadOutput(int index);
    string ReadParameter(string name);
    string ReadVariable(string name, VariableClass variableClass, int slot = 1);
    MelfaRobot.VariablePage ReadVariables(LocationType position, VariableClass variableClass, int slot = 1);
    void ReleaseBrake(byte axis);
    void ResetAlarm();
    void ResetAllPrograms();
    void ResetIO();
    void ResetPower();
    void ResetProgram(int slot = 1);
    void RunProgram(string program, TaskMode mode = TaskMode.Repeat, int slot = 1);
    void ServoMonitorReset();
    void SetBreakpoint(string program, int line, BreakpointType type);
    void SetControllerClock(string date, string time);
    void SetHand(int index, bool state);
    void SetOrigin(OriginType type, byte axis);
    void SetParameter(string name, string value);
    MelfaRobot.TaskSlotInfo SlotGet(int slot);
    void SlotSet(int slot, string program, TaskMode mode, TaskCondition condition, int priority);
    void StopProgram(int slot = 1);
    void ThrowIfError();
    void WriteCCLinkInput(int index, ushort value);
    void WriteCCLinkOutput(int index, ushort value);
    void WriteInput(int index, ushort value);
    void WriteOutput(int index, ushort value);
    void WriteParameter(string name, string value);
    void WriteParameterAndReboot(string name, string value);
    void WriteVariable(string program, string variable, string value);
}