using Melfa.Robot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Melfa.Robot;

public readonly partial struct PositionP : IPosition, IEquatable<PositionP>
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public double A { get; }
    public double B { get; }
    public double C { get; }
    public double L1 { get; }
    public double L2 { get; }
    public PositionConfig FLG1 { get; }
    public uint FLG2 { get; }

    public short Turn1 => GetTurnInternal(FLG2, 0);
    public short Turn2 => GetTurnInternal(FLG2, 1);
    public short Turn3 => GetTurnInternal(FLG2, 2);
    public short Turn4 => GetTurnInternal(FLG2, 3);
    public short Turn5 => GetTurnInternal(FLG2, 4);
    public short Turn6 => GetTurnInternal(FLG2, 5);
    public short Turn7 => GetTurnInternal(FLG2, 6);
    public short Turn8 => GetTurnInternal(FLG2, 7);

    public PositionP(double x = 0, double y = 0, double z = 0, double a = 0, double b = 0, double c = 0, double l1 = 0, double l2 = 0, PositionConfig flg1 = PositionConfig.LBF, uint flg2 = 0)
    {
        X = x;
        Y = y;
        Z = z;
        A = NormalizeAngleInternal(a);
        B = NormalizeAngleInternal(b);
        C = NormalizeAngleInternal(c);
        L1 = l1;
        L2 = l2;
        FLG1 = flg1;
        FLG2 = flg2;
    }

    public PositionP(string raw)
    {
        var match = RegexHelper.PositionXyzRegex().Match(raw ?? "");
        if (!match.Success)
            throw new PositionParseException(raw, typeof(PositionP));
        try
        {
            X = PositionHelper.DoubleFromString(match.Groups[nameof(X)].Value);
            Y = PositionHelper.DoubleFromString(match.Groups[nameof(Y)].Value);
            Z = PositionHelper.DoubleFromString(match.Groups[nameof(Z)].Value);
            A = NormalizeAngleInternal(PositionHelper.DoubleFromString(match.Groups[nameof(A)].Value));
            B = NormalizeAngleInternal(PositionHelper.DoubleFromString(match.Groups[nameof(B)].Value));
            C = NormalizeAngleInternal(PositionHelper.DoubleFromString(match.Groups[nameof(C)].Value));
            L1 = match.Groups[nameof(L1)].Success ? PositionHelper.DoubleFromString(match.Groups[nameof(L1)].Value) : double.NaN;
            L2 = match.Groups[nameof(L2)].Success ? PositionHelper.DoubleFromString(match.Groups[nameof(L2)].Value) : double.NaN;
            var flag = match.Groups[nameof(FLG1)].Value;
            FLG1 = char.IsLetter(flag[0])
                ? (PositionConfig)Enum.Parse(typeof(PositionConfig), flag, true)
                : (PositionConfig)checked((byte)PositionHelper.UInt32FromString(flag));
            FLG2 = PositionHelper.UInt32FromString(match.Groups[nameof(FLG2)].Value);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is ArgumentException)
        {
            throw new PositionParseException(raw, typeof(PositionP), ex);
        }
    }

    public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture,
        "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}) ({8}, {9})",
        PositionHelper.StringFromDouble("{0:0.00}", X),
        PositionHelper.StringFromDouble("{0:0.00}", Y),
        PositionHelper.StringFromDouble("{0:0.00}", Z),
        PositionHelper.StringFromDouble("{0:0.00}", A),
        PositionHelper.StringFromDouble("{0:0.00}", B),
        PositionHelper.StringFromDouble("{0:0.00}", C),
        PositionHelper.StringFromDouble("{0:0.00}", L1),
        PositionHelper.StringFromDouble("{0:0.00}", L2),
        (byte)FLG1,
        FLG2
    );

    private PositionP(Match pmatch, MatchCollection amatches) :
        this(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, PositionConfig.LBF, 0)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var match in amatches.Cast<Match>())
        {
            var axis = match.Groups["AXIS"].Value;
            if (!seen.Add(axis))
                throw new FormatException("Duplicate position axis.");
            var value = PositionHelper.DoubleFromString(match.Groups["VALUE"].Value);
            switch (axis)
            {
                case nameof(X): X = value; break;
                case nameof(Y): Y = value; break;
                case nameof(Z): Z = value; break;
                case nameof(A): A = NormalizeAngleInternal(value); break;
                case nameof(B): B = NormalizeAngleInternal(value); break;
                case nameof(C): C = NormalizeAngleInternal(value); break;
                case nameof(L1): L1 = value; break;
                case nameof(L2): L2 = value; break;
                default: throw new FormatException("Invalid position axis.");
            }
        }
        if (!new[] { nameof(X), nameof(Y), nameof(Z), nameof(A), nameof(B), nameof(C) }.All(seen.Contains))
            throw new FormatException("Required position axis is missing.");
        FLG1 = (PositionConfig)checked((byte)PositionHelper.UInt32FromString(pmatch.Groups["FLG1"].Value));
        FLG2 = PositionHelper.UInt32FromString(pmatch.Groups["FLG2"].Value);
    }

    //QokX;-400.94;Y;22.33;Z;985.71;A;-166.69;B;0.52;C;0.13;L1;0.00;L2;0.00;;6,0;100;0.00;00000000
    public static PositionP FromCommand(string raw)
    {
        var pmatch = RegexHelper.PositionXyzPosRegex().Match(raw ?? "");
        if (!pmatch.Success)
            throw new PositionParseException(raw, typeof(PositionP));
        try
        {
            PositionHelper.UInt32FromString(pmatch.Groups["OVRD"].Value);
            PositionHelper.DoubleFromString(pmatch.Groups["ENDSPEED"].Value);
            var amatches = RegexHelper.PositionXyzAxisRegex().Matches(pmatch.Groups["POS"].Value);
            return new PositionP(pmatch, amatches);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException)
        {
            throw new PositionParseException(raw, typeof(PositionP), ex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short GetTurnInternal(uint flg, int axis)
    {
        // Each FLG2 nibble is a signed 4-bit (two's complement) turn count.
        var n = (int)((flg >> (axis * 4)) & 0x0F);
        return (short)(n >= 8 ? n - 16 : n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double NormalizeAngleInternal(double angle)
    {
        if (double.IsNaN(angle) || double.IsInfinity(angle))
            return angle;
        angle %= 360;
        if (angle >= 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;
        return angle;
    }

    public static bool operator ==(PositionP left, PositionP right) => left.Equals(right);

    public static bool operator !=(PositionP left, PositionP right) => !(left == right);

    public override bool Equals(object obj) => obj is PositionP other && Equals(other);

    public bool Equals(PositionP other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) &&
        A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) &&
        L1.Equals(other.L1) && L2.Equals(other.L2) &&
        FLG1 == other.FLG1 && FLG2 == other.FLG2;

    public override int GetHashCode() =>
        HashCode.Combine(
            HashCode.Combine(X, Y, Z),
            HashCode.Combine(A, B, C),
            HashCode.Combine(L1, L2),
            HashCode.Combine(FLG1, FLG2)
        );
}