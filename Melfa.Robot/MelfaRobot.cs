using CommunityToolkit.Diagnostics;
using Melfa.Robot.Helpers;
using Numeral;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Melfa.Robot;

public partial class MelfaRobot : IDisposable
{
    public const int DEFAULT_PORT = 10001;

    public string Host { get; }
    public int Port { get; }
    public int RobotNumber { get; }

    /// <summary>Maximum time in milliseconds for a command exchange.</summary>
    public int CommandTimeout
    {
        get => Volatile.Read(ref _commandTimeout);
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            Volatile.Write(ref _commandTimeout, value);
        }
    }

    /// <summary>
    /// Require responses to end with CR instead of using the legacy short-read
    /// convention. TCP short reads are not reliable message boundaries.
    /// </summary>
    public bool RequireResponseTerminator
    {
        get => Volatile.Read(ref _requireResponseTerminator);
        set => Volatile.Write(ref _requireResponseTerminator, value);
    }

    public bool IsConnected
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref _Client)?.Connected ?? false;
    }

    public MelfaRobot(string host, int port = DEFAULT_PORT, int robot = 1)
    {
        Host = host;
        Port = port < 0 ? DEFAULT_PORT : port;
        RobotNumber = robot;
    }

    /// <summary>Connect to the robot</summary>
    public void Connect()
    {
        if (IsConnected)
            return;
        lock (_syncRoot)
        {
            if (IsConnected)
                return;

            var version = Volatile.Read(ref _connectionVersion);
            var client = new TcpClient();
            try
            {
                client.Connect(Host, Port);
                lock (_connectionSyncRoot)
                {
                    if (version != _connectionVersion)
                        throw new InvalidOperationException("Connection was cancelled.");
                    Volatile.Write(ref _Client, client);
                }
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }
    }

    /// <summary>Disconnect from the robot</summary>
    public void Disconnect()
    {
        TcpClient client;
        lock (_connectionSyncRoot)
        {
            Interlocked.Increment(ref _connectionVersion);
            client = Interlocked.Exchange(ref _Client, null);
        }
        client?.Dispose();
    }

    #region Communication
    /// <summary>[OPEN=] Communication open.</summary>
    /// <remarks>The commands send most first when communication from the peripheral equipment such as personal computers.</remarks>
    /// <param name="name">An arbitrary name specified in the alphanumeric character</param>
    /// <param name="lang">Specify the display language of the error message.</param>
    /// <returns>Robot information and configuration</returns>
    /// <seealso cref="Close"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RobotInformation Open(string name, string lang = null) =>
        new(DoCommandInternal(lang == null ? $"OPEN={name}" : $"OPEN={name};{lang}"));

    /// <summary>[CLOSE] Communication close.</summary>
    /// <remarks>The command sent when the communication is ended from the peripheral equipment sxuch as personal computers.</remarks>
    /// <seealso cref="Open(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close() => DoCommandInternal("CLOSE");

    /// <summary>[CNTL] Operation enable or disable.</summary>
    /// <remarks>
    ///     When the command which needs the operation right (such as <see cref="ProgramStart" />, 
    ///     <see cref="ServoOn" />, and more), the operation right should be made effective.    
    /// </remarks>
    public bool OperationEnabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal(value ? "CNTLON" : "CNTLOFF");
    }
    #endregion

    #region Program Edit
    /// <summary>[LOAD=] Open the program for edit</summary>
    /// <param name="filename">Edit program name</param>
    /// <seealso cref="ProgramSave" />
    /// <seealso cref="ProgramNew" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramOpen(string filename) => DoCommandInternal($"LOAD={filename}");

    /// <summary>[SAVE] Program save and close.</summary>
    /// <remarks>The content of the edit is preserved and the program is closed</remarks>
    /// <seealso cref="ProgramOpen(string)" />
    /// <seealso cref="ProgramNew" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramSave() => DoCommandInternal("SAVE");

    /// <summary>[NEW] Program quit and close</summary>
    /// <remarks>The program is closed annulling the content of the edit.</remarks>
    /// <seealso cref="ProgramOpen(string)" />
    /// <seealso cref="ProgramSave" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramNew() => DoCommandInternal("NEW");

    /// <summary>[EDATA, EMDAT] Edit program</summary>
    /// <remarks>The line and the position are registered in program. It is effective in the edit slot.</remarks>
    /// <param name="line">line number in the program</param>
    /// <param name="content">the content to replace</param>
    /// <seealso cref="ProgramInsert(int, string)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramEdit(int line, string content)
    {
        content = content.Trim().Replace("\r\n", "\n").Replace('\r', '\n').Replace('\n', '\x0b');
        DoCommandInternal(content.Contains('\x0b') ? $"EMDAT{line} {content}" : $"EDATA{line} {content}");
    }

    /// <summary>[EDINS] Insert program</summary>
    /// <remarks>The line is inserted in the program. It is effective in the edit slot.</remarks>
    /// <param name="line">line number in the program</param>
    /// <param name="content">the content to insert</param>
    /// <seealso cref="ProgramEdit(int, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramInsert(int line, string content) => DoCommandInternal($"EDINS={line};{content}");

    /// <summary>[VAL=] Variable data write</summary>
    /// <remarks>The value of the variable is changed. It is effective in the edit slot.</remarks>
    /// <param name="name">Variable name</param>
    /// <param name="value">Target value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramSetVariable(string name, string value) => DoCommandInternal($"VAL={name}={value}");

    /// <summary>[LISTI] Program list read</summary>
    /// <remarks>The line data is read from the program. It is effective in the edit slot.</remarks>
    /// <param name="type">The line to read</param>
    /// <returns>Content of the line</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ProgramGetLine(LocationType type) => DoCommandInternal($"LISTI{type.ToProgramLocationCommandString()}");
    /// <summary>[LISTI] Program list read</summary>
    /// <remarks>The line data is read from the program. It is effective in the edit slot.</remarks>
    /// <param name="line">specific line number</param>
    /// <returns>Conent of the line</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ProgramGetLine(int line) => DoCommandInternal($"LISTI{line}");

    // TODO: LISTL - Program more list read

    /// <summary>[LISTP] Program position read</summary>
    /// <param name="type">The position to read</param>
    /// <returns>Position data</returns>
    /// <exception cref="PositionParseException">
    ///     unable to parse the position to <see cref="PositionP"/> nor <see cref="PositionJ"/>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IPosition ProgramGetPosition(LocationType type) => ProgramGetPosition(type.ToProgramLocationCommandString());

    /// <summary>[LISTP] Program position read</summary>
    /// <param name="positionName">Specific position name</param>
    /// <returns>Position data</returns>
    /// <exception cref="PositionParseException">
    ///     unable to parse the position to <see cref="PositionP"/> nor <see cref="PositionJ"/>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IPosition ProgramGetPosition(string positionName)
    {
        var ret = DoCommandInternal($"LISTP{positionName}");
        var separator = ret.IndexOf('=');
        if (separator < 0)
            throw new PositionParseException(ret, typeof(IPosition));
        var value = ret.Substring(separator + 1);
        try { return new PositionP(value); } catch (PositionParseException) { }
        try { return new PositionJ(value); } catch (PositionParseException) { }
        throw new PositionParseException(value, typeof(IPosition));
    }

    /// <summary>[VTYPRD] Variable type read</summary>
    /// <param name="variableName">Name of the variable</param>
    /// <returns><see cref="Type"/> of the variable</returns>
    /// <exception cref="InvalidCastException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Type ProgramGetVariableType(string variableName)
    {
        var type = DoCommandInternal($"VTYPRD{variableName}");
        switch (type)
        {
            case "M": return typeof(int);
            case "C": return typeof(char);
            case "P": return typeof(PositionP);
            case "J": return typeof(PositionJ);
        }
        throw new InvalidCastException($"Unknown VariableType \"{type}\"");
    }

    /// <summary>[LISTCNT] Count program lines</summary>
    /// <param name="start">Counted start line number</param>
    /// <param name="end">Counted end line number</param>
    /// <returns>Number of lines</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ProgramCountLines(int start = 0, int end = int.MaxValue) => int.Parse(DoCommandInternal($"LISTCNT{start};{end}"));

    /// <summary>[EXEC] Direct execution</summary>
    /// <remarks>The instruction is executed directly. Returns the response when you accepted.</remarks>
    /// <param name="instruction">Instruction of MELFA-BASIC IV or MOVEMASTER commands</param>
    /// <seealso cref="ExecuteBlocking(string)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(string instruction) => DoCommandInternal($"EXEC{instruction}");

    /// <summary>[EXEC2] Direct execution (blocking)</summary>
    /// <remarks>The instruction is executed directly. Returns the response when you work completed.</remarks>
    /// <param name="instruction">Instruction of MELFA-BASIC IV or MOVEMASTER commands</param>
    /// <seealso cref="Execute(string)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteBlocking(string instruction) => DoCommandInternal($"EXEC2={instruction}");

    /// <summary>[STEP] Step execution</summary>
    /// <param name="method">Step method</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramStep(StepMethod method) => DoCommandInternal($"STEP{method.ToCommandString()}");

    /// <summary>[ECLR] Clear program contents</summary>
    /// <remarks>It is effective in the edit slot.</remarks>
    /// <seealso cref="ProgramDelete(int, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramClear() => DoCommandInternal("ECLR");

    /// <summary>[DELETE] Delete program lines</summary>
    /// <remarks>The line from the start line to the end line is deleted.</remarks>
    /// <param name="start">Delete start line number</param>
    /// <param name="end">Delete end line number</param>
    /// <seealso cref="ProgramClear"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramDelete(int start, int end) => DoCommandInternal($"DELETE{start};{end}");

    /// <summary>[RENUM] Program renumber</summary>
    /// <remarks>Renumber for program, effective in the edit slot.</remarks>
    /// <param name="newLine">New line number (0:10)</param>
    /// <param name="oldLineStart">Old start line number (0:Top)</param>
    /// <param name="step">Step of line number (0:10)</param>
    /// <param name="oldLineEnd">Old end line number (0:Bottom)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramRenumber(int newLine, int oldLineStart, int step, int oldLineEnd) =>
        DoCommandInternal($"RENUM{newLine};{oldLineStart};{step};{oldLineEnd}");
    #endregion

    #region File Operation
    // TODO: PDIR - Program directory
    // TODO: FDIR - File directory

    /// <summary>[FCHECK] File check</summary>
    /// <param name="filename">File name to check (if extension is ommitted, default to "MB5")</param>
    /// <returns>
    ///     <c>true</c>: File exists<br />
    ///     <c>false</c>: File does not exists<br />
    ///     <c>null</c>: Unknown
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool? FileExists(string filename)
    {
        var result = DoCommandInternal($"FCHECK{filename}");
        switch (result)
        {
            case "N": return true;
            case "F": return false;
            default: return null;
        }
    }

    /// <summary>[FPATH=] Get the save path name of the file</summary>
    /// <param name="filename">Target filename (if extension is omitted, default to "MB5")</param>
    /// <returns>File name in the path name with</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FileFullPath(string filename) => DoCommandInternal($"FPATH{filename}");

    /// <summary>[FCOPY] File copy</summary>
    /// <param name="source">Source file name</param>
    /// <param name="destination">Destination file name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FileCopy(string source, string destination) => DoCommandInternal($"FCOPY{source};{destination}");

    /// <summary>[FDEL] File delete</summary>
    /// <param name="filename">Target file name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FileDelete(string filename) => DoCommandInternal($"FDEL{filename}");

    /// <summary>[FRENAME] File rename</summary>
    /// <param name="source">Source file name</param>
    /// <param name="destination">Destination file name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FileRename(string source, string destination) => DoCommandInternal($"FRENAME{source};{destination}");

    /// <summary>[FATTRIB] File set / clear attribute</summary>
    /// <param name="filename">Target file name</param>
    /// <param name="readOnlyLines">
    ///     <c>true</c>: Read only lines (+p)<br />
    ///     <c>false</c>: Read and write lines (-p)<br />
    ///     <c>null</c>: Don't modify
    /// </param>
    /// <param name="readOnlyVariable">
    ///     <c>true</c>: Read only variable (+q)<br />
    ///     <c>false</c>: Read and write variable (-q)<br />
    ///     <c>null</c>: Don't modify
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FileAttribute(string filename, bool? readOnlyLines, bool? readOnlyVariable)
    {
        if (readOnlyLines is bool lines)
            DoCommandInternal(lines ? $"FATTRIB{filename};+p" : $"FATTRIB{filename};-p");
        if (readOnlyVariable is bool variable)
            DoCommandInternal(variable ? $"FATTRIB{filename};+q" : $"FATTRIB{filename};-q");
    }

    // TODO: FINIT - File init
    // TODO: FOPEN - File block open
    // TODO: FCLOSE - File block close

    /// <summary>[FREAD] File read</summary>
    /// <remarks>
    ///     <para>This is a combination of <c>FOPEN</c>, <c>FREAD</c>, and <c>FCLOSE</c>.</para>
    ///     <para>
    ///         It reads the full content to a <see cref="MemoryStream"/> all at once (instead
    ///         of on-demand), becuase we can't <c>FOPEN</c> multiple files on the controller.
    ///      </para>
    ///      <para>Thus, be prepared for the long-delay while reading large files.</para>
    /// </remarks>
    /// <param name="filename">The name of the file to open</param>
    /// <returns>
    ///     The resulting <see cref="MemoryStream"/>. Client code should
    ///     <see cref="IDisposable.Dispose()"/> it after use.
    /// </returns>
    public Stream FileRead(string filename)
    {
        var stream = new MemoryStream();
        try
        {
            WithOpenFile(filename, "r", () =>
            {
                int remaining;
                do
                {
                    var results = DoCommandInternal("FREAD").Split(';');
                    var bytes = HexConverter.GetBytes(results[0].AsSpan());
                    stream.Write(bytes, 0, bytes.Length);
                    remaining = int.Parse(results[1]);
                } while (remaining != 0);
            });
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    /// <summary>[FWRITE] File write</summary>
    /// <remarks>
    ///     <para>This is a combination of <c>FOPEN</c>, <c>FWRITE</c>, and <c>FCLOSE</c>.</para>
    ///     <para>
    ///         It writes the content of the <see cref="Stream"/> all at once (instead
    ///         of on-demand), becuase we can't <c>FOPEN</c> multiple files on the controller.
    ///     </para>
    /// </remarks>
    /// <param name="filename">Target filename</param>
    /// <param name="stream">Content to be written to the controller</param>
    public void FileWrite(string filename, Stream stream)
    {
        WithOpenFile(filename, "w", () =>
        {
            var bytes = new byte[110];
            int n;
            while ((n = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                var hex = HexConverter.GetString(bytes.AsSpan(0, n));
                DoCommandInternal($"FWRITE{n};{hex}");
            }
        });
    }

    /// <summary>[EFREE] Read file size</summary>
    /// <returns>(Total sizes of file system, Used size)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Total, int Used) FilesystemFree()
    {
        var ret = DoCommandInternal("EFREE").Split(';').Select(int.Parse).ToImmutableArray();
        return (ret[0], ret[1]);
    }

    // TODO: ESEARCH - String search
    #endregion

    #region Running
    /// <summary>[PRGLOAD=] Program load</summary>
    /// <remarks>The program is loaded into the task slot</remarks>
    /// <param name="program">Program name</param>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadProgram(string program, int slot = 1) => DoCommandInternal($"PRGLOAD={program}", slot);

    /// <summary>[PRGUP] Program select (up)</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramUp() => DoCommandInternal("PRGUP");

    /// <summary>[PRGDOWN] Program select (down)</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProgramDown() => DoCommandInternal("PRGDOWN");

    /// <summary>[PRGRD] Execution program name read</summary>
    /// <remarks>The program name of the task slot is read.</remarks>
    /// <param name="slot">Task slot</param>
    /// <returns>Program name</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetCurrentProgram(int slot = 1) => DoCommandInternal($"PRGRD", slot);

    /// <summary>[LINENO=] Execution line number change</summary>
    /// <remarks>The execution line number is changed.</remarks>
    /// <param name="line">Line number</param>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void JumpToProgram(int line, int slot = 1) => DoCommandInternal($"LINENO={line}", slot);

    /// <summary>[LINENO] Execution line number read</summary>
    /// <remarks>The execution line number is read.</remarks>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetProgramLineNumber(int slot = 1) => DoCommandInternal("LINENO", slot);

    /// <summary>[LINERD] Execution line contents</summary>
    /// <remakrs>The content of the line execution is read.</remakrs>
    /// <param name="slot">Task slot</param>
    /// <returns>The content of the line</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetProgramLineContent(int slot = 1) => DoCommandInternal("LINERD", slot);

    // TODO: LINESRD - Execution more line contents

    /// <summary>[SRV&lt;ON/OFF&gt;] Servo on or off</summary>
    /// <remakrs>The servo power supply is turned on or off.</remakrs>
    public bool ServoOn
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal(value ? "SRVON" : "SRVOFF");
    }

    /// <summary>[OVRD] [OVRD=] OP override change</summary>
    /// <remarks>The OP override is changed</remarks>
    public byte Override
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => byte.Parse(DoCommandInternal("OVRD"));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal($"OVRD={Math.Min(value, 100u)}");
    }

    /// <summary>[RUN] Program start</summary>
    /// <param name="program">Program name</param>
    /// <param name="mode">Program start mode</param>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunProgram(string program, TaskMode mode = TaskMode.Repeat, int slot = 1) => DoCommandInternal($"RUN{program};{(byte)mode}", slot);

    /// <summary>[STOP]Program stop</summary>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StopProgram(int slot = 1) => DoCommandInternal("STOP", slot);

    /// <summary>[STOP&lt;ON/OFF&gt;] Block the <see cref="RunProgram(string, TaskMode, int)" /></summary>
    /// <remarks>Block the start of program. It's not possible to start the program when blocked.</remarks>
    /// <param name="block">Select on or off</param>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlockProgramStart(bool block, int slot = 1) => DoCommandInternal(block ? "STOPON" : "STOPOFF", slot);

    /// <summary>[CSTOP] Cycle STOP</summary>
    /// <remarks>The program under the start stops at the cycle.</remarks>
    /// <param name="slot">Task slot</param>
    /// <seealso cref="RunProgram(string, TaskMode, int)" />
    /// <seealso cref="StopProgram(int)" />
    /// <seealso cref="BlockProgramStart(bool, int)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CycleStopProgram(int slot = 1) => DoCommandInternal("CSTOP", slot);

    /// <summary>[RSTALRM] Error reset</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetAlarm() => DoCommandInternal("RSTALRM");

    /// <summary>[SLOTINIT] All program reset</summary>
    /// <remarks>The program resets all slots.</remarks>
    /// <seealso cref="ResetProgram(int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetAllPrograms() => DoCommandInternal("SLOTINIT");

    /// <summary>[RSTPRG] Each program reset</summary>
    /// <remakrs>The program resets a specified slot.</remakrs>
    /// <param name="slot">Task slot</param>
    /// <seealso cref="ResetAllPrograms"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetProgram(int slot = 1) => DoCommandInternal("RSTPRG", slot);

    /// <summary>[RSTIO] Output signal reset</summary>
    /// <remakrs>The general-purpose output signal is reset.</remakrs>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetIO() => DoCommandInternal("RSTIO");

    /// <summary>[MLOCK&lt;ON/OFF&gt;] Machine lock ON or OFF</summary>
    public bool MachineLock
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal($"MLOCK{(value ? "ON" : "OFF")}");
    }

    /// <summary>[HND] Hand open or close</summary>
    /// <remakrs>The hand is opened and close.</remakrs>
    /// <param name="index">hand index (0 ~ 7)</param>
    /// <param name="state">true = opened, false = closed</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetHand(int index, bool state)
    {
        Guard.IsBetweenOrEqualTo(index, 0, 7);
        DoCommandInternal($"HND{(state ? "ON" : "OFF")}{index + 1}");
    }

    /// <summary>[ALIGN] Aligning the hand</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void HandAlign() => DoCommandInternal("ALIGN");

    /// <summary>[MOVSP] MOVE safe position ("JSAVE")</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveToSafePosition() => DoCommandInternal("MOVSP");

    /// <summary>[JOG] Jog operation</summary>
    /// <remarks>If the command is not received between about 140msec, the jog operation is automatically stopped.</remarks>
    /// <param name="mode">Jog mode</param>
    /// <param name="negative">
    ///     <para>The negative direction of operation specified by the axis pattern</para>
    ///     <para>
    ///         Bit 0: X, J1<br />
    ///         Bit 1: Y, J2<br />
    ///         Bit 2: Z, J3<br />
    ///         Bit 3: A, J4<br />
    ///         Bit 4: B, J5<br />
    ///         Bit 5: C, J6<br />
    ///         Bit 6: L1, J7<br />
    ///         Bit 7: L2, J8
    ///     </para>
    /// </param>
    /// <param name="positive">
    ///     <para>The positive direction of operation specified by the axis pattern</para>
    ///     <para>
    ///         Bit 0: X, J1<br />
    ///         Bit 1: Y, J2<br />
    ///         Bit 2: Z, J3<br />
    ///         Bit 3: A, J4<br />
    ///         Bit 4: B, J5<br />
    ///         Bit 5: C, J6<br />
    ///         Bit 6: L1, J7<br />
    ///         Bit 7: L2, J8
    ///     </para>
    /// </param>
    /// <param name="inching">Inching mode</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Jog(JogMode mode, byte negative, byte positive, InchingMode inching = InchingMode.Off) =>
        DoCommandInternal($"JOG{(byte)mode:x2};00;{negative:x2};{positive:x2};{(byte)inching:x2}");

    /// <summary>[JOG] JOG operation for Multifunctional Electric Hand</summary>
    /// /// <remarks>If the command is not received between about 140msec, the jog operation is automatically stopped.</remarks>
    /// <param name="mode">Electric hand jog mode</param>
    /// <param name="speed">Movement speed (0 ~ 100)</param>
    /// <param name="force">Holding force (0 ~ 100)</param>
    /// <param name="point">Point number (0 ~ 32)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Jog(ElectricHandJogMode mode, byte speed, byte force, byte point) =>
        DoCommandInternal($"JOG{(int)mode:x2};00;{speed:x2};{force:x2};{point:x2}");

    /// <summary>[LS&lt;ON/OFF&gt;] Limit switch ON or OFF</summary>
    public bool LimitSwitch
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal($"LS{(value ? "ON" : "OFF")}");
    }

    /// <summary>[ATENA] [AUE&lt;ON/OFF&gt;] Read start eligibility or enable/disable program start.</summary>
    public bool ProgramStart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DoCommandInternal("ATENA") switch
        {
            "0" => true,
            "1" => false,
            var status => throw new InvalidDataException($"Unexpected ATENA status: {status}")
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal(value ? "AUEON" : "AUEOFF");
    }

    /// <summary>[BRKPTSET] Set breakpoint</summary>
    /// <param name="program">Program name</param>
    /// <param name="line">Target line number</param>
    /// <param name="type">Breakpoint type</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBreakpoint(string program, int line, BreakpointType type) => DoCommandInternal($"BRKPTSET={program};{line};{(int)type}");

    /// <summary>[BRKPTCLR] Delete breakpoint</summary>
    /// <param name="program">Program name</param>
    /// <param name="line">Target line number (0 to clear all)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearBreakpoint(string program, int line = 0) => DoCommandInternal($"BRKPTCLR={program};{line}");

    // TODO: BRKPTGET - List breakpoint

    /// <summary>Get or set the tool number (0 ~ 16)</summary>
    public int Tool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.Parse(DoCommandInternal("TOOLRD"));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            CommunityToolkit.Diagnostics.Guard.IsBetweenOrEqualTo(value, 0, 16);
            DoCommandInternal($"TOOLSET{value}");
        }
    }

    /// <summary>[SAFE=] Low speed mode</summary>
    public bool LowSpeedMode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal(value ? "SAFE=1" : "SAFE=0");
    }
    #endregion

    #region Monitor
    /// <summary>[STATE] Read run status</summary>
    /// <param name="slot">Task slot</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RunState GetState(int slot = 1) => new(DoCommandInternal("STATE", slot), slot);

    // TODO: DSTATE - Read stop status
    // TODO: CALIB - Install status

    /// <summary>[IOSIGNAL] Input and output signal read</summary>
    /// <param name="input">Input signal number</param>
    /// <param name="output">Output signal number</param>
    /// <returns>(Input signal state, Output signal state)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ushort Input, ushort Output) ReadInputOutput(int input, int output)
    {
        var io = DoCommandInternal($"IOSIGNAL{input};{output}");
        return (
            ushort.Parse(io.Substring(0, 4), NumberStyles.HexNumber),
            ushort.Parse(io.Substring(4, 4), NumberStyles.HexNumber)
        );
    }

    /// <summary>[IN] Read GPIO input</summary>
    /// <param name="index">Index of the input signal</param>
    /// <returns>Current input states</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadInput(int index) => ushort.Parse(DoCommandInternal($"IN{index}"), NumberStyles.HexNumber);

    /// <summary>[OUT] Read GPIO output</summary>
    /// <param name="index">Index of the output signal</param>
    /// <returns>Current output signal states</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadOutput(int index) => ushort.Parse(DoCommandInternal($"OUT{index}"), NumberStyles.HexNumber);

    /// <summary>[OUT=] Write GPIO output</summary>
    /// <param name="index">Index of the output signal</param>
    /// <param name="value">Target output states</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteOutput(int index, ushort value) => DoCommandInternal($"OUT={index};{value:x4}");

    /// <summary>[DIN] CC-Link's input register data read</summary>
    /// <param name="index">Input register number</param>
    /// <returns>The input registered returned by the DEC (16 pieces)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ushort> ReadCCLinkInput(int index) => DoCommandInternal($"DIN{index}").Split(';').Select(ushort.Parse).ToImmutableArray();

    /// <summary>[DOUT] CC-Link's output register data read</summary>
    /// <param name="index">Output register number</param>
    /// <returns>The output registered returned by the DEC (16 pieces)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ushort> ReadCCLinkOutput(int index) => DoCommandInternal($"DOUT{index}").Split(';').Select(ushort.Parse).ToImmutableArray();

    /// <summary>[DOUT=] CC-Link's output register data write</summary>
    /// <param name="index">Output register number</param>
    /// <param name="value">Output register value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteCCLinkOutput(int index, ushort value) => DoCommandInternal($"DOUT={index};{value:x4}");

    /// <summary>[IN&lt;DMY/SET&gt;] Enable / Disable pseudo input</summary>
    public bool DummyInput
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal(value ? "INDMY" : "INSET");
    }

    /// <summary>[IN=] Write pseudo input data</summary>
    /// <param name="index">Index of the input signal</param>
    /// <param name="value">Target pseudo input signal states</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInput(int index, ushort value) => DoCommandInternal($"IN={index};{value:x4}");

    /// <summary>[DIN=] Write CC-Link pseudo input data</summary>
    /// <param name="index">Index of the input signal</param>
    /// <param name="value">Target pseudo input signal states</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteCCLinkInput(int index, ushort value) => DoCommandInternal($"DIN={index};{value:x4}");

    // TODO: STPSIG - Stop signal read

    /// <summary>[HNDSTS] Hand output signal read</summary>
    public ImmutableArray<HandState> HandStates
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => RegexHelper.HandStatesRegex()
            .Matches(DoCommandInternal("HNDSTS"))
            .OfType<Match>()
            .Select(match => new HandState(match))
            .ToImmutableArray();
    }

    // TODO: USERAREASTS - User specified area read

    public PositionJ CurrentPositionJ
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PositionJ.FromCommand(DoCommandInternal("JPOSF"));
    }
    public PositionP CurrentPositionP
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PositionP.FromCommand(DoCommandInternal("PPOSF"));
    }

    // TODO: {X,R}POS{1-8,F} - Current position read

    public PositionJ DestinationPositionJ
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PositionJ.FromCommand(DoCommandInternal("GJPOSF"));
    }
    public PositionP DestinationPositionP
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PositionP.FromCommand(DoCommandInternal("GPPOSF"));
    }

    /// <summary>[TIME] Controller clock as date (yy-MM-dd) and time (HH:mm:ss).</summary>
    /// <remarks>The controller does not specify a century or time zone.</remarks>
    public (string Date, string Time) ControllerClock
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadControllerClock();
    }

    /// <summary>[TIME=] Set the controller clock.</summary>
    /// <param name="date">Date in yy-MM-dd format.</param>
    /// <param name="time">Time in HH:mm:ss format.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetControllerClock(string date, string time)
    {
        if (!IsValidControllerTimestamp(date, time))
            throw new ArgumentException("Expected a valid date (yy-MM-dd) and time (HH:mm:ss).");
        DoCommandInternal($"TIME={date}{time}");
    }

    /// <summary>[PTIME] Read hour-meter data.</summary>
    public HourMeterData HourMeter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadHourMeter();
    }

    /// <summary>[PTIMEDEL=] Hour meter clear</summary>
    /// <remarks>The operating time of the robot is cleared.</remarks>
    /// <param name="kind">Kind at operation time</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearHourMeter(HourMeterKind kind) => DoCommandInternal($"PTIMEDEL={kind.ToCommandString()}");

    /// <summary>[CYCLETIME] Read a program's cycle-time data.</summary>
    /// <param name="program">Program name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CycleTimeData GetCycleTime(string program)
    {
        if (string.IsNullOrWhiteSpace(program) || program.Any(c => c == ';' || char.IsControl(c)))
            throw new ArgumentException("Invalid program name.", nameof(program));
        return ReadCycleTime(program);
    }

    /// <summary>[CYCLECLR] Cycle time clear</summary>
    /// <remakrs>The operating time of the program is cleared</remakrs>
    /// <param name="program">program name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CycleTimeClear(string program) => DoCommandInternal($"CYCLECLR{program}");

    public MelfaRobotException Error
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            try { ThrowIfError(); } catch (MelfaRobotException ex) { return ex; }
            return null;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfError()
    {
        var ret = DoCommandInternal("ERROR");
        if (!string.IsNullOrWhiteSpace(ret))
            throw MelfaRobotException.FromCommandResult(ret);
    }

    /// <summary>[ERRORMES] Read error contents from the controller.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetErrorMessage(int errorNumber)
    {
        if (errorNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(errorNumber));
        return DoCommandInternal($"ERRORMES{errorNumber}");
    }

    /// <summary>[ERRORLOG] Read an error history entry, or null if no history is available.</summary>
    /// <remarks>Top is newest, Bottom oldest; Next and Previous navigate the controller's shared history cursor.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorHistoryEntry GetErrorHistory(LocationType position) =>
        ParseErrorHistory(DoCommandInternal($"ERRORLOG{position.ToProgramLocationCommandString()}"));

    /// <summary>[ERRLOG2=] Read filtered error history, or null if no history matches.</summary>
    /// <param name="position">History position.</param>
    /// <param name="severity">High, low, caution or all; null selects all levels.</param>
    /// <param name="detailedErrorNumber">A 6-9 digit detail number.</param>
    /// <remarks>Next and Previous use the controller's last filter and shared history cursor.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorHistoryEntry GetFilteredErrorHistory(LocationType position, string detailedErrorNumber, ErrorSeverity? severity = null)
    {
        var level = ErrorHistoryFilterLevel(severity);
        if (detailedErrorNumber == null || !Regex.IsMatch(detailedErrorNumber, @"\A[0-9]{6,9}\z"))
            throw new ArgumentException("The detailed error number must contain 6 to 9 digits.", nameof(detailedErrorNumber));
        return ParseErrorHistory(DoCommandInternal($"ERRLOG2={position.ToProgramLocationCommandString()};{level};{detailedErrorNumber}"));
    }

    /// <summary>[ERRLOGCLR] Error history clear</summary>
    /// <param name="severity">High, low, caution, or all error levels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearErrorLog(ErrorSeverity severity) => DoCommandInternal($"ERRLOGCLR{ErrorHistoryFilterLevel(severity)}");

    /// <summary>[ERRSUM] Read an error summary entry by index (1-100).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorSummary GetErrorSummary(int index) => ReadErrorSummary(index, false);

    /// <summary>[ERRSUM2=] Read an error summary entry and its last occurrence by index (1-100).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ErrorSummary GetErrorSummaryWithLastOccurrence(int index) => ReadErrorSummary(index, true);

    /// <summary>[ERRSUMCLR] Clear error summary</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearErrorSummary() => DoCommandInternal("ERRSUMCLR");

    /// <summary>[SUMDATE0] Date and time when error history collection began.</summary>
    public ErrorTimestamp ErrorLogStartDate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ParseErrorTimestampResponse(DoCommandInternal("SUMDATE0"));
    }

    /// <summary>[SUMDATE1] Date and time when error summary collection began.</summary>
    public ErrorTimestamp ErrorSummaryStartDate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ParseErrorTimestampResponse(DoCommandInternal("SUMDATE1"));
    }

    /// <summary>[VAL] Read the value of a named variable.</summary>
    /// <remarks>Use the edit slot (typically TASKMAX+1) to read edit-slot variables.</remarks>
    /// <param name="name">Variable name.</param>
    /// <param name="variableClass">Variable class.</param>
    /// <param name="slot">Task slot.</param>
    /// <returns>The variable value as returned by the controller.</returns>
    /// <exception cref="InvalidDataException">The response does not contain the requested variable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadVariable(string name, VariableClass variableClass, int slot = 1)
    {
        ValidateVariable(name);
        return ParseVariableResponse(name, DoCommandInternal($"VAL{name};{variableClass.ToCommandString()}", slot));
    }

    internal static void ValidateVariable(string name)
    {
        if (string.IsNullOrEmpty(name) || !Regex.IsMatch(name, @"\A[A-Za-z][A-Za-z0-9_]*\z"))
            throw new ArgumentException("Invalid variable name.", nameof(name));
    }

    internal static string ParseVariableResponse(string name, string result)
    {
        var separator = result.IndexOf('=');
        if (separator <= 0 || !string.Equals(result.Substring(0, separator), name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Unexpected variable response: {result}");
        return result.Substring(separator + 1);
    }

    /// <summary>[VALS] Read a page of variables from a task or edit slot.</summary>
    /// <param name="position">The starting position or direction.</param>
    /// <param name="variableClass">Variable class.</param>
    /// <param name="slot">Task slot; use TASKMAX+1 for the edit slot.</param>
    /// <returns>The variables and continuation flag for this page.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VariablePage ReadVariables(LocationType position, VariableClass variableClass, int slot = 1) =>
        ReadVariablePage("VALS", position, variableClass, slot);

    /// <summary>[GVAL] Read the value of a named global variable.</summary>
    /// <param name="name">Variable name.</param>
    /// <param name="variableClass">Variable class.</param>
    /// <param name="slot">Task slot.</param>
    /// <returns>The variable value as returned by the controller.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadGlobalVariable(string name, VariableClass variableClass, int slot = 1)
    {
        ValidateVariable(name);
        return ParseVariableResponse(name, DoCommandInternal($"GVAL{name};{variableClass.ToCommandString()}", slot));
    }

    /// <summary>[GVAL] Read a global variable at a relative position.</summary>
    /// <param name="position">The starting position or direction.</param>
    /// <param name="variableClass">Variable class.</param>
    /// <param name="slot">Task slot.</param>
    /// <returns>The variable name and value as returned by the controller.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValuePair<string, string> ReadGlobalVariable(LocationType position, VariableClass variableClass, int slot = 1) =>
        ParseVariableEntry(DoCommandInternal($"GVAL{position.ToVariablePositionCommandString()};{variableClass.ToCommandString()}", slot));

    /// <summary>[GVALS] Read a page of global variables.</summary>
    /// <param name="position">The starting position or direction.</param>
    /// <param name="variableClass">Variable class.</param>
    /// <param name="slot">Task slot.</param>
    /// <returns>The variables and continuation flag for this page.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VariablePage ReadGlobalVariables(LocationType position, VariableClass variableClass, int slot = 1) =>
        ReadVariablePage("GVALS", position, variableClass, slot);

    /// <summary>[HOT] Variable data write</summary>
    /// <remarks>The value of the variable is changed. A integer variable is revocable while even starting.</remarks>
    /// <param name="program">Program name</param>
    /// <param name="variable">Variable name</param>
    /// <param name="value">Changed value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVariable(string program, string variable, string value) => DoCommandInternal($"HOT{program};{variable}={value}");

    /// <summary>[OPNUMRD] Number of option slots</summary>
    public int OptionSlotCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.Parse(DoCommandInternal("OPNUMRD"));
    }

    // TODO: OPSTSRD

    /// <summary>[THMRD] The internal temperature of the controller</summary>
    public double ControllerTemperature
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => double.Parse(DoCommandInternal("THMRD"));
    }

    /// <summary>[ETEMP] The temperature of each encoder</summary>
    public ImmutableArray<double> EncoderTemperatures
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DoCommandInternal("ETEMP")
            .Split(';')
            .Select(double.Parse)
            .Take(8)
            .ToImmutableArray();
    }

    /// <summary>[ETEMP] Maximum temperature of each encoder</summary>
    public ImmutableArray<double> MaximumEncoderTemperatures
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DoCommandInternal("ETEMP")
            .Split(';')
            .Select(double.Parse)
            .Skip(8)
            .ToImmutableArray();
    }

    /// <summary>[EMISS] Miss count of each encoder</summary>
    public ImmutableArray<int> EncoderMissCounts
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DoCommandInternal("EMISS")
            .Split(';')
            .Select(int.Parse)
            .ToImmutableArray();
    }

    /// <summary>[SRVENC] Encoder position feedback, position in one rotation, Fdt command, and position command for J1-J8.</summary>
    public ImmutableArray<(long PositionFeedback, long PositionInOneRotation, long FdtCommand, long PositionCommand)> ServoEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVENC", 4, (fields, i) =>
            (ParseServoInteger(fields[i]), ParseServoInteger(fields[i + 1]), ParseServoInteger(fields[i + 2]), ParseServoInteger(fields[i + 3])));
    }

    /// <summary>[SRVDRP] Position droop and maximum position droop for J1-J8.</summary>
    public ImmutableArray<(long PositionDroop, long MaximumPositionDroop)> ServoDroop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVDRP", 2, (fields, i) =>
            (ParseServoInteger(fields[i]), ParseServoInteger(fields[i + 1])));
    }

    /// <summary>[SRVSPD] Speed feedback, maximum speed, and speed command for J1-J8.</summary>
    public ImmutableArray<(long SpeedFeedback, long MaximumSpeed, long SpeedCommand)> ServoSpeed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVSPD", 3, (fields, i) =>
            (ParseServoInteger(fields[i]), ParseServoInteger(fields[i + 1]), ParseServoInteger(fields[i + 2])));
    }

    /// <summary>[SRVCUR0] Current command, positive and negative tolerable commands, and RMS current for J1-J8.</summary>
    public ImmutableArray<(double CurrentCommand, double TolerableCommandPositive, double TolerableCommandNegative, double RmsCurrent)> ServoCurrentCommands
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVCUR0", 4, (fields, i) =>
            (ParseServoNumber(fields[i]), ParseServoNumber(fields[i + 1]), ParseServoNumber(fields[i + 2]), ParseServoNumber(fields[i + 3])));
    }

    /// <summary>[SRVCUR1] Current feedback and two maximum current commands for J1-J8.</summary>
    public ImmutableArray<(double CurrentFeedback, double MaximumCurrentCommand1, double MaximumCurrentCommand2)> ServoCurrentFeedback
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVCUR1", 3, (fields, i) =>
            (ParseServoNumber(fields[i]), ParseServoNumber(fields[i + 1]), ParseServoNumber(fields[i + 2])));
    }

    /// <summary>[SRVLCR] Load level and maximum load level for J1-J8.</summary>
    public ImmutableArray<(double LoadLevel, double MaximumLoadLevel)> ServoLoadCurrent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadServoAxes("SRVLCR", 2, (fields, i) =>
            (ParseServoNumber(fields[i]), ParseServoNumber(fields[i + 1])));
    }

    /// <summary>[SRVVOL] Power voltage, maximum and minimum voltage, and regeneration levels for J1-J8.</summary>
    public (double PowerVoltage, double MaximumPowerVoltage, double MinimumPowerVoltage, ImmutableArray<double> RegenerationLevels) ServoVoltage
    {
        get
        {
            var fields = ReadServoFields("SRVVOL", 11);
            return (ParseServoNumber(fields[0]), ParseServoNumber(fields[1]), ParseServoNumber(fields[2]),
                fields.Skip(3).Select(ParseServoNumber).ToImmutableArray());
        }
    }

    /// <summary>[SRVMONRST=] Reset servo monitor data</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ServoMonitorReset() => DoCommandInternal("SRVMONRST=");

    // RAREAD
    #endregion

    #region Maintenance
    /// <summary>[PRMINIT] Reset all parameters to factory default</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FactoryReset() => DoCommandInternal("PRMINIT");

    /// <summary>[PNR] Parameter read</summary>
    /// <remarks>The parameter at the level corresponding to <see cref="ParameterKeyword" /> is read</remarks>
    /// <param name="name">Parameter name</param>
    /// <returns>Parameter value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetParameter(string name) => DoCommandInternal($"PNR{name}").Split(';')[1];

    /// <summary>[PRM=] Parameter write</summary>
    /// /// <remarks>The parameter at the level corresponding to <see cref="ParameterKeyword" /> is changed</remarks>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Target parameter value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetParameter(string name, string value) => DoCommandInternal($"PRM={name};{value}");

    /// <summary>[PAR] Parameter read</summary>
    /// <remarks>The parameter at the level corresponding to <see cref="ParameterKeyword" /> is read</remarks>
    /// <param name="name">parameter name</param>
    /// <returns>Parameter value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadParameter(string name) => DoCommandInternal($"PAR{name}").Split(';')[1];

    /// <summary>[PAW=] Parameter write</summary>
    /// <remarks>The parameter at the level corresponding to <see cref="ParameterKeyword" /> is changed</remarks>
    /// <param name="name">Parameter name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteParameter(string name, string value) => DoCommandInternal($"PAW={name};{value}");

    /// <summary>[PAW2=] Parameter write which need to reboot</summary>
    /// <remarks>
    ///     <para>All parameters are able to write regardless of keyword (<see cref="ParameterKeyword" />.</para>
    ///     <para>However, cycling the power is required without fail for the reflection of the parameter setting value.</para>
    ///     <para>
    ///         (Not possible on CRn-500 series controller)<br />
    ///         (Possible on CRnQ-700 / CRnD-700 since verswion R2 / S2)
    ///     </para>
    /// </remarks>
    /// <param name="name">Parameter name</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteParameterAndReboot(string name, string value) => DoCommandInternal($"PAW2={name};{value}");

    /// <summary>[PRMUNDO] Reset parameter to factory default</summary>
    /// <param name="name"></param>
    /// <param name="mech"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FactoryReset(string name, int mech = 0) => DoCommandInternal($"PRMUNDO{mech};{name}");

    /// <summary>[PMR=] Read change parameter list</summary>
    /// <returns>List of changed parameters</returns>
    public IEnumerable<string> ListChangedParameters()
    {
        var results = DoCommandInternal("PMR=0").Split(';');
        while (true)
        {
            if (int.Parse(results[1]) > 0)
            {
                var parameters = results[2].Split('\x0b');
                foreach (var parameter in parameters)
                    yield return parameter;
            }
            if (int.Parse(results[0]) == 0)
                yield break;
            results = DoCommandInternal("PMR=1").Split(';');
        }
    }

    /// <summary>[KEYWD] Keyword</summary>
    /// <remarks>The level of opening to the public of the parameter is changed.</remarks>
    public string ParameterKeyword
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => DoCommandInternal($"KEYWDp{value}");
    }

    /// <summary>[SLOTRD] Slot table read</summary>
    /// <param name="slot">slot index</param>
    /// <returns>Slot info</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskSlotInfo SlotGet(int slot) => new(DoCommandInternal("SLOTRD", slot), slot);

    /// <summary>[SLOTSET] Slot table write</summary>
    /// <remarks>The slot table is changed ("SLTn" parameter is changed), reboot required.</remarks>
    /// <param name="slot">slot index</param>
    /// <param name="program">Program name of slot table</param>
    /// <param name="mode">Operation mode of slot table</param>
    /// <param name="condition">Starting condition of slot table</param>
    /// <param name="priority">Priority of slot table (1 ~ 31)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SlotSet(int slot, string program, TaskMode mode, TaskCondition condition, int priority) =>
        DoCommandInternal($"SLOTSET{program};{mode.ToCommandString()};{condition.ToCommandString()};{priority}", slot);

    /// <summary>[ENCBATTM] Battery remain time</summary>
    public (int PowerOnHour, int RemainingHour) BatteryRemainingTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var values = DoCommandInternal("ENCBATTM")
                .Split(';')
                .Select(int.Parse)
                .ToImmutableArray();
            return (values[0], values[1]);
        }
    }

    /// <summary>[BREAKON] Release brake</summary>
    /// <remarks>When <c>0x00</c> is specified, the brake is locked.</remarks>
    /// <param name="axis">
    ///     Bit 0: J1<br />
    ///     Bit 1: J2<br />
    ///     Bit 2: J3<br />
    ///     Bit 3: J4<br />
    ///     Bit 4: J5<br />
    ///     Bit 5: J6<br />
    ///     Bit 6: J7<br />
    ///     Bit 7: J8
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseBrake(byte axis) => DoCommandInternal($"BREAKON{axis:x2}");

    /// <summary>[BREAKONF] Force release brake</summary>
    /// <remarks>
    ///     <para>The brake is released. (No operation right required)</para>
    ///     <para>When <c>0x00</c> is specified, the brake is locked.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForceReleaseBrake(byte axis) => DoCommandInternal($"BREAKONF{axis:x2}");

    /// <summary>[HOME] Setting the origin</summary>
    /// <param name="type">origin type</param>
    /// <param name="axis">
    ///     Bit 0: J1<br />
    ///     Bit 1: J2<br />
    ///     Bit 2: J3<br />
    ///     Bit 3: J4<br />
    ///     Bit 4: J5<br />
    ///     Bit 5: J6<br />
    ///     Bit 6: J7<br />
    ///     Bit 7: J8
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetOrigin(OriginType type, byte axis) => DoCommandInternal($"HOME{(int)type};{axis:x2}");

    /// <summary>[AXDATINST] Additional axis add for DATINST and DATRD</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InstallAdditionalAxisData() => DoCommandInternal("AXDATINST");

    /// <summary>[DATINST] Data input origin set</summary>
    /// <param name="j1">J1 data</param>
    /// <param name="j2">J2 data</param>
    /// <param name="j3">J3 data</param>
    /// <param name="j4">J4 data</param>
    /// <param name="j5">J5 data</param>
    /// <param name="j6">J6 data</param>
    /// <param name="checksum">Checksum</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string checksum) =>
        DoCommandInternal($"DATINST{j1};{j2};{j3};{j4};{j5};{j6};{checksum}");

    /// <summary>[DATINST] Data input origin set</summary>
    /// <param name="j1">J1 data</param>
    /// <param name="j2">J2 data</param>
    /// <param name="j3">J3 data</param>
    /// <param name="j4">J4 data</param>
    /// <param name="j5">J5 data</param>
    /// <param name="j6">J6 data</param>
    /// <param name="j7">J7 data</param>
    /// <param name="j8">J8 data</param>
    /// <param name="checksum">Checksum</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InstallAxisOriginData(string j1, string j2, string j3, string j4, string j5, string j6, string j7, string j8, string checksum) =>
        DoCommandInternal($"DATINST{j1};{j2};{j3};{j4};{j5};{j6};{j7};{j8};{checksum}");

    /// <summary>[DATRD] Data input origin read</summary>
    /// <returns>(Axis origin data, Checksum)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (IEnumerable<string> OriginData, string Checksum) ReadAxisOriginData()
    {
        var results = DoCommandInternal("DATRD").Split(';');
        return (results.Take(results.Length - 1), results.Last());
    }

    /// <summary>[RSTPWR] Reset power</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetPower() => DoCommandInternal("RSTPWR");

    /// <summary>[RPWRCHK=] Reset power check</summary>
    public bool CanResetPower
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.Parse(DoCommandInternal("RPWRCHK=")) == 1;
    }

    // TODO: MFTIME= - Read maintenance forecast reset date

    /// <summary>[MFRST=] Maintenance forecast reset</summary>
    /// <param name="axis">
    ///     Bit 0: J1<br />
    ///     Bit 1: J2<br />
    ///     Bit 2: J3<br />
    ///     Bit 3: J4<br />
    ///     Bit 4: J5<br />
    ///     Bit 5: J6<br />
    ///     Bit 6: J7<br />
    ///     Bit 7: J8
    /// </param>
    /// <param name="target">Reset target</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MaintenanceForecastReset(byte axis, MaintenanceResetTarget target) =>
        DoCommandInternal($"MFRST={axis:x2};{(byte)target:x2}");

    /// <summary>[MFFCST=] Maintenance forecast read</summary>
    /// <param name="type">forecast type</param>
    /// <returns>list of forecast hour</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<int> MaintenanceForecastHour(MaintenanceType type) =>
        DoCommandInternal($"MFFCST={(int)type}")
            .Split(';')
            .Select(int.Parse)
            .ToImmutableArray();
    #endregion

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }
}