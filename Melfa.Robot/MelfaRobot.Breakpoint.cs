namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A breakpoint returned by BRKPTGET.</summary>
    public readonly struct Breakpoint
    {
        internal Breakpoint(string programName, int lineNumber, BreakpointType type)
        {
            ProgramName = programName;
            LineNumber = lineNumber;
            Type = type;
        }

        public string ProgramName { get; }
        public int LineNumber { get; }
        public BreakpointType Type { get; }
    }
}
