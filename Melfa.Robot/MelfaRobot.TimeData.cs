using System.Collections.Immutable;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>Hour-meter values reported by PTIME.</summary>
    public sealed class HourMeterData
    {
        private readonly string _protocolPayload;

        internal HourMeterData(long powerOn, long allServoOn, ImmutableArray<long> mechanismServoOn,
            ImmutableArray<long> reserved, long programOperation, long reservedAfterProgram,
            long batteryAccumulation, long batteryRemaining, long mechanismInformation, string protocolPayload)
        {
            _protocolPayload = protocolPayload;
            PowerOnHours = powerOn;
            AllMechanismsServoOnHours = allServoOn;
            MechanismServoOnHours = mechanismServoOn;
            ReservedFields = reserved;
            ProgramOperationHours = programOperation;
            ReservedAfterProgram = reservedAfterProgram;
            BatteryAccumulationHours = batteryAccumulation;
            BatteryRemainingHours = batteryRemaining;
            MechanismInformation = mechanismInformation;
        }

        public long PowerOnHours { get; }
        public long AllMechanismsServoOnHours { get; }
        /// <summary>Servo-on hours for mechanisms 1 through 3.</summary>
        public ImmutableArray<long> MechanismServoOnHours { get; }
        /// <summary>Reserved fields between mechanism and program hours.</summary>
        public ImmutableArray<long> ReservedFields { get; }
        public long ProgramOperationHours { get; }
        public long ReservedAfterProgram { get; }
        public long BatteryAccumulationHours { get; }
        public long BatteryRemainingHours { get; }
        /// <summary>Mechanism mask; bits 0-2 represent mechanisms 1-3.</summary>
        public long MechanismInformation { get; }
        public override string ToString() => _protocolPayload;
    }

    /// <summary>Program operation and cycle measurements reported by CYCLETIME.</summary>
    public sealed class CycleTimeData
    {
        private readonly string _protocolPayload;

        internal CycleTimeData(long operation, long latest, long average, long count, string protocolPayload)
        {
            _protocolPayload = protocolPayload;
            OperationMilliseconds = operation;
            LatestCycleMilliseconds = latest;
            AverageCycleMilliseconds = average;
            CycleCount = count;
        }

        public long OperationMilliseconds { get; }
        public long LatestCycleMilliseconds { get; }
        public long AverageCycleMilliseconds { get; }
        public long CycleCount { get; }
        public override string ToString() => _protocolPayload;
    }
}
