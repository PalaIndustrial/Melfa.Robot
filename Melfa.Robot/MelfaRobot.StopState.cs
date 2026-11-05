using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>Run and stop status returned by DSTATE for one task slot.</summary>
    public readonly struct DetailedState
    {
        public DetailedState(int slot, RunStatus run, StopStatus stop, int error, int step, int mechanism)
        {
            TaskSlot = slot;
            RunStatus = run;
            StopStatus = stop;
            ErrorNumber = error;
            StepNumber = step;
            MechanismNumber = mechanism;
        }

        public int TaskSlot { get; }
        public RunStatus RunStatus { get; }
        public StopStatus StopStatus { get; }
        /// <summary>Error number, or zero when no error is reported.</summary>
        public int ErrorNumber { get; }
        public int StepNumber { get; }
        public int MechanismNumber { get; }

        /// <summary>Formats the state as a DSTATE response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2};{3};{4}",
            (byte)RunStatus, (byte)StopStatus, ErrorNumber, StepNumber, MechanismNumber);
    }

    /// <summary>Stop and emergency-stop signal masks returned by STPSIG.</summary>
    public readonly struct StopSignalState
    {
        public StopSignalState(StopSignalSource stop, EmergencyStopSource emergencyStop)
        {
            Stop = stop;
            EmergencyStop = emergencyStop;
        }

        public StopSignalSource Stop { get; }
        public EmergencyStopSource EmergencyStop { get; }

        /// <summary>Formats the masks as an STPSIG response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0:X};{1:X}",
            (uint)Stop, (uint)EmergencyStop);
    }
}
