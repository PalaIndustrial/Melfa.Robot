using System.Collections.Immutable;
using System.Globalization;
using Xunit;

namespace Melfa.Robot.Tests;

public partial class ProtocolTests
{
    [Fact]
    public async Task GeneratedResultsRoundTripThroughTheProtocol()
    {
        var breakpoint = new MelfaRobot.Breakpoint("MAIN.MB6", 42, BreakpointType.OneTime);
        var calibration = new MelfaRobot.CalibrationInfo(true, 0x3f, "3F");
        var detailed = new MelfaRobot.DetailedState(3, (RunStatus)0xa5, (StopStatus)0x0e, 4140, 12, 1);
        var signals = new MelfaRobot.StopSignalState((StopSignalSource)0x0b, (EmergencyStopSource)0x85);
        var hands = new[]
        {
            new MelfaRobot.HandState(900, true, HandType.Single),
            new MelfaRobot.HandState(902, false, HandType.Double),
            new MelfaRobot.HandState(-1, null, HandType.NotUsed),
        };
        var info = new MelfaRobot.RobotInformation(0x3f, 0x3f, 7, 0, ImmutableArray.Create<byte>(3, 5, 10, 30, 50, 70, 100),
            "MB6", "PRM", "RV-7FR-D", "CR8xx-D", "MELFA", new DateTime(2017, 11, 29), "Ver.A1dC", "ENG",
            "COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED", "1", "5", 8);
        var state = new MelfaRobot.RunState(4, "MAIN.MB6", 20, 100, EditStatus.Editing | EditStatus.Changed,
            (RunStatus)0xab, (StopStatus)0x0c, 0, 1, MechInfo.Mech1, "MAIN.MB6", TaskMode.Cycle, TaskCondition.Always, 1, 0);
        var slot = new MelfaRobot.TaskSlotInfo(4, "MAIN.MB6", TaskMode.Repeat, TaskCondition.Error, 31);
        var hourMeter = new MelfaRobot.HourMeterData(149, 51, ImmutableArray.Create(51L, 2L, 3L),
            ImmutableArray.Create(new long[12]), 102, 0, 2715, 11885, 1);
        var cycle = new MelfaRobot.CycleTimeData(224745, 156, 154, 1454);
        var history = new MelfaRobot.ErrorHistoryEntry(new MelfaRobot.ErrorTimestamp("04-02-09", "18:49:52"), 4140,
            "The program was not found", ErrorSeverity.Caution, "S1", 7, "414000000");
        var summary = new MelfaRobot.ErrorSummary(4, new MelfaRobot.ErrorSummaryEntry("006000000", ErrorSeverity.High,
            "EMG signal is input.", "Cause", "Check emergency stop", 4));
        var datedSummary = new MelfaRobot.ErrorSummary(4, new MelfaRobot.ErrorSummaryEntry("006000000", ErrorSeverity.Low,
            "EMG signal is input.", "Cause", "Check emergency stop", 4, new MelfaRobot.ErrorTimestamp("10-08-19", "18:49:52")), true);
        var emptySummary = new MelfaRobot.ErrorSummary(4, null);
        var emptyDatedSummary = new MelfaRobot.ErrorSummary(4, null, true);
        var page = new MelfaRobot.VariablePage(ImmutableArray.Create(
            new KeyValuePair<string, string>("M1", "+1"),
            new KeyValuePair<string, string>("C1", "\"a;=b\"")), true);

        using var server = Script(
            ("1;1;BRKPTGET=MAIN.MB6;1;0;100", Ok(breakpoint.ToString())),
            ("1;1;CALIB", Ok(calibration.ToString())),
            ("1;3;DSTATE", Ok(detailed.ToString())),
            ("1;1;STPSIG", Ok(signals.ToString())),
            ("1;1;HNDSTS", Ok(string.Join(";", hands.Select(hand => hand.ToString())))),
            ("1;1;OPEN=TEST", Ok(info.ToString())),
            ("1;4;STATE", Ok(state.ToString())),
            ("1;4;SLOTRD", Ok(slot.ToString())),
            ("1;1;PTIME", Ok(hourMeter.ToString())),
            ("1;1;CYCLETIMEMAIN", Ok(cycle.ToString())),
            ("1;1;ERRORLOGTOP", Ok(history.ToString())),
            ("1;1;ERRSUM1", Ok(summary.ToString())),
            ("1;1;ERRSUM2=1", Ok(datedSummary.ToString())),
            ("1;1;ERRSUM2", Ok(emptySummary.ToString())),
            ("1;1;ERRSUM2=2", Ok(emptyDatedSummary.ToString())),
            ("1;1;VALS<;M", Ok(page.ToString())));
        using var robot = server.Connect();
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var readBreakpoint = robot.GetBreakpoint("MAIN.MB6", 1, 0, 100);
            Assert.Equal((breakpoint.ProgramName, breakpoint.LineNumber, breakpoint.Type),
                (readBreakpoint.ProgramName, readBreakpoint.LineNumber, readBreakpoint.Type));

            var readCalibration = robot.Calibration;
            Assert.Equal((calibration.IsDefined, calibration.CompletedAxisMask, calibration.Reserved),
                (readCalibration.IsDefined, readCalibration.CompletedAxisMask, readCalibration.Reserved));

            Assert.Equal(detailed, robot.GetDetailedState(3));
            Assert.Equal(signals, robot.StopSignals);
            Assert.Equal(hands, robot.HandStates.Take(hands.Length));

            var readInfo = robot.Open("TEST");
            Assert.Equal(info.ToString(), readInfo.ToString());
            Assert.Equal(info.JogSpeed.ToArray(), readInfo.JogSpeed.ToArray());
            Assert.Equal(info.ReleaseDate, readInfo.ReleaseDate);
            Assert.Equal(info.MaximumTaskCount, readInfo.MaximumTaskCount);

            var readState = robot.GetState(4);
            Assert.Equal(state.ToString(), readState.ToString());
            Assert.Equal(state.EditStatus, readState.EditStatus);
            Assert.Equal(state.RunStatus, readState.RunStatus);
            Assert.Equal(state.TaskMode, readState.TaskMode);
            Assert.Equal(state.TaskCondition, readState.TaskCondition);

            var readSlot = robot.SlotGet(4);
            Assert.Equal((slot.Slot, slot.ProgramName, slot.Mode, slot.Condition, slot.Priority),
                (readSlot.Slot, readSlot.ProgramName, readSlot.Mode, readSlot.Condition, readSlot.Priority));

            var readHourMeter = robot.HourMeter;
            Assert.Equal(hourMeter.ToString(), readHourMeter.ToString());
            Assert.Equal(hourMeter.MechanismServoOnHours.ToArray(), readHourMeter.MechanismServoOnHours.ToArray());

            Assert.Equal(cycle.ToString(), robot.GetCycleTime("MAIN").ToString());

            var readHistory = robot.GetErrorHistory(LocationType.Top);
            Assert.Equal(history.ToString(), readHistory.ToString());
            Assert.Equal(ErrorSeverity.Caution, readHistory.Severity);
            Assert.Equal(7, readHistory.LineNumber);

            var readSummary = robot.GetErrorSummary(1);
            Assert.Equal(summary.ToString(), readSummary.ToString());
            Assert.Equal(ErrorSeverity.High, readSummary.Entry.Severity);
            Assert.False(readSummary.IncludesLastOccurrence);

            var readDatedSummary = robot.GetErrorSummaryWithLastOccurrence(1);
            Assert.Equal(datedSummary.ToString(), readDatedSummary.ToString());
            Assert.Equal("10-08-19", readDatedSummary.Entry.LastOccurrence.Date);
            Assert.True(readDatedSummary.IncludesLastOccurrence);

            var readEmptySummary = robot.GetErrorSummary(2);
            Assert.Null(readEmptySummary.Entry);
            Assert.Equal("4;000000000;;;;;0", readEmptySummary.ToString());

            var readEmptyDatedSummary = robot.GetErrorSummaryWithLastOccurrence(2);
            Assert.Null(readEmptyDatedSummary.Entry);
            Assert.Equal("4;000000000;;;;;0;;", readEmptyDatedSummary.ToString());
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }

