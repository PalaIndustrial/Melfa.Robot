namespace Melfa.Robot;

public enum ErrorSeverity : sbyte
{
    /// <summary>All levels, for error history filtering or clearing.</summary>
    All = 0,
    High = 1,
    Low = 2,
    Caution = 3,
    Warning = 3,
    Message = sbyte.MaxValue,
    Unknown = -1,
}