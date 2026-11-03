namespace Melfa.Robot;

/// <summary>Variable classes supported by the R-protocol.</summary>
public enum VariableClass : byte
{
    /// <summary>[M] Integer variable.</summary>
    Integer = 1,
    /// <summary>[C] Character variable.</summary>
    Character = 2,
    /// <summary>[P] Cartesian position variable.</summary>
    CartesianPosition = 3,
    /// <summary>[J] Joint position variable.</summary>
    JointPosition = 4,
}
