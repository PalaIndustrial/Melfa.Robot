using System;
using System.Text.RegularExpressions;

namespace Melfa.Robot;

internal static partial class RegexHelper
{
    private const string ErrorRegexPattern = @"(?'ErrorNumber'\d+);(?'ErrorLevel'\d+)";

    private const string InfoRegexPattern =
        @"(?'P_AXIS_PATTERN'[0-9a-fA-F]{2});" +
        @"(?'J_AXIS_PATTERN'[0-9a-fA-F]{2});" +
        @"(?'FLG1'\d+),(?'FLG2'\d+);" +
        @"(?'JSPD1'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD2'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD3'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD4'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD5'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD6'[0-9a-fA-F]{1,2})," +
        @"(?'JSPD7'[0-9a-fA-F]{1,2});" +
        @"(?'PROG_EXT'[^;]*);" +
        @"(?'PRM_EXT'[^;]*);" +
        @"(?'RobotType'[^;]*);" +
        @"(?'Controller'[^;]*);" +
        @"(?'Series'[^;]*);" +
        @"(?'DATE_YEAR'[0-9]+)-(?'DATE_MONTH'[0-9]+)-(?'DATE_DAY'[0-9]+);" +
        @"(?'Version'[^;]*);" +
        @"(?'Language'[^;]*);" +
        @"(?'Copyright'[^;]*);" +
        @"(?'ROBOT_INFO'\d+);" +
        @"(?'SerialNumber'[^;]*);" +
        @"(?'TASKMAX'\d+);";

    private const string RunStateRegexPattern =
        @"(?'ProgramName'[^;]*);" +
        @"(?'LineNumber'\d+);" +
        @"(?'Override'\d+);" +
        @"(?'EditStatus'\d+);" +
        @"(?'RunStatus'[0-9a-fA-F]{2})" +
        @"(?'StopStatus'[0-9a-fA-F]{2})" +
        @"(?'ErrorNumber'\d+);(?'StepNumber'\d+);" +
        @"(?'MechInfo'\d+);{8}" +
        @"(?'TaskProgramName'[^;]*);" +
        @"(?'TaskMode'\w+);" +
        @"(?'TaskCondition'\w+);" +
        @"(?'TaskPriority'\d+);" +
        @"(?'MechNumber'\d+)";

    private const string HandStateRegexPattern = @"(?'SignalNumber'-?\d+);(?'State'-?\d+);(?'HandType'-?\d+)";

    private const string PositionNumberPattern = @"[-+]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][-+]?[0-9]+)?";
    private const string PositionValuePattern = @"(?:" + PositionNumberPattern + @"|[nN][aA][nN]|[-+]?[iI][nN][fF])";
    private const string PositionJointRegexPattern =
        @"\A\s*\(" +
            @"\s*(?'J1'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J2'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J3'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J4'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J5'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J6'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J7'" + PositionValuePattern + @")\s*," +
            @"\s*(?'J8'" + PositionValuePattern + @")\s*" +
        @"\)\s*\z";
    private const string PositionJointPosRegexPattern = @"\A(?i:Qok)?(?'POS'(?:J[1-8];" + PositionNumberPattern + @";){6,8});?\*{4},\*{4};(?'OVRD'[0-9]+);(?'ENDSPEED'" + PositionNumberPattern + @");(?'LIMIT'[0-9a-fA-F]{8})\z";
    private const string PositionJointAxisRegexPattern = @"(?'AXIS'J[1-8]);(?'VALUE'" + PositionNumberPattern + @");";

    private const string PositionXyzRegexPattern =
        @"\A\s*\(" +
            @"\s*(?'X'" + PositionValuePattern + @")\s*," +
            @"\s*(?'Y'" + PositionValuePattern + @")\s*," +
            @"\s*(?'Z'" + PositionValuePattern + @")\s*," +
            @"\s*(?'A'" + PositionValuePattern + @")\s*," +
            @"\s*(?'B'" + PositionValuePattern + @")\s*," +
            @"\s*(?'C'" + PositionValuePattern + @")\s*," +
            @"\s*(?'L1'" + PositionValuePattern + @")\s*," +
            @"\s*(?'L2'" + PositionValuePattern + @")\s*" +
        @"\)\s*\(" +
            @"\s*(?'FLG1'(?:0[xX][0-9a-fA-F]+|[0-9]+|[lLrR][bBaA][fFnN]))\s*," +
            @"\s*(?'FLG2'(?:0[xX][0-9a-fA-F]+|[0-9]+))\s*" +
        @"\)\s*\z";
    private const string PositionXyzPosRegexPattern = @"\A(?i:Qok)?(?'POS'(?:(?:X|Y|Z|A|B|C|L1|L2);" + PositionNumberPattern + @";){6,8});?(?'FLG1'[0-9]+),(?'FLG2'[0-9]+);(?'OVRD'[0-9]+);(?'ENDSPEED'" + PositionNumberPattern + @");(?'LIMIT'[0-9a-fA-F]{8})\z";
    private const string PositionXyzAxisRegexPattern = @"(?'AXIS'(?:X|Y|Z|A|B|C|L1|L2));(?'VALUE'" + PositionNumberPattern + @");";

#if NET8_0_OR_GREATER
    [GeneratedRegex(ErrorRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex ErrorRegex();

    [GeneratedRegex(InfoRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex InfoRegex();

    [GeneratedRegex(RunStateRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex RunStateRegex();

    [GeneratedRegex(HandStateRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex HandStatesRegex();

    [GeneratedRegex(PositionJointRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionJointRegex();
    [GeneratedRegex(PositionJointPosRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionJointPosRegex();
    [GeneratedRegex(PositionJointAxisRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionJointAxisRegex();

    [GeneratedRegex(PositionXyzRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionXyzRegex();

    [GeneratedRegex(PositionXyzPosRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionXyzPosRegex();
    [GeneratedRegex(PositionXyzAxisRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PositionXyzAxisRegex();
#else
    private static readonly Lazy<Regex> _ErrorRegex = new(() => new Regex(ErrorRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex ErrorRegex() => _ErrorRegex.Value;

    private static readonly Lazy<Regex> _HandStateRegex = new(() => new Regex(HandStateRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex HandStatesRegex() => _HandStateRegex.Value;

    private static readonly Lazy<Regex> _InfoRegex = new(() => new Regex(InfoRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex InfoRegex() => _InfoRegex.Value;

    private static readonly Lazy<Regex> _RunStateRegex = new(() => new Regex(RunStateRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex RunStateRegex() => _RunStateRegex.Value;

    private static readonly Lazy<Regex> _PositionJointRegex = new(() => new Regex(PositionJointRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionJointRegex() => _PositionJointRegex.Value;

    private static readonly Lazy<Regex> _PositionJointPosRegex = new(() => new Regex(PositionJointPosRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionJointPosRegex() => _PositionJointPosRegex.Value;

    private static readonly Lazy<Regex> _PositionJointAxisRegex = new(() => new Regex(PositionJointAxisRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionJointAxisRegex() => _PositionJointAxisRegex.Value;

    private static readonly Lazy<Regex> _PositionXyzRegex = new(() => new Regex(PositionXyzRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionXyzRegex() => _PositionXyzRegex.Value;

    private static readonly Lazy<Regex> _PositionXyzPosRegex = new(() => new Regex(PositionXyzPosRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionXyzPosRegex() => _PositionXyzPosRegex.Value;

    private static readonly Lazy<Regex> _PositionXyzAxisRegex = new(() => new Regex(PositionXyzAxisRegexPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled));
    public static Regex PositionXyzAxisRegex() => _PositionXyzAxisRegex.Value;
#endif
}