using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A controller date and time, retained in the protocol's yy-MM-dd and HH:mm:ss formats.</summary>
    public sealed class ErrorTimestamp
    {
        public ErrorTimestamp(string date, string time) { Date = date; Time = time; }
        public string Date { get; }
        public string Time { get; }
        public override string ToString() => $"{Date};{Time}";
    }

    /// <summary>An error history entry reported by the controller.</summary>
    public sealed class ErrorHistoryEntry
    {
        public ErrorHistoryEntry(ErrorTimestamp timestamp, int number, string contents, ErrorSeverity severity,
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

        /// <summary>Formats the entry as an ERRORLOG response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4};{5};{6}",
            Timestamp?.ToString() ?? ";", ErrorNumber, Contents, (int)Severity, ProgramName, LineNumber, DetailedErrorNumber);
    }

    /// <summary>An individual controller error summary entry.</summary>
    public sealed class ErrorSummaryEntry
    {
        public ErrorSummaryEntry(string detailNumber, ErrorSeverity severity, string contents,
            string cause, string measures, int occurrences, ErrorTimestamp lastOccurrence = null)
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

        /// <summary>Formats the entry as the ERRSUM (or ERRSUM2 when <see cref="LastOccurrence"/> is set) payload after the total count.</summary>
        public override string ToString()
        {
            var text = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4};{5}",
                DetailedErrorNumber, FormatSummarySeverity(Severity), Contents, Cause, Measures, Occurrences);
            return LastOccurrence is null ? text : text + ";" + LastOccurrence;
        }

        private static string FormatSummarySeverity(ErrorSeverity severity) => severity switch
        {
            ErrorSeverity.High => "H",
            ErrorSeverity.Low => "L",
            ErrorSeverity.Caution => "C",
            _ => "",
        };
    }

    /// <summary>An error summary response, including the total number of recorded errors.</summary>
    public sealed class ErrorSummary
    {
        private const string EmptyEntry = "000000000;;;;;0";

        /// <param name="totalCount">Total number of recorded errors.</param>
        /// <param name="entry">The selected entry, or null when there is no entry at this index.</param>
        /// <param name="includesLastOccurrence">Whether the response is in ERRSUM2 form, carrying the last occurrence date and time.</param>
        public ErrorSummary(int totalCount, ErrorSummaryEntry entry, bool includesLastOccurrence = false)
        {
            TotalCount = totalCount;
            Entry = entry;
            IncludesLastOccurrence = includesLastOccurrence;
        }
        public int TotalCount { get; }
        /// <summary>The selected entry, or null when the controller reports no entry at this index.</summary>
        public ErrorSummaryEntry Entry { get; }
        /// <summary>Whether this summary was read with ERRSUM2 and carries the last occurrence date and time.</summary>
        public bool IncludesLastOccurrence { get; }

        /// <summary>Formats the summary as an ERRSUM or ERRSUM2 response payload.</summary>
        public override string ToString()
        {
            var total = TotalCount.ToString(CultureInfo.InvariantCulture);
            if (Entry is null)
                return total + ";" + EmptyEntry + (IncludesLastOccurrence ? ";;" : "");
            var entry = Entry.ToString();
            if (IncludesLastOccurrence && Entry.LastOccurrence is null)
                entry += ";;";
            return total + ";" + entry;
        }
    }
}
