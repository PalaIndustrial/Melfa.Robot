using System;
using System.ComponentModel;

namespace Melfa.Robot.Helpers;

internal static class EnumExtensions
{
    public static string ToCommandString(this HourMeterKind kind) => kind switch
    {
        HourMeterKind.PowerOnTime => "T",
        HourMeterKind.ServoOnTime => "S",
        HourMeterKind.ProgramOperationTime => "D",
        HourMeterKind.BatteryAccumulationTime => "B",
        HourMeterKind.All => "Z",
        _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(HourMeterKind)),
    };

    public static string ToCommandString(this StepMethod method) => method switch
    {
        StepMethod.Forward => "1",
        StepMethod.Continuous => "R",
        StepMethod.Backward => "B",
        StepMethod.Stop => "S",
        _ => throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(StepMethod)),
    };

    public static string ToProgramLocationCommandString(this LocationType type) => type switch
    {
        LocationType.Top => "TOP",
        LocationType.Bottom => "END",
        LocationType.Next => "+1",
        LocationType.Previous => "-1",
        _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LocationType)),
    };

    public static string ToVariablePositionCommandString(this LocationType position) => position switch
    {
        LocationType.Top => "<",
        LocationType.Bottom => ">",
        LocationType.Next => "+1",
        LocationType.Previous => "-1",
        _ => throw new ArgumentOutOfRangeException(nameof(position)),
    };

    public static string ToCommandString(this VariableClass variableClass) => variableClass switch
    {
        VariableClass.Integer => "M",
        VariableClass.Character => "C",
        VariableClass.CartesianPosition => "P",
        VariableClass.JointPosition => "J",
        _ => throw new InvalidEnumArgumentException(nameof(variableClass), (int)variableClass, typeof(VariableClass)),
    };

    public static string ToCommandString(this TaskMode mode) => mode switch
    {
        TaskMode.Repeat => "REP",
        TaskMode.Cycle => "CYC",
        TaskMode.Unknown or _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(TaskMode)),
    };

    public static string ToCommandString(this TaskCondition condition) => condition switch
    {
        TaskCondition.Start => "START",
        TaskCondition.Always => "ALWAYS",
        TaskCondition.Error => "ERROR",
        TaskCondition.Unknown or _ => throw new InvalidEnumArgumentException(nameof(condition), (int)condition, typeof(TaskCondition)),
    };
}