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
