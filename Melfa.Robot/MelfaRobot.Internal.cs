using Melfa.Robot.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    private readonly object _syncRoot = new();
    private readonly object _connectionSyncRoot = new();
    private readonly object _fileSyncRoot = new();
    private int _connectionVersion;
    private int _commandTimeout = 5000;
    private bool _requireResponseTerminator;
    private TcpClient _Client = null;

    private static CalibrationInfo ParseCalibration(string response)
    {
        if (response.Length < 3 || (response[0] != 'N' && response[0] != 'F') ||
            !byte.TryParse(response.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var axes))
            throw new InvalidDataException($"Unexpected CALIB response: {response}");
        return new CalibrationInfo(response[0] == 'N', axes, response.Substring(3));
    }

    private static DetailedState ParseDetailedState(string response, int slot)
    {
        var fields = response.Split(';');
        if (fields.Length != 3 || fields[0].Length < 5 ||
            !byte.TryParse(fields[0].Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var run) ||
            !byte.TryParse(fields[0].Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var stop) ||
            !int.TryParse(fields[0].Substring(4), NumberStyles.None, CultureInfo.InvariantCulture, out var error) ||
            !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var step) ||
            !int.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out var mechanism))
            throw new InvalidDataException($"Unexpected DSTATE response: {response}");
        return new DetailedState(slot, (RunStatus)run, (StopStatus)stop, error, step, mechanism);
    }

    private static StopSignalState ParseStopSignals(string response)
    {
        var fields = response.Split(';');
        if (fields.Length != 2 ||
            !uint.TryParse(fields[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var stop) ||
            !uint.TryParse(fields[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var emergencyStop))
            throw new InvalidDataException($"Unexpected STPSIG response: {response}");
        return new StopSignalState((StopSignalSource)stop, (EmergencyStopSource)emergencyStop);
    }

    private static int ErrorHistoryFilterLevel(ErrorSeverity? severity) => severity switch
    {
        null => 0,
        ErrorSeverity.All => 0,
        ErrorSeverity.High => 1,
        ErrorSeverity.Low => 2,
        ErrorSeverity.Caution => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    };

    private static ErrorSeverity ParseErrorSeverity(string value) => value switch
    {
        "1" or "H" => ErrorSeverity.High,
        "2" or "L" => ErrorSeverity.Low,
        "3" or "C" => ErrorSeverity.Caution,
        _ => throw new InvalidDataException($"Invalid error severity: {value}"),
    };

    private static int ParseErrorCount(string value)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
            throw new InvalidDataException($"Invalid error count or number: {value}");
        return result;
    }

    private static bool IsValidControllerTimestamp(string date, string time) =>
        DateTime.TryParseExact($"{date} {time}", "yy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static ErrorTimestamp ParseErrorTimestamp(string date, string time)
    {
        if (!IsValidControllerTimestamp(date, time))
            throw new InvalidDataException($"Invalid controller date and time: {date};{time}");
        return new ErrorTimestamp(date, time);
    }

    private (string Date, string Time) ReadControllerClock()
    {
        var result = DoCommandInternal("TIME");
        if (result.Length != 16)
            throw new InvalidDataException($"Unexpected controller clock response: {result}");
        var date = result.Substring(0, 8);
        var time = result.Substring(8, 8);
        if (!IsValidControllerTimestamp(date, time))
            throw new InvalidDataException($"Unexpected controller clock response: {result}");
        return (date, time);
    }

    private static long ParseTimeCounter(string value)
    {
        if (!long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
            throw new InvalidDataException($"Invalid time counter: {value}");
        return result;
    }

    private HourMeterData ReadHourMeter()
    {
        var response = DoCommandInternal("PTIME");
        var fields = response.Split(';');
        if (fields.Length != 22)
            throw new InvalidDataException($"Unexpected PTIME field count: {fields.Length}; expected 22.");
        var values = new long[fields.Length];
        for (var i = 0; i < fields.Length; i++)
            values[i] = ParseTimeCounter(fields[i]);
        return new HourMeterData(values[0], values[1], ImmutableArray.Create(values, 2, 3),
            ImmutableArray.Create(values, 5, 12), values[17], values[18], values[19], values[20], values[21]);
    }

    private CycleTimeData ReadCycleTime(string program)
    {
        var response = DoCommandInternal($"CYCLETIME{program}");
        var fields = response.Split(';');
        if (fields.Length != 4)
            throw new InvalidDataException($"Unexpected CYCLETIME response: {response}");
        return new CycleTimeData(ParseTimeCounter(fields[0]), ParseTimeCounter(fields[1]),
            ParseTimeCounter(fields[2]), ParseTimeCounter(fields[3]));
    }

    private static ErrorTimestamp ParseErrorTimestampResponse(string result)
    {
        var fields = result.Split(';');
        if (fields.Length != 2)
            throw new InvalidDataException($"Unexpected error timestamp response: {result}");
        return ParseErrorTimestamp(fields[0], fields[1]);
    }

    private static ErrorHistoryEntry ParseErrorHistory(string result)
    {
        if (result.Length == 0)
            return null;
        var fields = result.Split(';');
        if (fields.Length != 8 || !Regex.IsMatch(fields[7], @"\A[0-9]{1,9}\z"))
            throw new InvalidDataException($"Unexpected error history response: {result}");
        return new ErrorHistoryEntry(ParseErrorTimestamp(fields[0], fields[1]), ParseErrorCount(fields[2]),
            fields[3], ParseErrorSeverity(fields[4]), fields[5], ParseErrorCount(fields[6]), fields[7]);
    }

    private ErrorSummary ReadErrorSummary(int index, bool includeLastOccurrence)
    {
        if (index < 1 || index > 100)
            throw new ArgumentOutOfRangeException(nameof(index));
        var result = DoCommandInternal(includeLastOccurrence ? $"ERRSUM2={index}" : $"ERRSUM{index}");
        var fields = result.Split(';');
        if (fields.Length != (includeLastOccurrence ? 9 : 7) ||
            !Regex.IsMatch(fields[1], @"\A[0-9]{9}\z"))
            throw new InvalidDataException($"Unexpected error summary response: {result}");
        var totalCount = ParseErrorCount(fields[0]);
        var occurrences = ParseErrorCount(fields[6]);
        if (fields[1] == "000000000")
        {
            if (fields[2] != "" || fields[3] != "" || fields[4] != "" || fields[5] != "" ||
                occurrences != 0 || (includeLastOccurrence && (fields[7] != "" || fields[8] != "")))
                throw new InvalidDataException($"Unexpected empty error summary response: {result}");
            return new ErrorSummary(totalCount, null);
        }
        var timestamp = includeLastOccurrence ? ParseErrorTimestamp(fields[7], fields[8]) : null;
        return new ErrorSummary(totalCount, new ErrorSummaryEntry(fields[1], ParseErrorSeverity(fields[2]),
            fields[3], fields[4], fields[5], occurrences, timestamp));
    }

    private string[] ReadServoFields(string command, int count)
    {
        var fields = DoCommandInternal(command).Split(';');
        if (fields.Length != count)
            throw new InvalidDataException($"Unexpected {command} field count: {fields.Length}; expected {count}.");
        return fields;
    }

    private ImmutableArray<T> ReadServoAxes<T>(string command, int fieldsPerAxis, Func<string[], int, T> parse)
    {
        var fields = ReadServoFields(command, 8 * fieldsPerAxis);
        var axes = ImmutableArray.CreateBuilder<T>(8);
        for (var i = 0; i < fields.Length; i += fieldsPerAxis)
            axes.Add(parse(fields, i));
        return axes.ToImmutable();
    }

    private static long ParseServoInteger(string value)
    {
        if (!long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
            throw new InvalidDataException($"Invalid servo integer: {value}");
        return result;
    }

    private static double ParseServoNumber(string value)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ||
            double.IsNaN(result) || double.IsInfinity(result))
            throw new InvalidDataException($"Invalid servo number: {value}");
        return result;
    }

    private void WithOpenFile(string filename, string mode, Action action)
    {
        lock (_fileSyncRoot)
        {
            DoCommandInternal($"FOPEN{filename};{mode}");
            try
            {
                action();
            }
            catch (Exception operationError)
            {
                try { DoCommandInternal("FCLOSE"); }
                catch (Exception closeError)
                {
                    throw new AggregateException("File operation and close both failed.", operationError, closeError);
                }
                throw;
            }
            DoCommandInternal("FCLOSE");
        }
    }

    private VariablePage ReadVariablePage(string command, LocationType position, VariableClass variableClass, int slot)
    {
        var result = DoCommandInternal($"{command}{position.ToVariablePositionCommandString()};{variableClass.ToCommandString()}", slot);
        var flagSeparator = result.LastIndexOf(';');
        var countSeparator = flagSeparator > 0 ? result.LastIndexOf(';', flagSeparator - 1) : -1;
        if (countSeparator < 0 ||
            !int.TryParse(result.Substring(countSeparator + 1, flagSeparator - countSeparator - 1),
                NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
            (result.Substring(flagSeparator + 1) != "0" && result.Substring(flagSeparator + 1) != "1"))
            throw new InvalidDataException($"Unexpected variable page response: {result}");

        var entries = result.Substring(0, countSeparator);
        var variables = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
        if (entries.Length != 0)
        {
            var parts = entries.Split('\x0b');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                // The manual's examples include an extra semicolon before each VT.
                if (i < parts.Length - 1 && part.EndsWith(";", StringComparison.Ordinal))
                    part = part.Substring(0, part.Length - 1);
                variables.Add(ParseVariableEntry(part));
            }
        }
        if (variables.Count != count)
            throw new InvalidDataException($"Unexpected variable count in response: {result}");
        return new VariablePage(variables.ToImmutable(), result.Substring(flagSeparator + 1) == "1");
    }

    private static KeyValuePair<string, string> ParseVariableEntry(string entry)
    {
        var separator = entry.IndexOf('=');
        if (separator <= 0 || !Regex.IsMatch(entry.Substring(0, separator), @"\A[A-Za-z][A-Za-z0-9_]*\z"))
            throw new InvalidDataException($"Unexpected variable response: {entry}");
        return new KeyValuePair<string, string>(entry.Substring(0, separator), entry.Substring(separator + 1));
    }

    private string DoCommandInternal(string command, int slot)
    {
        lock (_syncRoot)
        {
            var client = Volatile.Read(ref _Client);
            if (client is null || !client.Connected)
                throw new InvalidOperationException("Not connected.");

            if (command.IndexOf('\r') >= 0 || command.IndexOf('\n') >= 0)
                throw new ArgumentException("Command cannot contain CR or LF.", nameof(command));
            var cmd = $"{RobotNumber};{slot};{command}";
            var encoding = Encoding.GetEncoding("us-ascii", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            var wbuf = encoding.GetBytes(cmd + "\r");
            if (wbuf.Length - 1 > 255)
                throw new InvalidOperationException("Command too long (max 255 bytes).");

            var timeout = CommandTimeout;
            var requireTerminator = RequireResponseTerminator;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string ret;
            using var response = new MemoryStream();
            const int bufferSize = 1536;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                var stream = client.GetStream();
                stream.WriteTimeout = timeout;
                stream.Write(wbuf, 0, wbuf.Length);

                while (true)
                {
                    var remaining = timeout - stopwatch.ElapsedMilliseconds;
                    if (remaining <= 0)
                        throw new IOException("Timed out waiting for the robot response.");
                    stream.ReadTimeout = (int)remaining;
                    var count = stream.Read(buffer, 0, requireTerminator ? 1 : bufferSize);
                    if (count == 0)
                        throw new EndOfStreamException("The robot disconnected before completing its response.");
                    if (requireTerminator && buffer[0] == '\r')
                        break;
                    if (response.Length + count > 65536)
                        throw new InvalidDataException("The robot response exceeds 65536 bytes.");
                    response.Write(buffer, 0, count);
                    if (!requireTerminator && count < bufferSize && response.Length >= 3)
                    {
                        var bytes = response.GetBuffer();
                        var isError = (bytes[0] == 'Q' || bytes[0] == 'q') &&
                                      (bytes[1] == 'E' || bytes[1] == 'e') &&
                                      (bytes[2] == 'R' || bytes[2] == 'r');
                        if (!isError || response.Length >= 7)
                            break;
                    }
                }
                ret = encoding.GetString(response.ToArray());
                if (!requireTerminator)
                    ret = ret.TrimEnd('\r', '\n');
                if (!ret.StartsWith("QoK", StringComparison.OrdinalIgnoreCase) &&
                    !(ret.StartsWith("QeR", StringComparison.OrdinalIgnoreCase) &&
                      ret.Length >= 7 && int.TryParse(ret.Substring(3, 4), out _)))
                    throw new InvalidDataException("Invalid robot response: " + ret);
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException || ex is DecoderFallbackException)
            {
                Interlocked.CompareExchange(ref _Client, null, client);
                client.Dispose();
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
            }
            if (ret.StartsWith("QoK", StringComparison.InvariantCultureIgnoreCase))
                return ret[3..];

            throw MelfaRobotException.FromErrorResult(cmd, ret);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string DoCommandInternal(string command) => DoCommandInternal(command, 1);
}
