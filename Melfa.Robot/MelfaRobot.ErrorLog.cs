namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A controller date and time, retained in the protocol's yy-MM-dd and HH:mm:ss formats.</summary>
    public sealed class ErrorTimestamp
    {
        internal ErrorTimestamp(string date, string time) { Date = date; Time = time; }
        public string Date { get; }
        public string Time { get; }
        public override string ToString() => $"{Date};{Time}";
    }

    /// <summary>An error history entry reported by the controller.</summary>
    public sealed class ErrorHistoryEntry
    {
        private readonly string _protocolPayload;

        internal ErrorHistoryEntry(ErrorTimestamp timestamp, int number, string contents, ErrorSeverity severity,
            string program, int line, string detailNumber, string protocolPayload)
        {
            _protocolPayload = protocolPayload;
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
        public override string ToString() => _protocolPayload;
    }

    /// <summary>An individual controller error summary entry.</summary>
    public sealed class ErrorSummaryEntry
    {
        private readonly string _protocolPayload;

        internal ErrorSummaryEntry(string detailNumber, ErrorSeverity severity, string contents,
            string cause, string measures, int occurrences, ErrorTimestamp lastOccurrence, string protocolPayload)
        {
            _protocolPayload = protocolPayload;
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
        public override string ToString() => _protocolPayload;
    }

    /// <summary>An error summary response, including the total number of recorded errors.</summary>
    public sealed class ErrorSummary
    {
        private readonly string _protocolPayload;

        internal ErrorSummary(int totalCount, ErrorSummaryEntry entry, string protocolPayload)
        {
            TotalCount = totalCount;
            Entry = entry;
            _protocolPayload = protocolPayload;
        }
        public int TotalCount { get; }
        /// <summary>The selected entry, or null when the controller reports no entry at this index.</summary>
        public ErrorSummaryEntry Entry { get; }
        public override string ToString() => _protocolPayload;
    }
}
