using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Melfa.Robot.Tests;

public partial class ProtocolTests
{
    private const string JointPositionResponse = "J1;+1.25;J2;-2.50;J3;3;J4;.5;J5;5.0;J6;6.0;;****,****;100;0.00;00000000";
    private const string CartesianPositionResponse = "X;+1.25;Y;-2.50;Z;3;A;90;B;-90;C;45;L1;7.0;;6,4294967295;100;0.00;00000000";

    private static string Ok(string payload = "") => "QoK" + payload + "\r";

    private static Controller Script(params (string Command, string Response)[] exchanges) =>
        new(async stream =>
        {
            foreach (var (command, response) in exchanges)
            {
                Assert.Equal(command, await ReadCommand(stream));
                await Send(stream, response);
            }
        });

    [Fact]
    public async Task FormatsCommandWrappers()
    {
        (Action<MelfaRobot> Act, string[] Commands)[] cases =
        {
            (r => r.OperationEnabled = true, new[] { "1;1;CNTLON" }),
            (r => r.OperationEnabled = false, new[] { "1;1;CNTLOFF" }),
            (r => r.ProgramOpen("TEST"), new[] { "1;1;LOAD=TEST" }),
            (r => r.ProgramSave(), new[] { "1;1;SAVE" }),
            (r => r.ProgramNew(), new[] { "1;1;NEW" }),
            (r => r.ProgramInsert(10, "MOV P1"), new[] { "1;1;EDINS=10;MOV P1" }),
            (r => r.ProgramSetVariable("M1", "5"), new[] { "1;1;VAL=M1=5" }),
            (r => r.Execute("MOV P1"), new[] { "1;1;EXECMOV P1" }),
            (r => r.ProgramStep(StepMethod.Forward), new[] { "1;1;STEP1" }),
            (r => r.ProgramStep(StepMethod.Continuous), new[] { "1;1;STEPR" }),
            (r => r.ProgramStep(StepMethod.Backward), new[] { "1;1;STEPB" }),
            (r => r.ProgramStep(StepMethod.Stop), new[] { "1;1;STEPS" }),
            (r => r.ProgramClear(), new[] { "1;1;ECLR" }),
            (r => r.ProgramDelete(10, 20), new[] { "1;1;DELETE10;20" }),
            (r => r.ProgramRenumber(100, 10, 5, 200), new[] { "1;1;RENUM100;10;5;200" }),
            (r => r.FileCopy("A.MB6", "B.MB6"), new[] { "1;1;FCOPYA.MB6;B.MB6" }),
            (r => r.FileDelete("A.MB6"), new[] { "1;1;FDELA.MB6" }),
            (r => r.FileRename("A.MB6", "B.MB6"), new[] { "1;1;FRENAMEA.MB6;B.MB6" }),
            (r => r.FileAttribute("A", true, false), new[] { "1;1;FATTRIBA;+p", "1;1;FATTRIBA;-q" }),
            (r => r.FileAttribute("A", false, true), new[] { "1;1;FATTRIBA;-p", "1;1;FATTRIBA;+q" }),
            (r => r.FileAttribute("A", null, null), Array.Empty<string>()),
            (r => r.LoadProgram("TEST", 3), new[] { "1;3;PRGLOAD=TEST" }),
            (r => r.ProgramUp(), new[] { "1;1;PRGUP" }),
            (r => r.ProgramDown(), new[] { "1;1;PRGDOWN" }),
            (r => r.JumpToProgram(20, 2), new[] { "1;2;LINENO=20" }),
            (r => r.GetProgramLineNumber(2), new[] { "1;2;LINENO" }),
            (r => r.ServoOn = true, new[] { "1;1;SRVON" }),
            (r => r.ServoOn = false, new[] { "1;1;SRVOFF" }),
            (r => r.Override = 50, new[] { "1;1;OVRD=50" }),
            (r => r.Override = 150, new[] { "1;1;OVRD=100" }),
            (r => r.RunProgram("TEST", TaskMode.Cycle, 2), new[] { "1;2;RUNTEST;1" }),
            (r => r.RunProgram("TEST"), new[] { "1;1;RUNTEST;0" }),
            (r => r.StopProgram(2), new[] { "1;2;STOP" }),
            (r => r.BlockProgramStart(true), new[] { "1;1;STOPON" }),
            (r => r.BlockProgramStart(false, 3), new[] { "1;3;STOPOFF" }),
            (r => r.CycleStopProgram(4), new[] { "1;4;CSTOP" }),
            (r => r.ResetAlarm(), new[] { "1;1;RSTALRM" }),
            (r => r.ResetAllPrograms(), new[] { "1;1;SLOTINIT" }),
            (r => r.ResetProgram(3), new[] { "1;3;RSTPRG" }),
            (r => r.ResetIO(), new[] { "1;1;RSTIO" }),
            (r => r.MachineLock = true, new[] { "1;1;MLOCKON" }),
            (r => r.MachineLock = false, new[] { "1;1;MLOCKOFF" }),
            (r => r.SetHand(0, true), new[] { "1;1;HNDON1" }),
            (r => r.SetHand(7, false), new[] { "1;1;HNDOFF8" }),
            (r => r.HandAlign(), new[] { "1;1;ALIGN" }),
            (r => r.MoveToSafePosition(), new[] { "1;1;MOVSP" }),
            (r => r.Jog(ElectricHandJogMode.Move, 50, 30, 5), new[] { "1;1;JOG03;00;32;1e;05" }),
            (r => r.LimitSwitch = true, new[] { "1;1;LSON" }),
            (r => r.LimitSwitch = false, new[] { "1;1;LSOFF" }),
            (r => r.ProgramStart = true, new[] { "1;1;AUEON" }),
            (r => r.ProgramStart = false, new[] { "1;1;AUEOFF" }),
            (r => r.ClearBreakpoint("TEST"), new[] { "1;1;BRKPTCLR=TEST;0" }),
            (r => r.ClearBreakpoint("TEST", 20), new[] { "1;1;BRKPTCLR=TEST;20" }),
            (r => r.Tool = 0, new[] { "1;1;TOOLSET0" }),
            (r => r.Tool = 16, new[] { "1;1;TOOLSET16" }),
            (r => r.LowSpeedMode = true, new[] { "1;1;SAFE=1" }),
            (r => r.LowSpeedMode = false, new[] { "1;1;SAFE=0" }),
            (r => r.WriteOutput(3, 0xFF), new[] { "1;1;OUT=3;00ff" }),
            (r => r.WriteCCLinkOutput(2, 0x1234), new[] { "1;1;DOUT=2;1234" }),
            (r => r.DummyInput = true, new[] { "1;1;INDMY" }),
            (r => r.DummyInput = false, new[] { "1;1;INSET" }),
            (r => r.WriteInput(4, 0xABCD), new[] { "1;1;IN=4;abcd" }),
            (r => r.WriteCCLinkInput(5, 1), new[] { "1;1;DIN=5;0001" }),
            (r => r.ClearHourMeter(HourMeterKind.PowerOnTime), new[] { "1;1;PTIMEDEL=T" }),
            (r => r.ClearHourMeter(HourMeterKind.ServoOnTime), new[] { "1;1;PTIMEDEL=S" }),
            (r => r.ClearHourMeter(HourMeterKind.ProgramOperationTime), new[] { "1;1;PTIMEDEL=D" }),
            (r => r.ClearHourMeter(HourMeterKind.BatteryAccumulationTime), new[] { "1;1;PTIMEDEL=B" }),
            (r => r.ClearHourMeter(HourMeterKind.All), new[] { "1;1;PTIMEDEL=Z" }),
            (r => r.CycleTimeClear("TEST"), new[] { "1;1;CYCLECLRTEST" }),
            (r => r.ClearErrorLog(ErrorSeverity.High), new[] { "1;1;ERRLOGCLR1" }),
            (r => r.ClearErrorLog(ErrorSeverity.Low), new[] { "1;1;ERRLOGCLR2" }),
            (r => r.ClearErrorLog(ErrorSeverity.Caution), new[] { "1;1;ERRLOGCLR3" }),
            (r => r.ClearErrorSummary(), new[] { "1;1;ERRSUMCLR" }),
            (r => r.WriteVariable("TEST", "M1", "5"), new[] { "1;1;HOTTEST;M1=5" }),
            (r => r.ServoMonitorReset(), new[] { "1;1;SRVMONRST=" }),
            (r => r.FactoryReset(), new[] { "1;1;PRMINIT" }),
            (r => r.SetParameter("BZR", "1"), new[] { "1;1;PRM=BZR;1" }),
            (r => r.WriteParameter("BZR", "0"), new[] { "1;1;PAW=BZR;0" }),
            (r => r.WriteParameterAndReboot("BZR", "1"), new[] { "1;1;PAW2=BZR;1" }),
            (r => r.FactoryReset("BZR"), new[] { "1;1;PRMUNDO0;BZR" }),
            (r => r.FactoryReset("BZR", 2), new[] { "1;1;PRMUNDO2;BZR" }),
            (r => r.ParameterKeyword = "KEY", new[] { "1;1;KEYWDpKEY" }),
            (r => r.SlotSet(2, "TEST", TaskMode.Repeat, TaskCondition.Start, 1), new[] { "1;2;SLOTSETTEST;REP;START;1" }),
            (r => r.SlotSet(3, "TEST", TaskMode.Cycle, TaskCondition.Always, 31), new[] { "1;3;SLOTSETTEST;CYC;ALWAYS;31" }),
            (r => r.SlotSet(4, "TEST", TaskMode.Cycle, TaskCondition.Error, 5), new[] { "1;4;SLOTSETTEST;CYC;ERROR;5" }),
            (r => r.ReleaseBrake(0x3F), new[] { "1;1;BREAKON3f" }),
            (r => r.ForceReleaseBrake(0x01), new[] { "1;1;BREAKONF01" }),
            (r => r.SetOrigin(OriginType.CalibrationJig, 0x3F), new[] { "1;1;HOME3;3f" }),
            (r => r.InstallAdditionalAxisData(), new[] { "1;1;AXDATINST" }),
            (r => r.InstallAxisOriginData("A", "B", "C", "D", "E", "F", "CS"), new[] { "1;1;DATINSTA;B;C;D;E;F;CS" }),
            (r => r.InstallAxisOriginData("A", "B", "C", "D", "E", "F", "G", "H", "CS"), new[] { "1;1;DATINSTA;B;C;D;E;F;G;H;CS" }),
            (r => r.ResetPower(), new[] { "1;1;RSTPWR" }),
            (r => r.MaintenanceForecastReset(0x3F, MaintenanceResetTarget.Belt | MaintenanceResetTarget.Grease), new[] { "1;1;MFRST=3f;03" }),
        };
        using var server = Script(cases.SelectMany(c => c.Commands).Select(command => (command, Ok())).ToArray());
        using var robot = server.Connect();
        foreach (var (act, _) in cases)
            act(robot);
        await server.Complete();
    }

