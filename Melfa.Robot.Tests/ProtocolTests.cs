using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace Melfa.Robot.Tests;

public class ProtocolTests
{
    [Fact]
    public async Task ReadsBreakpointsByNumberAndLineRange()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;BRKPTGET=TEST;1;10;200", await ReadCommand(stream));
            await Send(stream, "QoKTEST;00020;0\r");
            Assert.Equal("1;1;BRKPTGET=;128;0;0", await ReadCommand(stream));
            await Send(stream, "QoKOTHER.MB6;123;1\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetBreakpoint("TEST", 0, 0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetBreakpoint("TEST", 129, 0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetBreakpoint("TEST", 1, -1, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetBreakpoint("TEST", 1, 0, -1));
        Assert.Throws<ArgumentException>(() => robot.GetBreakpoint("TEST;STOP", 1, 0, 100));
        Assert.Throws<ArgumentException>(() => robot.GetBreakpoint("TEST\rSTOP", 1, 0, 100));
        Assert.Throws<ArgumentException>(() => robot.GetBreakpoint(" ", 1, 0, 100));
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var first = robot.GetBreakpoint("TEST", 1, 10, 200);
            Assert.Equal("TEST", first.ProgramName);
            Assert.Equal(20, first.LineNumber);
            Assert.Equal(BreakpointType.Continuous, first.Type);
            Assert.Equal("TEST;00020;0", first.ToString());
            var last = robot.GetBreakpoint(null!, 128, 0, 0);
            Assert.Equal("OTHER.MB6", last.ProgramName);
            Assert.Equal(123, last.LineNumber);
            Assert.Equal(BreakpointType.OneTime, last.Type);
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        await server.Complete();
    }

    [Theory]
    [InlineData("")]
    [InlineData("TEST;20")]
    [InlineData("TEST;20;0;extra")]
    [InlineData(";20;0")]
    [InlineData("TEST;abc;0")]
    [InlineData("TEST;-1;0")]
    [InlineData("TEST;20;2")]
    [InlineData("TEST;20;01")]
    public async Task RejectsMalformedBreakpointResponses(string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;BRKPTGET=TEST;1;1;100", await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => robot.GetBreakpoint("TEST", 1, 1, 100));
        await server.Complete();
    }

    [Fact]
    public async Task BreakpointReadSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;BRKPTGET=TEST;1;1;100", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => robot.GetBreakpoint("TEST", 1, 1, 100));
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public void OptionStatusRequiresASlot()
    {
        Assert.Null(typeof(MelfaRobot).GetMethod(nameof(MelfaRobot.GetOptionStatus), Type.EmptyTypes));
        Assert.NotNull(typeof(MelfaRobot).GetMethod(nameof(MelfaRobot.GetOptionStatus), new[] { typeof(int) }));
    }

