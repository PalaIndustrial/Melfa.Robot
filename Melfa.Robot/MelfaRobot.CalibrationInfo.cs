namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>Installation status returned by CALIB.</summary>
    public readonly struct CalibrationInfo
    {
        internal CalibrationInfo(bool isDefined, byte completedAxisMask, string reserved)
        {
            IsDefined = isDefined;
            CompletedAxisMask = completedAxisMask;
            Reserved = reserved;
        }

        /// <summary>Whether the controller reports installation as defined.</summary>
        public bool IsDefined { get; }
        /// <summary>Completed axes; bits 0-7 correspond to J1-J8.</summary>
        public byte CompletedAxisMask { get; }
        /// <summary>Any remaining reserved response data, retained as returned by the controller.</summary>
        public string Reserved { get; }
    }
}