    [Fact]
    public async Task ParsesProgramAndFileResponses()
    {
        using var server = Script(
            ("1;1;LISTI20", Ok("20 MOV P1")),
            ("1;1;VTYPRDM1", Ok("M")),
            ("1;1;VTYPRDC1", Ok("C")),
            ("1;1;VTYPRDP1", Ok("P")),
            ("1;1;VTYPRDJ1", Ok("J")),
            ("1;1;VTYPRDX1", Ok("X")),
            ("1;1;LISTCNT0;2147483647", Ok("0005")),
            ("1;1;LISTCNT10;20", Ok("3")),
            ("1;1;LISTPJ1", Ok("J1=(1,2,3,4,5,6,7,8)")),
            ("1;1;LISTPP1", Ok("P1")),
            ("1;1;LISTPP1", Ok("P1=(garbage)")),
            ("1;1;FCHECKA", Ok("N")),
            ("1;1;FCHECKA", Ok("F")),
            ("1;1;FCHECKA", Ok("?")),
            ("1;1;FPATHA", Ok("/PROG/A.MB6")),
            ("1;1;EFREE", Ok("125696;15616;12")),
            ("1;2;LINERD", Ok("10 MOV P1")));
        using var robot = server.Connect();
        Assert.Equal("20 MOV P1", robot.ProgramGetLine(20));
        Assert.Equal(typeof(int), robot.ProgramGetVariableType("M1"));
        Assert.Equal(typeof(char), robot.ProgramGetVariableType("C1"));
        Assert.Equal(typeof(PositionP), robot.ProgramGetVariableType("P1"));
        Assert.Equal(typeof(PositionJ), robot.ProgramGetVariableType("J1"));
        Assert.Throws<InvalidCastException>(() => robot.ProgramGetVariableType("X1"));
        Assert.Equal(5, robot.ProgramCountLines());
        Assert.Equal(3, robot.ProgramCountLines(10, 20));
        var joint = Assert.IsType<PositionJ>(robot.ProgramGetPosition("J1"));
        Assert.Equal(1, joint.J1);
        Assert.Equal(8, joint.J8);
        Assert.Throws<PositionParseException>(() => robot.ProgramGetPosition("P1"));
        Assert.Throws<PositionParseException>(() => robot.ProgramGetPosition("P1"));
        Assert.True(robot.FileExists("A"));
        Assert.False(robot.FileExists("A"));
        Assert.Null(robot.FileExists("A"));
        Assert.Equal("/PROG/A.MB6", robot.FileFullPath("A"));
        Assert.Equal((125696, 15616), robot.FilesystemFree());
        Assert.Equal("10 MOV P1", robot.GetProgramLineContent(2));
        await server.Complete();
    }

