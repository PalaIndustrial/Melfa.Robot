using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    private const string MaintenanceResetDateFormat = "yyyy'/'MM'/'dd";
    private const string MaintenanceResetTimeFormat = "HH':'mm':'ss";

    /// <summary>Maintenance forecast reset date and count of one axis.</summary>
    public readonly struct MaintenanceResetAxis : IEquatable<MaintenanceResetAxis>
    {
        public MaintenanceResetAxis(DateTime? resetDate, int resetCount)
        {
            ResetDate = resetDate;
            ResetCount = resetCount;
        }

        /// <summary>Controller-local date and time of the last reset, or <c>null</c> if the controller reports none.</summary>
        public DateTime? ResetDate { get; }
        /// <summary>Number of resets.</summary>
        public int ResetCount { get; }

        public bool Equals(MaintenanceResetAxis other) => ResetDate == other.ResetDate && ResetCount == other.ResetCount;
        public override bool Equals(object obj) => obj is MaintenanceResetAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ResetDate, ResetCount);
        public static bool operator ==(MaintenanceResetAxis left, MaintenanceResetAxis right) => left.Equals(right);
        public static bool operator !=(MaintenanceResetAxis left, MaintenanceResetAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its reset date, time and count fields.</summary>
        public override string ToString() => $"{FormatResetDate()};{ResetCount.ToString(CultureInfo.InvariantCulture)}";

        internal string FormatResetDate() => ResetDate is { } date
            ? date.ToString(MaintenanceResetDateFormat, CultureInfo.InvariantCulture) + ";" +
              date.ToString(MaintenanceResetTimeFormat, CultureInfo.InvariantCulture)
            : ";";
    }

    /// <summary>Maintenance forecast reset dates and counts reported by MFTIME=.</summary>
    public sealed class MaintenanceResetData
    {
        public MaintenanceResetData(IEnumerable<MaintenanceResetAxis> axes)
        {
            if (axes == null)
                throw new ArgumentNullException(nameof(axes));
            Axes = axes.ToImmutableArray();
            if (Axes.Length != 8)
                throw new ArgumentException("Expected exactly 8 axes.", nameof(axes));
        }

        /// <summary>Reset data for J1 through J8.</summary>
        public ImmutableArray<MaintenanceResetAxis> Axes { get; }

        /// <summary>Formats the data as an MFTIME= response payload: eight dates and times, then eight counts.</summary>
        public override string ToString() => string.Join(";",
            Axes.Select(axis => axis.FormatResetDate())
                .Concat(Axes.Select(axis => axis.ResetCount.ToString(CultureInfo.InvariantCulture))));
    }
}
