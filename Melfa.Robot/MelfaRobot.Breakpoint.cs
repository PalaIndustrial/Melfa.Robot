using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A breakpoint returned by BRKPTGET.</summary>
    public readonly struct Breakpoint
    {
        public Breakpoint(string programName, int lineNumber, BreakpointType type)
        {
            ProgramName = programName;
            LineNumber = lineNumber;
            Type = type;
        }

        public string ProgramName { get; }
        public int LineNumber { get; }
        public BreakpointType Type { get; }

        /// <summary>Formats the breakpoint as a BRKPTGET response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}",
            ProgramName, LineNumber, Type == BreakpointType.OneTime ? 1 : 0);
    }
}
