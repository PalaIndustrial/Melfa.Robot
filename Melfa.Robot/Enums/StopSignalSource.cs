using System;

namespace Melfa.Robot;

/// <summary>Stop signal sources reported by STPSIG.</summary>
[Flags]
public enum StopSignalSource : uint
{
    None = 0,
    TeachingBox = 0x01,
    PersonalComputer = 0x02,
    InputOutput = 0x04,
    OperationPanel = 0x08,
}