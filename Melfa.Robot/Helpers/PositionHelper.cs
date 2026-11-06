using CommunityToolkit.Diagnostics;
using System;
using System.Globalization;

namespace Melfa.Robot.Helpers;

internal static class PositionHelper
{
    public static double DoubleFromString(string value)
    {
        Guard.IsNotNull(value);
        switch (value.ToLowerInvariant())
        {
            case "nan": return double.NaN;
            case "inf": return double.PositiveInfinity;
            case "+inf": return double.PositiveInfinity;
            case "-inf": return double.NegativeInfinity;
            default:
                var result = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (double.IsInfinity(result))
                    throw new OverflowException("Position value is outside the range of a double.");
                return result;
        }
    }

    public static uint UInt32FromString(string value)
    {
        Guard.IsNotNull(value);
        return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?
            uint.Parse(value.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) :
            uint.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static string StringFromDouble(string format, double value)
    {
        if (double.IsNaN(value))
            return "NaN";
        if (double.IsPositiveInfinity(value))
            return "Inf";
        if (double.IsNegativeInfinity(value))
            return "-Inf";
        return string.Format(CultureInfo.InvariantCulture, format, value);
    }

    public static string StringFromDoubleMB6(string format, double value)
    {
        if (double.IsNaN(value) || double.IsPositiveInfinity(value) || double.IsNegativeInfinity(value))
            return "0.0";
        return string.Format(CultureInfo.InvariantCulture, format, value);
    }
}