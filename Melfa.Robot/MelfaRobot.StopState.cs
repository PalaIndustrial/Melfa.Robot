namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>Run and stop status returned by DSTATE for one task slot.</summary>
    public readonly struct DetailedState
    {
        internal DetailedState(int slot, RunStatus run, StopStatus stop, int error, int step, int mechanism)
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
    }

    /// <summary>Stop and emergency-stop signal masks returned by STPSIG.</summary>
    public readonly struct StopSignalState
    {
        internal StopSignalState(StopSignalSource stop, EmergencyStopSource emergencyStop)
        {
            Stop = stop;
            EmergencyStop = emergencyStop;
        }

        public StopSignalSource Stop { get; }
        public EmergencyStopSource EmergencyStop { get; }
    }
}
