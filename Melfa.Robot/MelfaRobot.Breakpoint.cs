namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A breakpoint returned by BRKPTGET.</summary>
    public readonly struct Breakpoint
    {
        private readonly string _protocolPayload;

        internal Breakpoint(string programName, int lineNumber, BreakpointType type, string protocolPayload)
        {
            _protocolPayload = protocolPayload;
            ProgramName = programName;
            LineNumber = lineNumber;
            Type = type;
        }

        public string ProgramName { get; }
        public int LineNumber { get; }
        public BreakpointType Type { get; }
        public override string ToString() => RequireProtocolPayload(_protocolPayload);
    }
}
