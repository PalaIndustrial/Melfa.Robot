namespace Melfa.Robot;

/// <summary>
/// <para>Maintenance Type, however <c>MFFCST=</c> and <c>MFTIME=</c> has different numbering for type.</para>
/// <para>We use one same enum for convinience.</para>
/// <list type=">"><c>MFFCST=</c>: Battery = 0, Grease = 1, Belt = 2</list>
/// <list type=">"><c>MFTIME=</c>: Battery = 1, Grease = 2, Belt = 3</list>
/// </summary>
public enum MaintenanceType
{
    Battery = 1,
    Grease = 2,
    Belt = 3,
}