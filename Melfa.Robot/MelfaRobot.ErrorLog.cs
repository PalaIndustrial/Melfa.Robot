namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A controller date and time, retained in the protocol's yy-MM-dd and HH:mm:ss formats.</summary>
    public sealed class ErrorTimestamp
    {
        internal ErrorTimestamp(string date, string time) { Date = date; Time = time; }
        public string Date { get; }
        public string Time { get; }
    }

    /// <summary>An error history entry reported by the controller.</summary>
    public sealed class ErrorHistoryEntry
    {
        internal ErrorHistoryEntry(ErrorTimestamp timestamp, int number, string contents, ErrorSeverity severity,
            string program, int line, string detailNumber)
        {
            Timestamp = timestamp;
            ErrorNumber = number;
            Contents = contents;
            Severity = severity;
            ProgramName = program;
            LineNumber = line;
            DetailedErrorNumber = detailNumber;
        }

        public ErrorTimestamp Timestamp { get; }
        public int ErrorNumber { get; }
        public string Contents { get; }
        public ErrorSeverity Severity { get; }
        public string ProgramName { get; }
        public int LineNumber { get; }
        public string DetailedErrorNumber { get; }
    }

    /// <summary>An individual controller error summary entry.</summary>
    public sealed class ErrorSummaryEntry
    {
        internal ErrorSummaryEntry(string detailNumber, ErrorSeverity severity, string contents,
            string cause, string measures, int occurrences, ErrorTimestamp lastOccurrence)
        {
            DetailedErrorNumber = detailNumber;
            Severity = severity;
            Contents = contents;
            Cause = cause;
            Measures = measures;
            Occurrences = occurrences;
            LastOccurrence = lastOccurrence;
        }

        public string DetailedErrorNumber { get; }
        public ErrorSeverity Severity { get; }
        public string Contents { get; }
        public string Cause { get; }
        public string Measures { get; }
        public int Occurrences { get; }
        /// <summary>Last occurrence, or null when read with ERRSUM.</summary>
        public ErrorTimestamp LastOccurrence { get; }
    }

    /// <summary>An error summary response, including the total number of recorded errors.</summary>
    public sealed class ErrorSummary
    {
        internal ErrorSummary(int totalCount, ErrorSummaryEntry entry) { TotalCount = totalCount; Entry = entry; }
        public int TotalCount { get; }
        /// <summary>The selected entry, or null when the controller reports no entry at this index.</summary>
        public ErrorSummaryEntry Entry { get; }
    }
}