    [Fact]
    public async Task ParsesMonitorAndIoResponses()
    {
        using var server = Script(
            ("1;1;OVRD", Ok("50")),
            ("1;1;TOOLRD", Ok("3")),
            ("1;1;ATENA", Ok("2")),
            ("1;1;IOSIGNAL0;10", Ok("0001ABCD")),
            ("1;1;IN0", Ok("00ff")),
            ("1;1;OUT16", Ok("ABCD")),
            ("1;1;DIN0", Ok("1;2;65535")),
            ("1;1;DOUT1", Ok("4")),
            ("1;1;GJPOSF", Ok(JointPositionResponse)),
            ("1;1;GPPOSF", Ok(CartesianPositionResponse)),
            ("1;1;OPNUMRD", Ok("3")),
            ("1;1;THMRD", Ok("38.5")),
            ("1;1;ETEMP", Ok("38;39;41;39;36;36;0;0;41;42;44;41;38;37;0;0")),
            ("1;1;ETEMP", Ok("38;39;41;39;36;36;0;0;41;42;44;41;38;37;0;0")),
            ("1;1;EMISS", Ok("0;1;2;3;4;5;0;0")));
        using var robot = server.Connect();
        Assert.Equal(50, robot.Override);
        Assert.Equal(3, robot.Tool);
        Assert.Throws<InvalidDataException>(() => robot.ProgramStart);
        Assert.Equal(((ushort)1, (ushort)0xABCD), robot.ReadInputOutput(0, 10));
        Assert.Equal(0xFF, robot.ReadInput(0));
        Assert.Equal(0xABCD, robot.ReadOutput(16));
        Assert.Equal(new ushort[] { 1, 2, 65535 }, robot.ReadCCLinkInput(0));
        Assert.Equal(new ushort[] { 4 }, robot.ReadCCLinkOutput(1));
        var joint = robot.DestinationPositionJ;
        Assert.Equal(1.25, joint.J1);
        Assert.Equal(-2.5, joint.J2);
        var cartesian = robot.DestinationPositionP;
        Assert.Equal(1.25, cartesian.X);
        Assert.Equal(7.0, cartesian.L1);
        Assert.Equal(-1, cartesian.Turn1);
        Assert.Equal(-1, cartesian.Turn8);
        Assert.Equal(3, robot.OptionSlotCount);
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Equal(38.5, robot.ControllerTemperature);
            Assert.Equal(new double[] { 38, 39, 41, 39, 36, 36, 0, 0 }, robot.EncoderTemperatures);
            Assert.Equal(new double[] { 41, 42, 44, 41, 38, 37, 0, 0 }, robot.MaximumEncoderTemperatures);
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 0, 0 }, robot.EncoderMissCounts);
        await server.Complete();
    }

    [Fact]
    public async Task ParsesMaintenanceResponses()
    {
        using var server = Script(
            ("1;1;PNRBZR", Ok("BZR;1")),
            ("1;1;PARMEXDEF", Ok("MEXDEF;0,0,0")),
            ("1;1;ENCBATTM", Ok("51;11885")),
            ("1;1;DATRD", Ok("A;B;C;D;E;F;CS")),
            ("1;1;RPWRCHK=", Ok("1")),
            ("1;1;RPWRCHK=", Ok("0")),
            ("1;1;MFFCST=1", Ok("100;200;300")),
            ("1;1;MFFCST=0", Ok("1")),
            ("1;1;MFFCST=2", Ok("2")),
            ("1;1;MFTIME=1", Ok(string.Concat(Enumerable.Repeat("2014/04/09;17:06:16;", 8)) + "0;0;0;0;0;0;0;0")));
        using var robot = server.Connect();
        Assert.Equal("1", robot.GetParameter("BZR"));
        Assert.Equal("0,0,0", robot.ReadParameter("MEXDEF"));
        Assert.Equal((51, 11885), robot.BatteryRemainingTime);
        var (origin, checksum) = robot.ReadAxisOriginData();
        Assert.Equal(new[] { "A", "B", "C", "D", "E", "F" }, origin);
        Assert.Equal("CS", checksum);
        Assert.True(robot.CanResetPower);
        Assert.False(robot.CanResetPower);
        Assert.Equal(new[] { 100, 200, 300 }, robot.MaintenanceForecastHour(MaintenanceType.Grease));
        Assert.Equal(new[] { 1 }, robot.MaintenanceForecastHour(MaintenanceType.Battery));
        Assert.Equal(new[] { 2 }, robot.MaintenanceForecastHour(MaintenanceType.Belt));
        var reset = robot.MaintenanceForecastResetDate(MaintenanceType.Battery);
        Assert.Equal(8, reset.Axes.Length);
        Assert.All(reset.Axes, axis =>
        {
            Assert.Equal(new DateTime(2014, 4, 9, 17, 6, 16), axis.ResetDate);
            Assert.Equal(0, axis.ResetCount);
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.MaintenanceForecastHour((MaintenanceType)0));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.MaintenanceForecastResetDate((MaintenanceType)4));
        await server.Complete();
    }

    [Theory]
    [InlineData("2014/04/09", "17:06:16", "0", 7)]
    [InlineData("2014/13/09", "17:06:16", "0", 8)]
    [InlineData("2014-04-09", "17:06:16", "0", 8)]
    [InlineData("2014/04/09", "", "0", 8)]
    [InlineData("2014/04/09", "17:06:16", "-1", 8)]
    [InlineData("2014/04/09", "17:06:16", "x", 8)]
    public async Task RejectsMalformedMaintenanceResetData(string lastDate, string lastTime, string lastCount, int countFields)
    {
        var response = string.Concat(Enumerable.Repeat("2014/04/09;17:06:16;", 7)) + $"{lastDate};{lastTime};" +
            string.Concat(Enumerable.Repeat("0;", countFields - 1)) + lastCount;
        using var server = Script(("1;1;MFTIME=2", Ok(response)));
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => robot.MaintenanceForecastResetDate(MaintenanceType.Grease));
        await server.Complete();
    }

    [Fact]
    public async Task ReportsControllerErrorState()
    {
        using var server = Script(
            ("1;1;ERROR", Ok()),
            ("1;1;ERROR", Ok("4140;2")),
            ("1;1;ERROR", Ok("4140;2")),
            ("1;1;ERROR", Ok()),
            ("1;1;SAVE", "QeR4140\r"));
        using var robot = server.Connect();
        Assert.Null(robot.Error);
        var error = robot.Error;
        Assert.NotNull(error);
        Assert.Equal(4140, error.ErrorNumber);
        Assert.Null(error.Command);
        Assert.Equal($"4140 ({error.ErrorLevel}) - {error.ErrorMessage}", error.Message);
        Assert.Equal(4140, Assert.Throws<MelfaRobotException>(() => robot.ThrowIfError()).ErrorNumber);
        robot.ThrowIfError();
        var commandError = Assert.Throws<MelfaRobotException>(() => robot.ProgramSave());
        Assert.Equal("1;1;SAVE", commandError.Command);
        Assert.Equal(4140, commandError.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ParsesRunStateFields()
    {
        var payload = "TEST.MB6;00020;100;5;aB0c0;001;1" + new string(';', 8) + "TASK.MB6;CYC;ALWAYS;05;2";
        using var server = Script(("1;3;STATE", Ok(payload)));
        using var robot = server.Connect();
        var state = robot.GetState(3);
        Assert.Equal(3, state.TaskSlot);
        Assert.Equal("TEST.MB6", state.ProgramName);
        Assert.Equal(20, state.LineNumber);
        Assert.Equal(100, state.Override);
        Assert.Equal(EditStatus.Editing | EditStatus.Changed, state.EditStatus);
        Assert.Equal((RunStatus)0xAB, state.RunStatus);
        Assert.Equal((StopStatus)0x0C, state.StopStatus);
        Assert.Equal(0, state.ErrorNumber);
        Assert.Equal(1, state.StepNumber);
        Assert.Equal(MechInfo.Mech1, state.MechInfo);
        Assert.Equal("TASK.MB6", state.TaskProgramName);
        Assert.Equal(TaskMode.Cycle, state.TaskMode);
        Assert.Equal(TaskCondition.Always, state.TaskCondition);
        Assert.Equal(5, state.TaskPriority);
        Assert.Equal(2, state.MechNumber);
        await server.Complete();
    }

    [Theory]
    [InlineData("REP", "START", TaskMode.Repeat, TaskCondition.Start)]
    [InlineData("CYC", "ALWAYS", TaskMode.Cycle, TaskCondition.Always)]
    [InlineData("CYC", "ERROR", TaskMode.Cycle, TaskCondition.Error)]
    [InlineData("XXX", "YYY", TaskMode.Unknown, TaskCondition.Unknown)]
    public async Task ParsesTaskModesAndConditions(string mode, string condition, TaskMode expectedMode, TaskCondition expectedCondition)
    {
        var state = "TEST.MB6;00020;100;5;aB0c0;001;1" + new string(';', 9) + $"{mode};{condition};01;0";
        using var server = Script(("1;2;STATE", Ok(state)), ("1;2;SLOTRD", Ok($"TEST;{mode};{condition};7")));
        using var robot = server.Connect();
        var runState = robot.GetState(2);
        Assert.Equal(expectedMode, runState.TaskMode);
        Assert.Equal(expectedCondition, runState.TaskCondition);
        Assert.Equal("", runState.TaskProgramName);
        var slot = robot.SlotGet(2);
        Assert.Equal(2, slot.Slot);
        Assert.Equal("TEST", slot.ProgramName);
        Assert.Equal(expectedMode, slot.Mode);
        Assert.Equal(expectedCondition, slot.Condition);
        Assert.Equal(7, slot.Priority);
        await server.Complete();
    }

    [Fact]
    public void ParsesRobotInformationFields()
    {
        var info = new MelfaRobot.RobotInformation("QoK3F;0F;7,1;3,5,A,1E,32,46,64;MB6;PRM;RV-7FR-D;CR8xx-D;MELFA;17-11-29;Ver.A1dC;ENG;COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;5;8;\r");
        Assert.Equal(0x3F, info.AxisPatternXYZ);
        Assert.Equal(0x0F, info.AxisPatternJoint);
        Assert.Equal(7, info.StructuralFlag1);
        Assert.Equal(1, info.StructuralFlag2);
        Assert.Equal(new byte[] { 3, 5, 0xA, 0x1E, 0x32, 0x46, 0x64 }, info.JogSpeed);
        Assert.Equal("MB6", info.ProgramExtension);
        Assert.Equal("PRM", info.ParameterExtension);
        Assert.Equal("RV-7FR-D", info.RobotType);
        Assert.Equal("CR8xx-D", info.Controller);
        Assert.Equal("MELFA", info.Series);
        Assert.Equal(new DateTime(2017, 11, 29), info.ReleaseDate);
        Assert.Equal("Ver.A1dC", info.Version);
        Assert.Equal("ENG", info.Language);
        Assert.Equal("COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED", info.Copyright);
        Assert.Equal("1", info.RobotInfo);
        Assert.Equal("5", info.SerialNumber);
        Assert.Equal(8, info.MaximumTaskCount);
    }

    [Fact]
    public void ValidatesArgumentsBeforeSending()
    {
        using var robot = new MelfaRobot("127.0.0.1", -1, 2);
        Assert.Equal(MelfaRobot.DEFAULT_PORT, robot.Port);
        Assert.Equal("127.0.0.1", robot.Host);
        Assert.Equal(2, robot.RobotNumber);
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.CommandTimeout = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.CommandTimeout = -1);
        robot.CommandTimeout = 1234;
        Assert.Equal(1234, robot.CommandTimeout);
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.SetHand(-1, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.SetHand(8, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.Tool = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.Tool = 17);
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ProgramStep((StepMethod)0));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ClearHourMeter((HourMeterKind)0));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.SlotSet(1, "TEST", TaskMode.Unknown, TaskCondition.Start, 1));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.SlotSet(1, "TEST", TaskMode.Repeat, TaskCondition.Unknown, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetOptionStatus(0));
        Assert.False(robot.IsConnected);
        Assert.Equal("Not connected.", Assert.Throws<InvalidOperationException>(() => robot.ProgramSave()).Message);
    }

    [Fact]
    public async Task ConnectIsIdempotentAndCommandsFailAfterDisconnect()
    {
        using var server = Script(("1;1;SAVE", Ok()));
        using var robot = server.Connect();
        robot.Connect();
        Assert.True(robot.IsConnected);
        robot.ProgramSave();
        robot.Disconnect();
        Assert.False(robot.IsConnected);
        Assert.Throws<InvalidOperationException>(() => robot.ProgramSave());
        robot.Disconnect();
        await server.Complete();
    }

    [Fact]
    public void ConnectFailureLeavesRobotDisconnected()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        using var robot = new MelfaRobot("127.0.0.1", port);
        Assert.ThrowsAny<SocketException>(() => robot.Connect());
        Assert.False(robot.IsConnected);
    }

    [Fact]
    public async Task ControllerClosingMidResponseDisconnects()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;SAVE", await ReadCommand(stream));
            await Send(stream, "QoK");
        });
        using var robot = server.Connect();
        Assert.Throws<EndOfStreamException>(() => robot.ProgramSave());
        Assert.False(robot.IsConnected);
        await server.Complete();
    }

    [Theory]
    [InlineData("XYZ\r")]
    [InlineData("QeRabcd\r")]
    [InlineData("QeR12\r")]
    public async Task RejectsResponsesWithInvalidPrefix(string response)
    {
        using var server = Script(("1;1;SAVE", response), ("1;1;SAVE", Ok()));
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => robot.ProgramSave());
        Assert.True(robot.IsConnected);
        robot.ProgramSave();
        await server.Complete();
    }

    [Fact]
    public async Task RejectsOversizedResponses()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "QoK" + new string('A', 65536) + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => robot.GetCurrentProgram());
        await server.Complete();
    }

    [Fact]
    public async Task FileReadClosesFileWhenReadFails()
    {
        using var server = Script(
            ("1;1;FOPENTEST.MB6;r", Ok()),
            ("1;1;FREAD", "QeR4140\r"),
            ("1;1;FCLOSE", Ok()),
            ("1;1;FOPENTEST.MB6;r", Ok()),
            ("1;1;FREAD", "QeR4140\r"),
            ("1;1;FCLOSE", "QeR4150\r"));
        using var robot = server.Connect();
        Assert.Equal(4140, Assert.Throws<MelfaRobotException>(() => robot.FileRead("TEST.MB6")).ErrorNumber);
        var aggregate = Assert.Throws<AggregateException>(() => robot.FileRead("TEST.MB6"));
        Assert.Equal(new[] { 4140, 4150 }, aggregate.InnerExceptions.Cast<MelfaRobotException>().Select(e => e.ErrorNumber));
        await server.Complete();
    }

    [Theory]
    [InlineData("3")]
    [InlineData("C")]
    public async Task ParsesCautionErrorHistorySeverity(string severity)
    {
        using var server = Script(("1;1;ERRORLOGTOP", Ok($"04-02-09;18:49:52;4140;message;{severity};S1;0;414000000")));
        using var robot = server.Connect();
        Assert.Equal(ErrorSeverity.Caution, robot.GetErrorHistory(LocationType.Top)!.Severity);
        await server.Complete();
    }
}
