using Melfa.Robot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melfa.Robot;

public partial struct PositionJ : IPosition, IEquatable<PositionJ>
{
    public double J1 { get; }
    public double J2 { get; }
    public double J3 { get; }
    public double J4 { get; }
    public double J5 { get; }
    public double J6 { get; }
    public double J7 { get; }
    public double J8 { get; }

    public PositionJ(double j1 = 0, double j2 = 0, double j3 = 0, double j4 = 0, double j5 = 0, double j6 = 0, double j7 = 0, double j8 = 0)
    {
        J1 = j1;
        J2 = j2;
        J3 = j3;
        J4 = j4;
        J5 = j5;
        J6 = j6;
        J7 = j7;
        J8 = j8;
    }

    public PositionJ(string raw)
    {
        var match = RegexHelper.PositionJointRegex().Match(raw ?? "");
        if (!match.Success)
            throw new PositionParseException(raw, typeof(PositionJ));
        try
        {
            J1 = PositionHelper.DoubleFromString(match.Groups[nameof(J1)].Value);
            J2 = PositionHelper.DoubleFromString(match.Groups[nameof(J2)].Value);
            J3 = PositionHelper.DoubleFromString(match.Groups[nameof(J3)].Value);
            J4 = PositionHelper.DoubleFromString(match.Groups[nameof(J4)].Value);
            J5 = PositionHelper.DoubleFromString(match.Groups[nameof(J5)].Value);
            J6 = PositionHelper.DoubleFromString(match.Groups[nameof(J6)].Value);
            J7 = PositionHelper.DoubleFromString(match.Groups[nameof(J7)].Value);
            J8 = PositionHelper.DoubleFromString(match.Groups[nameof(J8)].Value);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException)
        {
            throw new PositionParseException(raw, typeof(PositionJ), ex);
        }
    }

    public PositionJ(MatchCollection matches) :
        this(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches));
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in matches.Cast<Match>())
        {
            var axis = m.Groups["AXIS"].Value;
            if (!seen.Add(axis))
                throw new PositionParseException(m.Value, typeof(PositionJ));
            double value;
            try { value = PositionHelper.DoubleFromString(m.Groups["VALUE"].Value); }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            { throw new PositionParseException(m.Value, typeof(PositionJ), ex); }
            switch (axis)
            {
                case nameof(J1): J1 = value; break;
                case nameof(J2): J2 = value; break;
                case nameof(J3): J3 = value; break;
                case nameof(J4): J4 = value; break;
                case nameof(J5): J5 = value; break;
                case nameof(J6): J6 = value; break;
                case nameof(J7): J7 = value; break;
                case nameof(J8): J8 = value; break;
                default: throw new PositionParseException(m.Value, typeof(PositionJ));
            }
        }
    }

    /// <summary>
    /// MB6 hates "NaN", for undefined Axis, write "0.0" instead.
    /// </summary>
    /// <returns>a string represation of the position which MB6 won't complain about</returns>
    public override string ToString() => string.Format(
        "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J1),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J2),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J3),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J4),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J5),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J6),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J7),
        PositionHelper.StringFromDoubleMB6("{0:0.00}", J8)
    );

    //1;1;JPOSF
    //QokJ1;0.00;J2;-0.00;J3;89.99;J4;-166.69;J5;-90.54;J6;-0.00;J7;0.00;J8;0.00;;****,****;100;0.00;00000000
    public static PositionJ FromCommand(string raw)
    {
        var match = RegexHelper.PositionJointPosRegex().Match(raw ?? "");
        if (!match.Success)
            throw new PositionParseException(raw, typeof(PositionJ));
        try
        {
            PositionHelper.UInt32FromString(match.Groups["OVRD"].Value);
            PositionHelper.DoubleFromString(match.Groups["ENDSPEED"].Value);
            var axes = RegexHelper.PositionJointAxisRegex().Matches(match.Groups["POS"].Value);
            var names = new HashSet<string>(axes.Cast<Match>().Select(axis => axis.Groups["AXIS"].Value), StringComparer.Ordinal);
            if (!new[] { nameof(J1), nameof(J2), nameof(J3), nameof(J4), nameof(J5), nameof(J6) }.All(names.Contains))
                throw new FormatException("Required joint axis is missing.");
            return new PositionJ(axes);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is PositionParseException)
        {
            throw new PositionParseException(raw, typeof(PositionJ), ex);
        }
    }

    public static bool operator ==(PositionJ left, PositionJ right) => left.Equals(right);

    public static bool operator !=(PositionJ left, PositionJ right) => !(left == right);

    public override bool Equals(object obj) => obj is PositionJ other && Equals(other);

    public bool Equals(PositionJ other) =>
        J1.Equals(other.J1) && J2.Equals(other.J2) && J3.Equals(other.J3) &&
        J4.Equals(other.J4) && J5.Equals(other.J5) && J6.Equals(other.J6) &&
        J7.Equals(other.J7) && J8.Equals(other.J8);

    public override int GetHashCode() => HashCode.Combine(J1, J2, J3, J4, J5, J6, J7, J8);
}