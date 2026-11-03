using System;

namespace Melfa.Robot;

/// <summary>Emergency-stop signal sources reported by STPSIG.</summary>
[Flags]
public enum EmergencyStopSource : uint
{
    None = 0,
    InputOutput = 0x01,
    OperationPanel = 0x02,
    TeachingBox = 0x04,
}
