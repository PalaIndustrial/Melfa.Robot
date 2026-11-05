using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    private static string FormatServoNumber(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    /// <summary>One axis of an SRVENC response.</summary>
    public readonly struct ServoEncoderAxis : IEquatable<ServoEncoderAxis>
    {
        public ServoEncoderAxis(long positionFeedback, long positionInOneRotation, long fdtCommand, long positionCommand)
        {
            PositionFeedback = positionFeedback;
            PositionInOneRotation = positionInOneRotation;
            FdtCommand = fdtCommand;
            PositionCommand = positionCommand;
        }

        /// <summary>Encoder position feedback.</summary>
        public long PositionFeedback { get; }
        /// <summary>Position within one motor rotation.</summary>
        public long PositionInOneRotation { get; }
        /// <summary>Fdt command.</summary>
        public long FdtCommand { get; }
        /// <summary>Position command.</summary>
        public long PositionCommand { get; }

        public bool Equals(ServoEncoderAxis other) =>
            PositionFeedback == other.PositionFeedback && PositionInOneRotation == other.PositionInOneRotation &&
            FdtCommand == other.FdtCommand && PositionCommand == other.PositionCommand;
        public override bool Equals(object obj) => obj is ServoEncoderAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PositionFeedback, PositionInOneRotation, FdtCommand, PositionCommand);
        public static bool operator ==(ServoEncoderAxis left, ServoEncoderAxis right) => left.Equals(right);
        public static bool operator !=(ServoEncoderAxis left, ServoEncoderAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its four SRVENC response fields.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3}",
            PositionFeedback, PositionInOneRotation, FdtCommand, PositionCommand);
    }

    /// <summary>One axis of an SRVDRP response.</summary>
    public readonly struct ServoDroopAxis : IEquatable<ServoDroopAxis>
    {
        public ServoDroopAxis(long positionDroop, long maximumPositionDroop)
        {
            PositionDroop = positionDroop;
            MaximumPositionDroop = maximumPositionDroop;
        }

        public long PositionDroop { get; }
        public long MaximumPositionDroop { get; }

        public bool Equals(ServoDroopAxis other) =>
            PositionDroop == other.PositionDroop && MaximumPositionDroop == other.MaximumPositionDroop;
        public override bool Equals(object obj) => obj is ServoDroopAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PositionDroop, MaximumPositionDroop);
        public static bool operator ==(ServoDroopAxis left, ServoDroopAxis right) => left.Equals(right);
        public static bool operator !=(ServoDroopAxis left, ServoDroopAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its two SRVDRP response fields.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1}",
            PositionDroop, MaximumPositionDroop);
    }

    /// <summary>One axis of an SRVSPD response.</summary>
    public readonly struct ServoSpeedAxis : IEquatable<ServoSpeedAxis>
    {
        public ServoSpeedAxis(long speedFeedback, long maximumSpeed, long speedCommand)
        {
            SpeedFeedback = speedFeedback;
            MaximumSpeed = maximumSpeed;
            SpeedCommand = speedCommand;
        }

        public long SpeedFeedback { get; }
        public long MaximumSpeed { get; }
        public long SpeedCommand { get; }

        public bool Equals(ServoSpeedAxis other) =>
            SpeedFeedback == other.SpeedFeedback && MaximumSpeed == other.MaximumSpeed && SpeedCommand == other.SpeedCommand;
        public override bool Equals(object obj) => obj is ServoSpeedAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(SpeedFeedback, MaximumSpeed, SpeedCommand);
        public static bool operator ==(ServoSpeedAxis left, ServoSpeedAxis right) => left.Equals(right);
        public static bool operator !=(ServoSpeedAxis left, ServoSpeedAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its three SRVSPD response fields.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}",
            SpeedFeedback, MaximumSpeed, SpeedCommand);
    }

    /// <summary>One axis of an SRVCUR0 (current command) response.</summary>
    public readonly struct ServoCurrentCommandAxis : IEquatable<ServoCurrentCommandAxis>
    {
        public ServoCurrentCommandAxis(double currentCommand, double tolerableCommandPositive, double tolerableCommandNegative, double rmsCurrent)
        {
            CurrentCommand = currentCommand;
            TolerableCommandPositive = tolerableCommandPositive;
            TolerableCommandNegative = tolerableCommandNegative;
            RmsCurrent = rmsCurrent;
        }

        public double CurrentCommand { get; }
        public double TolerableCommandPositive { get; }
        public double TolerableCommandNegative { get; }
        public double RmsCurrent { get; }

        public bool Equals(ServoCurrentCommandAxis other) =>
            CurrentCommand.Equals(other.CurrentCommand) && TolerableCommandPositive.Equals(other.TolerableCommandPositive) &&
            TolerableCommandNegative.Equals(other.TolerableCommandNegative) && RmsCurrent.Equals(other.RmsCurrent);
        public override bool Equals(object obj) => obj is ServoCurrentCommandAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(CurrentCommand, TolerableCommandPositive, TolerableCommandNegative, RmsCurrent);
        public static bool operator ==(ServoCurrentCommandAxis left, ServoCurrentCommandAxis right) => left.Equals(right);
        public static bool operator !=(ServoCurrentCommandAxis left, ServoCurrentCommandAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its four SRVCUR0 response fields.</summary>
        public override string ToString() => string.Join(";", FormatServoNumber(CurrentCommand),
            FormatServoNumber(TolerableCommandPositive), FormatServoNumber(TolerableCommandNegative), FormatServoNumber(RmsCurrent));
    }

    /// <summary>One axis of an SRVCUR1 (current feedback) response.</summary>
    public readonly struct ServoCurrentFeedbackAxis : IEquatable<ServoCurrentFeedbackAxis>
    {
        public ServoCurrentFeedbackAxis(double currentFeedback, double maximumCurrentCommand1, double maximumCurrentCommand2)
        {
            CurrentFeedback = currentFeedback;
            MaximumCurrentCommand1 = maximumCurrentCommand1;
            MaximumCurrentCommand2 = maximumCurrentCommand2;
        }

        public double CurrentFeedback { get; }
        public double MaximumCurrentCommand1 { get; }
        public double MaximumCurrentCommand2 { get; }

        public bool Equals(ServoCurrentFeedbackAxis other) =>
            CurrentFeedback.Equals(other.CurrentFeedback) && MaximumCurrentCommand1.Equals(other.MaximumCurrentCommand1) &&
            MaximumCurrentCommand2.Equals(other.MaximumCurrentCommand2);
        public override bool Equals(object obj) => obj is ServoCurrentFeedbackAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(CurrentFeedback, MaximumCurrentCommand1, MaximumCurrentCommand2);
        public static bool operator ==(ServoCurrentFeedbackAxis left, ServoCurrentFeedbackAxis right) => left.Equals(right);
        public static bool operator !=(ServoCurrentFeedbackAxis left, ServoCurrentFeedbackAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its three SRVCUR1 response fields.</summary>
        public override string ToString() => string.Join(";", FormatServoNumber(CurrentFeedback),
            FormatServoNumber(MaximumCurrentCommand1), FormatServoNumber(MaximumCurrentCommand2));
    }

    /// <summary>One axis of an SRVLCR response.</summary>
    public readonly struct ServoLoadCurrentAxis : IEquatable<ServoLoadCurrentAxis>
    {
        public ServoLoadCurrentAxis(double loadLevel, double maximumLoadLevel)
        {
            LoadLevel = loadLevel;
            MaximumLoadLevel = maximumLoadLevel;
        }

        public double LoadLevel { get; }
        public double MaximumLoadLevel { get; }

        public bool Equals(ServoLoadCurrentAxis other) =>
            LoadLevel.Equals(other.LoadLevel) && MaximumLoadLevel.Equals(other.MaximumLoadLevel);
        public override bool Equals(object obj) => obj is ServoLoadCurrentAxis other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(LoadLevel, MaximumLoadLevel);
        public static bool operator ==(ServoLoadCurrentAxis left, ServoLoadCurrentAxis right) => left.Equals(right);
        public static bool operator !=(ServoLoadCurrentAxis left, ServoLoadCurrentAxis right) => !left.Equals(right);

        /// <summary>Formats the axis as its two SRVLCR response fields.</summary>
        public override string ToString() => FormatServoNumber(LoadLevel) + ";" + FormatServoNumber(MaximumLoadLevel);
    }

    /// <summary>Main-circuit power supply readings returned by SRVVOL.</summary>
    public readonly struct ServoVoltageInfo : IEquatable<ServoVoltageInfo>
    {
        public ServoVoltageInfo(double powerVoltage, double maximumPowerVoltage, double minimumPowerVoltage,
            ImmutableArray<double> regenerationLevels)
        {
            PowerVoltage = powerVoltage;
            MaximumPowerVoltage = maximumPowerVoltage;
            MinimumPowerVoltage = minimumPowerVoltage;
            RegenerationLevels = regenerationLevels;
        }

        public double PowerVoltage { get; }
        public double MaximumPowerVoltage { get; }
        public double MinimumPowerVoltage { get; }
        /// <summary>Regeneration levels for J1-J8.</summary>
        public ImmutableArray<double> RegenerationLevels { get; }

        private ImmutableArray<double> Levels => RegenerationLevels.IsDefault ? ImmutableArray<double>.Empty : RegenerationLevels;

        /// <remarks>Regeneration levels are compared element by element.</remarks>
        public bool Equals(ServoVoltageInfo other) =>
            PowerVoltage.Equals(other.PowerVoltage) && MaximumPowerVoltage.Equals(other.MaximumPowerVoltage) &&
            MinimumPowerVoltage.Equals(other.MinimumPowerVoltage) && Levels.SequenceEqual(other.Levels);
        public override bool Equals(object obj) => obj is ServoVoltageInfo other && Equals(other);
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(PowerVoltage);
            hash.Add(MaximumPowerVoltage);
            hash.Add(MinimumPowerVoltage);
            foreach (var level in Levels)
                hash.Add(level);
            return hash.ToHashCode();
        }
        public static bool operator ==(ServoVoltageInfo left, ServoVoltageInfo right) => left.Equals(right);
        public static bool operator !=(ServoVoltageInfo left, ServoVoltageInfo right) => !left.Equals(right);

        /// <summary>Formats the readings as an SRVVOL response payload.</summary>
        public override string ToString() => string.Join(";",
            new[] { PowerVoltage, MaximumPowerVoltage, MinimumPowerVoltage }.Concat(Levels).Select(FormatServoNumber));
    }
}