        var readPage = robot.ReadVariables(LocationType.Top, VariableClass.Integer);
        Assert.Equal(page.Variables.ToArray(), readPage.Variables.ToArray());
        Assert.True(readPage.HasMore);
        Assert.Equal("M1=+1\vC1=\"a;=b\";2;1", readPage.ToString());
        await server.Complete();
    }

    [Fact]
    public async Task GeneratedServoMonitorDataRoundTripsThroughTheProtocol()
    {
        var encoder = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoEncoderAxis(-i * 1000L, i, i * 2L, i * 3L)).ToImmutableArray();
        var droop = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoDroopAxis(i, -i * 10L)).ToImmutableArray();
        var speed = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoSpeedAxis(i, i * 100L, -i)).ToImmutableArray();
        var commands = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoCurrentCommandAxis(i / 3d, 1.25, -2.5, 0.1 * i)).ToImmutableArray();
        var feedback = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoCurrentFeedbackAxis(-i / 7d, 0.49, 3.3)).ToImmutableArray();
        var load = Enumerable.Range(1, 8).Select(i => new MelfaRobot.ServoLoadCurrentAxis(i * 1.5, 35)).ToImmutableArray();
        var voltage = new MelfaRobot.ServoVoltageInfo(200.5, 210.75, 190.25, ImmutableArray.Create(0.1, 0.2, 0.3, 0, 0, 0, 0, 1e-7));
        static string Join<T>(ImmutableArray<T> axes) => string.Join(";", axes.Select(axis => axis.ToString()));

        using var server = Script(
            ("1;1;SRVENC", Ok(Join(encoder))),
            ("1;1;SRVDRP", Ok(Join(droop))),
            ("1;1;SRVSPD", Ok(Join(speed))),
            ("1;1;SRVCUR0", Ok(Join(commands))),
            ("1;1;SRVCUR1", Ok(Join(feedback))),
            ("1;1;SRVLCR", Ok(Join(load))),
            ("1;1;SRVVOL", Ok(voltage.ToString())));
        using var robot = server.Connect();
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal(encoder.ToArray(), robot.ServoEncoder.ToArray());
            Assert.Equal(droop.ToArray(), robot.ServoDroop.ToArray());
            Assert.Equal(speed.ToArray(), robot.ServoSpeed.ToArray());
            Assert.Equal(commands.ToArray(), robot.ServoCurrentCommands.ToArray());
            Assert.Equal(feedback.ToArray(), robot.ServoCurrentFeedback.ToArray());
            Assert.Equal(load.ToArray(), robot.ServoLoadCurrent.ToArray());
            var readVoltage = robot.ServoVoltage;
            Assert.Equal(voltage, readVoltage);
            Assert.True(voltage == readVoltage);
            Assert.Equal(voltage.GetHashCode(), readVoltage.GetHashCode());
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        await server.Complete();
    }

    [Fact]
    public void ServoMonitorStructsCompareByValue()
    {
        Assert.True(new MelfaRobot.ServoDroopAxis(1, 2) == new MelfaRobot.ServoDroopAxis(1, 2));
        Assert.True(new MelfaRobot.ServoSpeedAxis(1, 2, 3) != new MelfaRobot.ServoSpeedAxis(1, 2, 4));
        Assert.False(new MelfaRobot.ServoEncoderAxis(1, 2, 3, 4).Equals((object)(1L, 2L, 3L, 4L)));
        Assert.Equal(default(MelfaRobot.ServoVoltageInfo), new MelfaRobot.ServoVoltageInfo(0, 0, 0, ImmutableArray<double>.Empty));
        Assert.NotEqual(new MelfaRobot.ServoVoltageInfo(1, 2, 3, ImmutableArray.Create(1d)),
            new MelfaRobot.ServoVoltageInfo(1, 2, 3, ImmutableArray.Create(2d)));
        Assert.Equal("0;0;0", default(MelfaRobot.ServoVoltageInfo).ToString());
        Assert.Equal("0;0", default(MelfaRobot.ServoLoadCurrentAxis).ToString());
    }

    [Fact]
    public async Task GeneratedMaintenanceResetDataRoundTripsThroughTheProtocol()
    {
        var generated = new MelfaRobot.MaintenanceResetData(Enumerable.Range(1, 8).Select(i =>
            new MelfaRobot.MaintenanceResetAxis(i == 7 ? null : new DateTime(2020 + i, i, i * 3, i * 2, i * 7, i * 5), i * 11)));
        Assert.Equal("2021/01/03;02:07:05;2022/02/06;04:14:10;2023/03/09;06:21:15;2024/04/12;08:28:20;" +
            "2025/05/15;10:35:25;2026/06/18;12:42:30;;;2028/08/24;16:56:40;11;22;33;44;55;66;77;88", generated.ToString());

        using var server = Script(("1;1;MFTIME=3", Ok(generated.ToString())));
        using var robot = server.Connect();
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var parsed = robot.MaintenanceForecastResetDate(MaintenanceType.Belt);
            Assert.Equal(generated.Axes.ToArray(), parsed.Axes.ToArray());
            Assert.Equal(generated.ToString(), parsed.ToString());
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        await server.Complete();
    }

    [Fact]
    public void MaintenanceResetDataValidatesAndComparesByValue()
    {
        Assert.Throws<ArgumentNullException>(() => new MelfaRobot.MaintenanceResetData(null));
        Assert.Throws<ArgumentException>(() => new MelfaRobot.MaintenanceResetData(new MelfaRobot.MaintenanceResetAxis[7]));
        var date = new DateTime(2014, 4, 9, 17, 6, 16);
        Assert.True(new MelfaRobot.MaintenanceResetAxis(date, 1) == new MelfaRobot.MaintenanceResetAxis(date, 1));
        Assert.True(new MelfaRobot.MaintenanceResetAxis(date, 1) != new MelfaRobot.MaintenanceResetAxis(null, 1));
        Assert.False(new MelfaRobot.MaintenanceResetAxis(date, 1).Equals((object)1));
        Assert.Equal(new MelfaRobot.MaintenanceResetAxis(date, 1).GetHashCode(), new MelfaRobot.MaintenanceResetAxis(date, 1).GetHashCode());
        Assert.Equal("2014/04/09;17:06:16;1", new MelfaRobot.MaintenanceResetAxis(date, 1).ToString());
        Assert.Equal(";;0", default(MelfaRobot.MaintenanceResetAxis).ToString());
        Assert.Equal(new string(';', 15) + ";0;0;0;0;0;0;0;0",
            new MelfaRobot.MaintenanceResetData(new MelfaRobot.MaintenanceResetAxis[8]).ToString());
    }

    [Fact]
    public void GeneratedResultsFormatEdgeValues()
    {
        Assert.Equal("100;-1;0", new MelfaRobot.HandState(100, null, HandType.Single).ToString());
        Assert.Equal("FF0", new MelfaRobot.CalibrationInfo(false, 0xf0).ToString());
        Assert.Equal(";;;0", new MelfaRobot.TaskSlotInfo(1, "", TaskMode.Unknown, TaskCondition.Unknown, 0).ToString());
        Assert.Equal(";0;0", new MelfaRobot.VariablePage(default, false).ToString());
        Assert.Equal("0;0;0;0;0;0;0", new MelfaRobot.HourMeterData(0, 0, default, default, 0, 0, 0, 0, 0).ToString());
        Assert.Equal("4;000000000;;;;;0;;", new MelfaRobot.ErrorSummary(4,
            new MelfaRobot.ErrorSummaryEntry("000000000", ErrorSeverity.Unknown, "", "", "", 0), true).ToString());
        Assert.Equal("FFFFFFFF;80000000", new MelfaRobot.StopSignalState((StopSignalSource)uint.MaxValue, (EmergencyStopSource)0x80000000).ToString());
    }
}