    [Fact]
    public async Task ReadsOptionStatusWithoutChangingControllerData()
    {
        const string information = "Ethernet;IP Addr 192.168.0.1\vPortNo(R-time) 10000\vPortNo(OPT11-19) 10001/10002/10003/10004/10005/10006/10007/10008/10009\vConnect Count(OPT11-19) 2/0/0/0/0/0/0/0/0\vMAC Addr 00:00:00:00:00:00\vH/W Ver: 0";
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;OPSTSRD1", await ReadCommand(stream));
            await Send(stream, "QoK" + information + "\r");
            Assert.Equal("1;1;OPSTSRD2", await ReadCommand(stream));
            await Send(stream, "QoKOther option;unknown format\v\r");
            Assert.Equal("1;1;OPSTSRD3", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;OPSTSRD4", await ReadCommand(stream));
            await Send(stream, "QoK" + information + "\r");
        });
        using var robot = server.Connect();
        Assert.Equal(information, robot.GetOptionStatus(1));
        Assert.Equal("Other option;unknown format\v", robot.GetOptionStatus(2));
        Assert.Equal("", robot.GetOptionStatus(3));
        Assert.Equal(information, robot.GetOptionStatus(4));
        await server.Complete();
    }

    [Fact]
    public async Task RejectsZeroOptionSlot()
    {
        using var server = new Controller(_ => Task.CompletedTask);
        using var robot = server.Connect();
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetOptionStatus(0));
        await server.Complete();
    }

    [Fact]
    public async Task OptionStatusReadSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;OPSTSRD2", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => robot.GetOptionStatus(2));
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsCalibrationStatusAndAxisMask()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;CALIB", await ReadCommand(stream));
            await Send(stream, "QoKN3F3F\r");
            Assert.Equal("1;1;CALIB", await ReadCommand(stream));
            await Send(stream, "QoKF8a00\r");
            Assert.Equal("1;1;CALIB", await ReadCommand(stream));
            await Send(stream, "QoKN00\r");
        });
        using var robot = server.Connect();
        var defined = robot.Calibration;
        Assert.True(defined.IsDefined);
        Assert.Equal(0x3f, defined.CompletedAxisMask);
        Assert.Equal("3F", defined.Reserved);
        Assert.Equal("N3F3F", defined.ToString());
        var undefined = robot.Calibration;
        Assert.False(undefined.IsDefined);
        Assert.Equal(0x8a, undefined.CompletedAxisMask);
        Assert.Equal("00", undefined.Reserved);
        Assert.Equal("F8a00", undefined.ToString());
        var noReservedData = robot.Calibration;
        Assert.True(noReservedData.IsDefined);
        Assert.Equal(0, noReservedData.CompletedAxisMask);
        Assert.Equal("", noReservedData.Reserved);
        await server.Complete();
    }

    [Theory]
    [InlineData("")]
    [InlineData("N")]
    [InlineData("N3")]
    [InlineData("X3F3F")]
    [InlineData("n3F3F")]
    [InlineData("N3G3F")]
    [InlineData("N 13F")]
    public async Task RejectsMalformedCalibrationResponses(string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;CALIB", await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => _ = robot.Calibration);
        await server.Complete();
    }

    [Fact]
    public async Task CalibrationReadSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;CALIB", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => _ = robot.Calibration);
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsDetailedStopStateAndSignalMasks()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;DSTATE", await ReadCommand(stream));
            await Send(stream, "QoK09060;17;1\r");
            Assert.Equal("1;4;DSTATE", await ReadCommand(stream));
            await Send(stream, "QoKa5fE4140;123;3\r");
            Assert.Equal("1;1;STPSIG", await ReadCommand(stream));
            await Send(stream, "QoK0;0\r");
            Assert.Equal("1;1;STPSIG", await ReadCommand(stream));
            await Send(stream, "QoK0b;85\r");
        });
        using var robot = server.Connect();
        var state = robot.GetDetailedState();
        Assert.Equal(1, state.TaskSlot);
        Assert.Equal(RunStatus.Repeat | RunStatus.Teach, state.RunStatus);
        Assert.Equal(StopStatus.Stop | StopStatus.Wait, state.StopStatus);
        Assert.Equal(0, state.ErrorNumber);
        Assert.Equal(17, state.StepNumber);
        Assert.Equal(1, state.MechanismNumber);
        Assert.Equal("09060;17;1", state.ToString());
        var other = robot.GetDetailedState(4);
        Assert.Equal(4, other.TaskSlot);
        Assert.Equal((RunStatus)0xa5, other.RunStatus);
        Assert.Equal((StopStatus)0xfe, other.StopStatus);
        Assert.Equal(4140, other.ErrorNumber);
        Assert.Equal(123, other.StepNumber);
        Assert.Equal(3, other.MechanismNumber);
        Assert.Equal("a5fE4140;123;3", other.ToString());
        var clear = robot.StopSignals;
        Assert.Equal(StopSignalSource.None, clear.Stop);
        Assert.Equal(EmergencyStopSource.None, clear.EmergencyStop);
        Assert.Equal("0;0", clear.ToString());
        var signals = robot.StopSignals;
        Assert.Equal(StopSignalSource.TeachingBox | StopSignalSource.PersonalComputer | StopSignalSource.OperationPanel, signals.Stop);
        Assert.Equal((EmergencyStopSource)0x85, signals.EmergencyStop);
        Assert.Equal("0b;85", signals.ToString());
        Assert.True((signals.EmergencyStop & EmergencyStopSource.InputOutput) != 0);
        Assert.True((signals.EmergencyStop & EmergencyStopSource.TeachingBox) != 0);
        Assert.False((signals.EmergencyStop & EmergencyStopSource.OperationPanel) != 0);
        await server.Complete();
    }

    [Theory]
    [InlineData("DSTATE", "")]
    [InlineData("DSTATE", "0906;17;1")]
    [InlineData("DSTATE", "0g060;17;1")]
    [InlineData("DSTATE", "090x0;17;1")]
    [InlineData("DSTATE", "0906X;17;1")]
    [InlineData("DSTATE", "09060;-1;1")]
    [InlineData("DSTATE", "09060;17;1;0")]
    [InlineData("STPSIG", "0")]
    [InlineData("STPSIG", "0;")]
    [InlineData("STPSIG", "0x1;0")]
    [InlineData("STPSIG", "100000000;0")]
    [InlineData("STPSIG", "0;G")]
    [InlineData("STPSIG", "0;0;0")]
    public async Task RejectsMalformedStopStatusResponses(string command, string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;" + command, await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() =>
        {
            if (command == "DSTATE")
                _ = robot.GetDetailedState();
            else
                _ = robot.StopSignals;
        });
        await server.Complete();
    }

    [Theory]
    [InlineData("DSTATE")]
    [InlineData("STPSIG")]
    public async Task StopStatusReadsSurfaceControllerErrors(string command)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;" + command, await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() =>
        {
            if (command == "DSTATE")
                _ = robot.GetDetailedState();
            else
                _ = robot.StopSignals;
        });
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsAndSetsControllerClockAndReadsTimeMeters()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;TIME", await ReadCommand(stream));
            await Send(stream, "QoK04-02-0917:54:32\r");
            Assert.Equal("1;1;TIME=04-02-0917:00:00", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;PTIME", await ReadCommand(stream));
            await Send(stream, "QoK149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;2715;11885;1\r");
            Assert.Equal("1;1;CYCLETIME100", await ReadCommand(stream));
            await Send(stream, "QoK224745;156;154;1454\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.SetControllerClock("04-02-30", "17:00:00"));
        Assert.Throws<ArgumentException>(() => robot.SetControllerClock("04-02-09", "25:00:00"));
        Assert.Throws<ArgumentException>(() => robot.SetControllerClock("04-02-09", "17:00:00\rSTOP"));
        Assert.Throws<ArgumentException>(() => robot.GetCycleTime(""));
        Assert.Throws<ArgumentException>(() => robot.GetCycleTime("100;STOP"));
        Assert.Throws<ArgumentException>(() => robot.GetCycleTime("100\rSTOP"));
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var clock = robot.ControllerClock;
            Assert.Equal("04-02-09", clock.Date);
            Assert.Equal("17:54:32", clock.Time);
            robot.SetControllerClock("04-02-09", "17:00:00");
            var hourMeter = robot.HourMeter;
            Assert.Equal(149, hourMeter.PowerOnHours);
            Assert.Equal(51, hourMeter.AllMechanismsServoOnHours);
            Assert.Equal(new long[] { 51, 0, 0 }, hourMeter.MechanismServoOnHours);
            Assert.Equal(new long[12], hourMeter.ReservedFields);
            Assert.Equal(102, hourMeter.ProgramOperationHours);
            Assert.Equal(0, hourMeter.ReservedAfterProgram);
            Assert.Equal(2715, hourMeter.BatteryAccumulationHours);
            Assert.Equal(11885, hourMeter.BatteryRemainingHours);
            Assert.Equal(1, hourMeter.MechanismInformation);
            Assert.Equal("149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;2715;11885;1", hourMeter.ToString());
            var cycle = robot.GetCycleTime("100");
            Assert.Equal(224745, cycle.OperationMilliseconds);
            Assert.Equal(156, cycle.LatestCycleMilliseconds);
            Assert.Equal(154, cycle.AverageCycleMilliseconds);
            Assert.Equal(1454, cycle.CycleCount);
            Assert.Equal("224745;156;154;1454", cycle.ToString());
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        await server.Complete();
    }

    [Theory]
    [InlineData("TIME", "04-02-0917:54")]
    [InlineData("TIME", "04-02-3017:54:32")]
    [InlineData("TIME", "04-02-0917:54:32;")]
    [InlineData("PTIME", "149;51")]
    [InlineData("PTIME", "149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;2715;11885;1;0")]
    [InlineData("PTIME", "149;51;51;0;0;0;0;0;0;0;0;0;0;0;0;0;0;102;0;invalid;11885;1")]
    [InlineData("CYCLETIME", "224745;156;154")]
    [InlineData("CYCLETIME", "224745;156;154;1454;0")]
    [InlineData("CYCLETIME", "224745;156;NaN;1454")]
    public async Task RejectsMalformedTimeResponses(string command, string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;" + (command == "CYCLETIME" ? "CYCLETIME100" : command), await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() =>
        {
            switch (command)
            {
                case "TIME": _ = robot.ControllerClock; break;
                case "PTIME": _ = robot.HourMeter; break;
                case "CYCLETIME": _ = robot.GetCycleTime("100"); break;
                default: throw new InvalidOperationException();
            }
        });
        await server.Complete();
    }

    [Fact]
    public async Task MapsAllHourMeterFieldsInOrder()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PTIME", await ReadCommand(stream));
            await Send(stream, "QoK1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22\r");
        });
        using var robot = server.Connect();
        var meter = robot.HourMeter;
        Assert.Equal(1, meter.PowerOnHours);
        Assert.Equal(2, meter.AllMechanismsServoOnHours);
        Assert.Equal(new long[] { 3, 4, 5 }, meter.MechanismServoOnHours);
        Assert.Equal(Enumerable.Range(6, 12).Select(i => (long)i), meter.ReservedFields);
        Assert.Equal(18, meter.ProgramOperationHours);
        Assert.Equal(19, meter.ReservedAfterProgram);
        Assert.Equal(20, meter.BatteryAccumulationHours);
        Assert.Equal(21, meter.BatteryRemainingHours);
        Assert.Equal(22, meter.MechanismInformation);
        await server.Complete();
    }

    [Fact]
    public async Task SettingControllerClockSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;TIME=99-12-3123:59:59", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => robot.SetControllerClock("99-12-31", "23:59:59"));
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsErrorHistoryAndFilteredHistory()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;ERRORMES4140", await ReadCommand(stream));
            await Send(stream, "QoKThe program was not found\r");
            Assert.Equal("1;1;ERRORLOGTOP", await ReadCommand(stream));
            await Send(stream, "QoK04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000\r");
            Assert.Equal("1;1;ERRORLOGEND", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;ERRORLOG+1", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;ERRORLOG-1", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;ERRLOG2=TOP;0;006000000", await ReadCommand(stream));
            await Send(stream, "QoK10-08-19;18:49:52;600;Emergency;1;MAIN;100;006000000\r");
            Assert.Equal("1;1;ERRLOG2=+1;3;006000000", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;ERRLOG2=END;0;006000000", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;ERRLOGCLR0", await ReadCommand(stream));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetErrorMessage(-1));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.GetErrorHistory((LocationType)0));
        Assert.Throws<ArgumentException>(() => robot.GetFilteredErrorHistory(LocationType.Top, "12345"));
        Assert.Throws<ArgumentException>(() => robot.GetFilteredErrorHistory(LocationType.Top, "123456;STOP"));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetFilteredErrorHistory(LocationType.Top, "123456", ErrorSeverity.Unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.ClearErrorLog(ErrorSeverity.Unknown));
        Assert.Equal("The program was not found", robot.GetErrorMessage(4140));
        var history = robot.GetErrorHistory(LocationType.Top);
        Assert.NotNull(history);
        Assert.Equal("04-02-09", history.Timestamp.Date);
        Assert.Equal("18:49:52", history.Timestamp.Time);
        Assert.Equal(4140, history.ErrorNumber);
        Assert.Equal("The program was not found", history.Contents);
        Assert.Equal(ErrorSeverity.Low, history.Severity);
        Assert.Equal("S1", history.ProgramName);
        Assert.Equal(0, history.LineNumber);
        Assert.Equal("414000000", history.DetailedErrorNumber);
        Assert.Equal("04-02-09;18:49:52;4140;The program was not found;2;S1;0;414000000", history.ToString());
        Assert.Equal("04-02-09;18:49:52", history.Timestamp.ToString());
        Assert.Null(robot.GetErrorHistory(LocationType.Bottom));
        Assert.Null(robot.GetErrorHistory(LocationType.Next));
        Assert.Null(robot.GetErrorHistory(LocationType.Previous));
        var filtered = robot.GetFilteredErrorHistory(LocationType.Top, "006000000");
        Assert.NotNull(filtered);
        Assert.Equal("006000000", filtered.DetailedErrorNumber);
        Assert.Equal(ErrorSeverity.High, filtered.Severity);
        Assert.Null(robot.GetFilteredErrorHistory(LocationType.Next, "006000000", ErrorSeverity.Caution));
        Assert.Null(robot.GetFilteredErrorHistory(LocationType.Bottom, "006000000", ErrorSeverity.All));
        robot.ClearErrorLog(ErrorSeverity.All);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsErrorSummariesAndStartDates()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;ERRSUM1", await ReadCommand(stream));
            await Send(stream, "QoK4;006000000;H;EMG signal is input.;Cause;Check emergency stop;4\r");
            Assert.Equal("1;1;ERRSUM5", await ReadCommand(stream));
            await Send(stream, "QoK4;000000000;;;;;0\r");
            Assert.Equal("1;1;ERRSUM2=1", await ReadCommand(stream));
            await Send(stream, "QoK4;006000000;L;EMG signal is input.;Cause;Check emergency stop;4;10-08-19;18:49:52\r");
            Assert.Equal("1;1;ERRSUM2=100", await ReadCommand(stream));
            await Send(stream, "QoK4;000000000;;;;;0;;\r");
            Assert.Equal("1;1;SUMDATE0", await ReadCommand(stream));
            await Send(stream, "QoK10-08-09;20:23:46\r");
            Assert.Equal("1;1;SUMDATE1", await ReadCommand(stream));
            await Send(stream, "QoK99-12-31;23:59:59\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetErrorSummary(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.GetErrorSummaryWithLastOccurrence(101));
        var summary = robot.GetErrorSummary(1);
        Assert.Equal(4, summary.TotalCount);
        Assert.NotNull(summary.Entry);
        Assert.Equal("006000000", summary.Entry.DetailedErrorNumber);
        Assert.Equal(ErrorSeverity.High, summary.Entry.Severity);
        Assert.Equal("EMG signal is input.", summary.Entry.Contents);
        Assert.Equal("Cause", summary.Entry.Cause);
        Assert.Equal("Check emergency stop", summary.Entry.Measures);
        Assert.Equal(4, summary.Entry.Occurrences);
        Assert.Null(summary.Entry.LastOccurrence);
        Assert.Equal("4;006000000;H;EMG signal is input.;Cause;Check emergency stop;4", summary.ToString());
        Assert.Equal("006000000;H;EMG signal is input.;Cause;Check emergency stop;4", summary.Entry.ToString());
        var noEntry = robot.GetErrorSummary(5);
        Assert.Equal(4, noEntry.TotalCount);
        Assert.Null(noEntry.Entry);
        Assert.Equal("4;000000000;;;;;0", noEntry.ToString());
        var dated = robot.GetErrorSummaryWithLastOccurrence(1);
        Assert.NotNull(dated.Entry);
        Assert.Equal(ErrorSeverity.Low, dated.Entry.Severity);
        Assert.Equal("10-08-19", dated.Entry.LastOccurrence.Date);
        Assert.Equal("18:49:52", dated.Entry.LastOccurrence.Time);
        Assert.Equal("4;006000000;L;EMG signal is input.;Cause;Check emergency stop;4;10-08-19;18:49:52", dated.ToString());
        Assert.Equal("006000000;L;EMG signal is input.;Cause;Check emergency stop;4;10-08-19;18:49:52", dated.Entry.ToString());
        var noDatedEntry = robot.GetErrorSummaryWithLastOccurrence(100);
        Assert.Equal(4, noDatedEntry.TotalCount);
        Assert.Null(noDatedEntry.Entry);
        Assert.Equal("4;000000000;;;;;0;;", noDatedEntry.ToString());
        Assert.Equal("10-08-09", robot.ErrorLogStartDate.Date);
        var start = robot.ErrorSummaryStartDate;
        Assert.Equal("99-12-31", start.Date);
        Assert.Equal("23:59:59", start.Time);
        Assert.Equal("99-12-31;23:59:59", start.ToString());
        await server.Complete();
    }

    [Fact]
    public async Task RunStateAndTaskSlotToStringRetainResponseData()
    {
        const string state = "TEST.MB6;00020;100;5;aB0c0;001;1;;;;;;;;;REP;START;01;0";
        const string slot = "TEST.MB6;CYC;ALWAYS;01";
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;4;STATE", await ReadCommand(stream));
            await Send(stream, "QoK" + state + "\r");
            Assert.Equal("1;4;SLOTRD", await ReadCommand(stream));
            await Send(stream, "QoK" + slot + "\r");
        });
        using var robot = server.Connect();
        Assert.Equal(state, robot.GetState(4).ToString());
        Assert.Equal(slot, robot.SlotGet(4).ToString());
        await server.Complete();
    }

    [Fact]
    public void RobotInformationToStringExcludesResponseFraming()
    {
        const string payload = "3F;3F;7,0;3,5,A,1E,32,46,64;MB6;PRM;RV-7FR-D;CR8xx-D;MELFA;17-11-29;Ver.A1dC;ENG;COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;5;8;";
        Assert.Equal(payload, new MelfaRobot.RobotInformation(payload).ToString());
        Assert.Equal(payload, new MelfaRobot.RobotInformation("Qok" + payload + "\r\n").ToString());
    }

    [Fact]
    public void DefaultProtocolStructsHaveNoResponseToFormat()
    {
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.Breakpoint).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.CalibrationInfo).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.DetailedState).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.HandState).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.RobotInformation).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.RunState).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.StopSignalState).ToString());
        Assert.Throws<InvalidOperationException>(() => default(MelfaRobot.TaskSlotInfo).ToString());
    }

    [Theory]
    [InlineData("ERRORLOG", "04-02-09;18:49:52;4140;message;9;S1;0;414000000")]
    [InlineData("ERRORLOG", "04-02-30;18:49:52;4140;message;2;S1;0;414000000")]
    [InlineData("ERRORLOG", "04-02-09;18:49:52;4140;message;2;S1;0")]
    [InlineData("ERRORLOG", "04-02-09;18:49:52;4140;message;2;S1;0;invalid")]
    [InlineData("ERRSUM", "4;006000000;X;message;cause;measures;4")]
    [InlineData("ERRSUM", "4;000000000;H;message;cause;measures;1")]
    [InlineData("ERRSUM", "4;6000000;H;message;cause;measures;4")]
    [InlineData("ERRSUM2", "4;006000000;C;message;cause;measures;4;;")]
    [InlineData("SUMDATE", "10-08-09;25:00:00")]
    public async Task RejectsMalformedErrorLogData(string command, string response)
    {
        using var server = new Controller(async stream =>
        {
            var expected = command switch
            {
                "ERRORLOG" => "ERRORLOGTOP",
                "ERRSUM" => "ERRSUM1",
                "ERRSUM2" => "ERRSUM2=1",
                _ => "SUMDATE0",
            };
            Assert.Equal("1;1;" + expected, await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() =>
        {
            switch (command)
            {
                case "ERRORLOG": _ = robot.GetErrorHistory(LocationType.Top); break;
                case "ERRSUM": _ = robot.GetErrorSummary(1); break;
                case "ERRSUM2": _ = robot.GetErrorSummaryWithLastOccurrence(1); break;
                case "SUMDATE": _ = robot.ErrorLogStartDate; break;
                default: throw new InvalidOperationException();
            }
        });
        await server.Complete();
    }

    [Fact]
    public async Task ReadsServoMonitorData()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;SRVENC", await ReadCommand(stream));
            await Send(stream, "QoK-4475890;1;2;-4487192;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23;24;25;26;27;28;29;30;31\r");
            Assert.Equal("1;1;SRVDRP", await ReadCommand(stream));
            await Send(stream, "QoK16860;1525531;22463;-1686967;4;5;6;7;8;9;10;11;12;13;14;15\r");
            Assert.Equal("1;1;SRVSPD", await ReadCommand(stream));
            await Send(stream, "QoK16;3755;68;29;-4148;115;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23\r");
            Assert.Equal("1;1;SRVCUR0", await ReadCommand(stream));
            await Send(stream, "QoK-0.42;1.25;-2.50;3.75;4;5;6;7;8;9;10;11;12;13;14;15;16;17;18;19;20;21;22;23;24;25;26;27;28;29;30;31\r");
            Assert.Equal("1;1;SRVCUR1", await ReadCommand(stream));
            await Send(stream, "QoK-0.42;0.49;3.30;0.80;1.73;2.63;-0.57;0.67;0.83;0.24;0.24;0.86;0.13;0.39;0.87;-0.24;0.24;0.93;0.00;0.00;0.00;0.00;0.00;0.00\r");
            Assert.Equal("1;1;SRVLCR", await ReadCommand(stream));
            await Send(stream, "QoK3.00;3.50;35.00;35.50;30.00;30.50;14.00;14.50;32.00;32.50;8.00;8.50;0.00;0.50;1.00;1.50\r");
            Assert.Equal("1;1;SRVVOL", await ReadCommand(stream));
            await Send(stream, "QoK200.5;210.75;190.25;1;2;3;4;5;6;7;8\r");
        });
        using var robot = server.Connect();
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var encoder = robot.ServoEncoder;
            Assert.Equal(8, encoder.Length);
            Assert.Equal((-4475890L, 1L, 2L, -4487192L), encoder[0]);
            Assert.Equal((28L, 29L, 30L, 31L), encoder[7]);
            var droop = robot.ServoDroop;
            Assert.Equal(8, droop.Length);
            Assert.Equal((22463L, -1686967L), droop[1]);
            Assert.Equal((14L, 15L), droop[7]);
            var speed = robot.ServoSpeed;
            Assert.Equal(8, speed.Length);
            Assert.Equal((29L, -4148L, 115L), speed[1]);
            Assert.Equal((21L, 22L, 23L), speed[7]);
            var commands = robot.ServoCurrentCommands;
            Assert.Equal(8, commands.Length);
            Assert.Equal((-0.42, 1.25, -2.50, 3.75), commands[0]);
            Assert.Equal((28d, 29d, 30d, 31d), commands[7]);
            var feedback = robot.ServoCurrentFeedback;
            Assert.Equal(8, feedback.Length);
            Assert.Equal((-0.42, 0.49, 3.30), feedback[0]);
            Assert.Equal((0d, 0d, 0d), feedback[7]);
            var load = robot.ServoLoadCurrent;
            Assert.Equal(8, load.Length);
            Assert.Equal((3d, 3.5), load[0]);
            Assert.Equal((1d, 1.5), load[7]);
            var voltage = robot.ServoVoltage;
            Assert.Equal(200.5, voltage.PowerVoltage);
            Assert.Equal(210.75, voltage.MaximumPowerVoltage);
            Assert.Equal(190.25, voltage.MinimumPowerVoltage);
            Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6, 7, 8 }, voltage.RegenerationLevels);
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        await server.Complete();
    }

    [Theory]
    [InlineData("SRVENC", "0;0")]
    [InlineData("SRVDRP", "0;0")]
    [InlineData("SRVSPD", "0;0")]
    [InlineData("SRVCUR0", "0;0")]
    [InlineData("SRVCUR1", "0;0")]
    [InlineData("SRVLCR", "0;0")]
    [InlineData("SRVVOL", "0;0")]
    [InlineData("SRVDRP", "1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;invalid")]
    [InlineData("SRVCUR1", "NaN;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0")]
    [InlineData("SRVVOL", "1;2;3;0;0;0;0;0;0;0;Infinity")]
    public async Task RejectsMalformedServoMonitorData(string command, string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal($"1;1;{command}", await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() =>
        {
            switch (command)
            {
                case "SRVENC": _ = robot.ServoEncoder; break;
                case "SRVDRP": _ = robot.ServoDroop; break;
                case "SRVSPD": _ = robot.ServoSpeed; break;
                case "SRVCUR0": _ = robot.ServoCurrentCommands; break;
                case "SRVCUR1": _ = robot.ServoCurrentFeedback; break;
                case "SRVLCR": _ = robot.ServoLoadCurrent; break;
                case "SRVVOL": _ = robot.ServoVoltage; break;
                default: throw new InvalidOperationException();
            }
        });
        await server.Complete();
    }

    [Fact]
    public async Task ServoMonitorReadSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;SRVENC", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => _ = robot.ServoEncoder);
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsHandStatus()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;HNDSTS", await ReadCommand(stream));
            await Send(stream, "QoK900;1;0;902;2;1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1;-1\r");
        });
        using var robot = server.Connect();
        var hands = robot.HandStates;
        Assert.Equal(8, hands.Length);
        Assert.Equal(900, hands[0].SignalNumber);
        Assert.True(hands[0].State);
        Assert.Equal(HandType.Single, hands[0].HandType);
        Assert.Equal("900;1;0", hands[0].ToString());
        Assert.Equal(902, hands[1].SignalNumber);
        Assert.False(hands[1].State);
        Assert.Equal(HandType.Double, hands[1].HandType);
        Assert.Equal(-1, hands[2].SignalNumber);
        Assert.Null(hands[2].State);
        Assert.Equal(HandType.NotUsed, hands[2].HandType);
        Assert.Equal("-1;-1;-1", hands[2].ToString());
        await server.Complete();
    }

    [Fact]
    public async Task FormatsDirectMotionCommand()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXECMOV P1", await ReadCommand(stream));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        robot.Execute("MOV P1");
        await server.Complete();
    }

    [Fact]
    public async Task FormatsBlockingMotionCommandAndWaitsForResponse()
    {
        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXEC2=MOV P1", await ReadCommand(stream));
            received.SetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        var command = Task.Run(() => robot.ExecuteBlocking("MOV P1"));
        try
        {
            await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(command.IsCompleted);
        }
        finally { release.TrySetResult(true); }
        await command.WaitAsync(TimeSpan.FromSeconds(5));
        await server.Complete();
    }

    [Fact]
    public async Task BlockingMotionCommandSurfacesControllerError()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;EXEC2=MOV P1", await ReadCommand(stream));
            await Send(stream, "QeR1234\r");
        });
        using var robot = server.Connect();
        var error = Assert.Throws<MelfaRobotException>(() => robot.ExecuteBlocking("MOV P1"));
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsCurrentJointPosition()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;JPOSF", await ReadCommand(stream));
            await Send(stream, "QoKJ1;+1.25;J2;-2.50;J3;3;J4;.5;J5;5.0;J6;6.0;;****,****;100;0.00;00000000\r");
        });
        using var robot = server.Connect();
        var position = robot.CurrentPositionJ;
        Assert.Equal(1.25, position.J1);
        Assert.Equal(-2.50, position.J2);
        Assert.Equal(6.0, position.J6);
        Assert.True(double.IsNaN(position.J7));
        Assert.True(double.IsNaN(position.J8));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsCurrentCartesianPosition()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PPOSF", await ReadCommand(stream));
            await Send(stream, "QoKX;290.62;Y;-0.09;Z;11.26;A;-179.94;B;-0.26;C;179.93;L1;0.00;;6,0;100;0.00;00000000\r");
        });
        using var robot = server.Connect();
        var position = robot.CurrentPositionP;
        Assert.Equal(290.62, position.X);
        Assert.Equal(-0.09, position.Y);
        Assert.Equal(11.26, position.Z);
        await server.Complete();
    }

    [Theory]
    [InlineData(LocationType.Top, "TOP")]
    [InlineData(LocationType.Bottom, "END")]
    [InlineData(LocationType.Next, "+1")]
    [InlineData(LocationType.Previous, "-1")]
    public async Task FormatsProgramLocations(LocationType location, string commandLocation)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal($"1;1;LISTI{commandLocation}", await ReadCommand(stream));
            await Send(stream, "QoK10 END\r");
            Assert.Equal($"1;1;LISTP{commandLocation}", await ReadCommand(stream));
            await Send(stream, "QoKP1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ProgramGetLine((LocationType)0));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ProgramGetPosition((LocationType)0));
        Assert.Equal("10 END", robot.ProgramGetLine(location));
        Assert.Equal(400.13, Assert.IsType<PositionP>(robot.ProgramGetPosition(location)).X);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsNamedVariableAndRejectsMismatchedResponse()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ1=(0,0,0,0,0,0,0,0)\r");
            Assert.Equal("1;1;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ2=(0,0,0,0,0,0,0,0)\r");
            Assert.Equal("1;9;VALJ1;J", await ReadCommand(stream));
            await Send(stream, "QoKJ1=(1,0,0,0,0,0,0,0)\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.ReadVariable("J1;STOP", VariableClass.JointPosition));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ReadVariable("J1", (VariableClass)0));
        Assert.Equal("(0,0,0,0,0,0,0,0)", robot.ReadVariable("J1", VariableClass.JointPosition));
        Assert.Throws<InvalidDataException>(() => robot.ReadVariable("J1", VariableClass.JointPosition));
        Assert.Equal("(1,0,0,0,0,0,0,0)", robot.ReadVariable("J1", VariableClass.JointPosition, 9));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsGlobalVariablesByNameAndPosition()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;9;GVALM_01;M", await ReadCommand(stream));
            await Send(stream, "QoKM_01=+3\r");
            Assert.Equal("1;1;GVAL<;C", await ReadCommand(stream));
            await Send(stream, "QoKC_01=\"a=b;c\"\r");
            Assert.Equal("1;1;GVAL>;J", await ReadCommand(stream));
            await Send(stream, "QoKJ_01=(1,2)\r");
            Assert.Equal("1;1;GVAL+1;P", await ReadCommand(stream));
            await Send(stream, "QoKP_01=(1,2)\r");
            Assert.Equal("1;1;GVAL-1;M", await ReadCommand(stream));
            await Send(stream, "QoKM_02=2\r");
            Assert.Equal("1;1;GVALM_01;M", await ReadCommand(stream));
            await Send(stream, "QoKM_02=2\r");
            Assert.Equal("1;1;GVAL<;M", await ReadCommand(stream));
            await Send(stream, "QoKbad name=2\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.ReadGlobalVariable("M_01;STOP", VariableClass.Integer));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ReadGlobalVariable("M_01", (VariableClass)0));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ReadGlobalVariable(LocationType.Top, (VariableClass)0));
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.ReadGlobalVariable((LocationType)0, VariableClass.Integer));
        Assert.Equal("+3", robot.ReadGlobalVariable("M_01", VariableClass.Integer, 9));
        var first = robot.ReadGlobalVariable(LocationType.Top, VariableClass.Character);
        Assert.Equal("C_01", first.Key);
        Assert.Equal("\"a=b;c\"", first.Value);
        Assert.Equal("J_01", robot.ReadGlobalVariable(LocationType.Bottom, VariableClass.JointPosition).Key);
        Assert.Equal("P_01", robot.ReadGlobalVariable(LocationType.Next, VariableClass.CartesianPosition).Key);
        Assert.Equal("M_02", robot.ReadGlobalVariable(LocationType.Previous, VariableClass.Integer).Key);
        Assert.Throws<InvalidDataException>(() => robot.ReadGlobalVariable("M_01", VariableClass.Integer));
        Assert.Throws<InvalidDataException>(() => robot.ReadGlobalVariable(LocationType.Top, VariableClass.Integer));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsVariablePagesAndContinuation()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;9;VALS<;M", await ReadCommand(stream));
            await Send(stream, "QoKM1=+1;\vM2=+2;\vM3=+3;3;1\r");
            Assert.Equal("1;9;VALS+1;M", await ReadCommand(stream));
            await Send(stream, "QoKM4=+4\vM5=+5;2;0\r");
            Assert.Equal("1;1;GVALS>;C", await ReadCommand(stream));
            await Send(stream, "QoKC_01=\"a;=b\"\vC_02=\"\";2;1\r");
            Assert.Equal("1;1;GVALS-1;C", await ReadCommand(stream));
            await Send(stream, "QoK;0;0\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentOutOfRangeException>(() => robot.ReadVariables((LocationType)0, VariableClass.Integer));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ReadVariables(LocationType.Top, (VariableClass)0));
        Assert.Throws<InvalidEnumArgumentException>(() => robot.ReadGlobalVariables(LocationType.Top, (VariableClass)0));
        var first = robot.ReadVariables(LocationType.Top, VariableClass.Integer, 9);
        Assert.True(first.HasMore);
        Assert.Equal(new[] { "M1", "M2", "M3" }, first.Variables.Select(v => v.Key));
        Assert.Equal(new[] { "+1", "+2", "+3" }, first.Variables.Select(v => v.Value));
        Assert.Equal("M1=+1;\vM2=+2;\vM3=+3;3;1", first.ToString());
        var next = robot.ReadVariables(LocationType.Next, VariableClass.Integer, 9);
        Assert.False(next.HasMore);
        Assert.Equal(new[] { "M4", "M5" }, next.Variables.Select(v => v.Key));
        Assert.Equal("M4=+4\vM5=+5;2;0", next.ToString());
        var globals = robot.ReadGlobalVariables(LocationType.Bottom, VariableClass.Character);
        Assert.True(globals.HasMore);
        Assert.Equal("\"a;=b\"", globals.Variables[0].Value);
        Assert.Equal("\"\"", globals.Variables[1].Value);
        var empty = robot.ReadGlobalVariables(LocationType.Previous, VariableClass.Character);
        Assert.False(empty.HasMore);
        Assert.Empty(empty.Variables);
        Assert.Equal(";0;0", empty.ToString());
        await server.Complete();
    }

    [Theory]
    [InlineData("M1=1;2;0")]
    [InlineData("M1=1;1;2")]
    [InlineData("M1=1;-1;0")]
    [InlineData("M1=1;one;0")]
    [InlineData("M1=1;1")]
    [InlineData("M1=1\v;2;0")]
    [InlineData("invalid name=1;1;0")]
    public async Task RejectsMalformedVariablePages(string response)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;VALS<;M", await ReadCommand(stream));
            await Send(stream, "QoK" + response + "\r");
        });
        using var robot = server.Connect();
        Assert.Throws<InvalidDataException>(() => robot.ReadVariables(LocationType.Top, VariableClass.Integer));
        await server.Complete();
    }

    [Fact]
    public async Task ReadsUnterminatedControllerResponses()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;OPEN=TEST", await ReadCommand(stream));
            await Send(stream, "QoK3F;3F;7,0;3,5,A,1E,32,46,64;MB6;PRM;RV-7FR-D;CR8xx-D;MELFA;17-11-29;Ver.A1dC;ENG;COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;5;8;");
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "QoKTEST.MB6");
            Assert.Equal("1;1;CLOSE", await ReadCommand(stream));
            await Send(stream, "QoK");
        });
        using var robot = server.Connect(requireTerminator: false);
        Assert.False(robot.RequireResponseTerminator);
        var info = robot.Open("TEST");
        Assert.Equal("RV-7FR-D", info.RobotType);
        Assert.Equal("MB6", info.ProgramExtension);
        Assert.Equal(8, info.MaximumTaskCount);
        Assert.Equal("3F;3F;7,0;3,5,A,1E,32,46,64;MB6;PRM;RV-7FR-D;CR8xx-D;MELFA;17-11-29;Ver.A1dC;ENG;COPYRIGHT(C)2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;5;8;", info.ToString());
        Assert.Equal("TEST.MB6", robot.GetCurrentProgram());
        robot.Close();
        await server.Complete();
    }

    [Fact]
    public async Task ShortReadModeWaitsForCompleteResponsePrefixAndErrorNumber()
    {
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            await Send(stream, "Q");
            await Task.Delay(30);
            await Send(stream, "eR12");
            await Task.Delay(30);
            await Send(stream, "34");
        });
        using var robot = server.Connect(requireTerminator: false);
        var error = Assert.Throws<MelfaRobotException>(() => robot.GetCurrentProgram());
        Assert.Equal(1234, error.ErrorNumber);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsFragmentedResponsesLargerThanTheOldBuffer()
    {
        var payload = new string('A', 1000);
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "Qo");
            await Task.Delay(30);
            await Send(stream, "K" + payload[..500]);
            await Task.Delay(30);
            await Send(stream, payload[500..] + "\r");
        });
        using var robot = server.Connect();
        Assert.Equal(payload, robot.GetCurrentProgram());
        await server.Complete();
    }

    [Fact]
    public async Task TimeoutInvalidatesConnection()
    {
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            await Send(stream, "QoKpartial");
            await Task.Delay(500);
        });
        using var robot = server.Connect();
        robot.CommandTimeout = 100;
        Assert.ThrowsAny<IOException>(() => robot.GetCurrentProgram());
        Assert.False(robot.IsConnected);
        await server.Complete();
    }

    [Fact]
    public async Task RejectsInvalidCommandsWithoutSendingOrDisconnecting()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PRGRD", await ReadCommand(stream));
            await Send(stream, "QoKREADY\r");
        });
        using var robot = server.Connect();
        Assert.Throws<ArgumentException>(() => robot.Execute("MOV P1\rSTOP"));
        Assert.Throws<ArgumentException>(() => robot.Execute("MOV P1\nSTOP"));
        Assert.Throws<EncoderFallbackException>(() => robot.Execute("\u00e9"));
        Assert.Throws<InvalidOperationException>(() => robot.Execute(new string('A', 255)));
        Assert.Equal("READY", robot.GetCurrentProgram());
        await server.Complete();
    }

    [Fact]
    public async Task DisconnectInterruptsPendingRead()
    {
        var requestReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var server = new Controller(async stream =>
        {
            await ReadCommand(stream);
            requestReceived.SetResult(true);
            await Task.Delay(500);
        });
        using var robot = server.Connect();
        var pending = Task.Run(() => Record.Exception(() => robot.GetCurrentProgram()));
        await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        robot.Disconnect();
        var exception = await pending.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.NotNull(exception);
        Assert.False(robot.IsConnected);
        await server.Complete();
    }

    [Fact]
    public async Task FormatsCommandsAndReadsStartEligibility()
    {
        string[] commands =
        {
            "1;1;EMDAT10 MOV P1\u000b20 MOV P2",
            "1;1;JOG01;00;01;02;00",
            "1;1;BRKPTSET=TEST;20;1",
            "1;1;FOPENTEST.MB5;w",
            "1;1;FWRITE2;0102",
            "1;1;FCLOSE",
            "1;1;ATENA",
            "1;1;ATENA",
            "1;1;LISTPP1"
        };
        using var server = new Controller(async stream =>
        {
            for (var i = 0; i < commands.Length; i++)
            {
                Assert.Equal(commands[i], await ReadCommand(stream));
                var response = i switch
                {
                    6 => "QoK0\r",
                    7 => "QoK1\r",
                    8 => "QoKP1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)\r",
                    _ => "QoK\r"
                };
                await Send(stream, response);
            }
        });
        using var robot = server.Connect();
        robot.ProgramEdit(10, "MOV P1\r\n20 MOV P2");
        robot.Jog(JogMode.XYZ, 1, 2);
        robot.SetBreakpoint("TEST", 20, BreakpointType.OneTime);
        using var data = new MemoryStream(new byte[] { 1, 2 });
        robot.FileWrite("TEST.MB5", data);
        Assert.True(robot.ProgramStart);
        Assert.False(robot.ProgramStart);
        Assert.Equal(400.13, Assert.IsType<PositionP>(robot.ProgramGetPosition("P1")).X);
        await server.Complete();
    }

    [Fact]
    public async Task ReadsPreparedProgramFileInBlocks()
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;FOPENTEST.MB6;r", await ReadCommand(stream));
            await Send(stream, "QoK\r");
            Assert.Equal("1;1;FREAD", await ReadCommand(stream));
            await Send(stream, "QoK524F42;1\r");
            Assert.Equal("1;1;FREAD", await ReadCommand(stream));
            await Send(stream, "QoK505247;0\r");
            Assert.Equal("1;1;FCLOSE", await ReadCommand(stream));
            await Send(stream, "QoK\r");
        });
        using var robot = server.Connect();
        using var contents = robot.FileRead("TEST.MB6");
        using var buffer = new MemoryStream();
        contents.CopyTo(buffer);
        Assert.Equal(Encoding.ASCII.GetBytes("ROBPRG"), buffer.ToArray());
        await server.Complete();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListsAllParameterPages(bool multiplePages)
    {
        using var server = new Controller(async stream =>
        {
            Assert.Equal("1;1;PMR=0", await ReadCommand(stream));
            await Send(stream, multiplePages ? "QoK1;1;FIRST\r" : "QoK0;1;ONLY\r");
            if (multiplePages)
            {
                Assert.Equal("1;1;PMR=1", await ReadCommand(stream));
                await Send(stream, "QoK0;1;LAST\r");
            }
        });
        using var robot = server.Connect();
        Assert.Equal(multiplePages ? new[] { "FIRST", "LAST" } : new[] { "ONLY" }, robot.ListChangedParameters().ToArray());
        await server.Complete();
    }

    [Fact]
    public async Task FileTransfersCannotOpenAnotherFileWhileAStreamIsBeingRead()
    {
        var firstRead = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new ManualResetEventSlim();
        var openedSecond = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<string>();
        using var server = new Controller(async stream =>
        {
            for (var i = 0; i < 6; i++)
            {
                var command = await ReadCommand(stream);
                commands.Enqueue(command);
                if (command == "1;1;FOPENSECOND;w")
                    openedSecond.TrySetResult(true);
                await Send(stream, "QoK\r");
            }
        });
        using var robot = server.Connect();
        using var first = new BlockingStream(firstRead, release);
        using var second = new MemoryStream(new byte[] { 2 });
        var writeFirst = Task.Run(() => robot.FileWrite("FIRST", first));
        try
        {
            await firstRead.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var writeSecond = Task.Run(() => robot.FileWrite("SECOND", second));
            Assert.NotSame(openedSecond.Task, await Task.WhenAny(openedSecond.Task, Task.Delay(200)));
            release.Set();
            await Task.WhenAll(writeFirst, writeSecond).WaitAsync(TimeSpan.FromSeconds(5));
            await server.Complete();
            Assert.Equal(new[]
            {
                "1;1;FOPENFIRST;w", "1;1;FWRITE1;01", "1;1;FCLOSE",
                "1;1;FOPENSECOND;w", "1;1;FWRITE1;02", "1;1;FCLOSE"
            }, commands.ToArray());
        }
        finally { release.Set(); }
    }

    private static async Task<string> ReadCommand(NetworkStream stream)
    {
        using var bytes = new MemoryStream();
        var buffer = new byte[1];
        while (await stream.ReadAsync(buffer) != 0)
        {
            if (buffer[0] == '\r')
                return Encoding.ASCII.GetString(bytes.ToArray());
            bytes.WriteByte(buffer[0]);
        }
        throw new EndOfStreamException("Command was not terminated.");
    }

    private static async Task Send(NetworkStream stream, string response) =>
        await stream.WriteAsync(Encoding.ASCII.GetBytes(response));

    private sealed class Controller : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly Task _server;
        private TcpClient? _client;

        public Controller(Func<NetworkStream, Task> script)
        {
            _listener.Start();
            _server = Task.Run(async () =>
            {
                using var client = await _listener.AcceptTcpClientAsync();
                _client = client;
                await script(client.GetStream());
            });
        }

        public MelfaRobot Connect(bool requireTerminator = true)
        {
            var robot = new MelfaRobot("127.0.0.1", ((IPEndPoint)_listener.LocalEndpoint).Port);
            if (requireTerminator)
                robot.RequireResponseTerminator = true;
            robot.Connect();
            return robot;
        }

        public Task Complete() => _server.WaitAsync(TimeSpan.FromSeconds(5));

        public void Dispose()
        {
            _client?.Dispose();
            _listener.Stop();
        }
    }

    private sealed class BlockingStream : MemoryStream
    {
        private readonly TaskCompletionSource<bool> _entered;
        private readonly ManualResetEventSlim _release;

        public BlockingStream(TaskCompletionSource<bool> entered, ManualResetEventSlim release) : base(new byte[] { 1 })
        {
            _entered = entered;
            _release = release;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _entered.TrySetResult(true);
            if (!_release.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("Timed out waiting to release the input stream.");
            return base.Read(buffer, offset, count);
        }
    }
}
