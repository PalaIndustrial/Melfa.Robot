using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Xunit;

namespace Melfa.Robot.Tests;

internal static class ScratchOperations
{
    internal static void WithCleanup(Action body, Action cleanup)
    {
        Exception? error = null;
        try { body(); }
        catch (Exception ex) { error = ex; }
        try { cleanup(); }
        catch (Exception ex)
        {
            if (error is not null)
                throw new AggregateException("Operation and cleanup both failed; robot state is uncertain.", error, ex);
            throw new InvalidOperationException("Cleanup failed; robot state is uncertain.", ex);
        }
        if (error is not null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }

    internal static void WithScratch(IRobotCommands robot, SafetyConfig config, int taskCount, string extension, Action action)
    {
        var name = config.Program!;
        if (!Regex.IsMatch(extension, @"\A[A-Za-z0-9]{1,8}\z", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Unrecognized program extension from OPEN; cannot establish scratch filename.");
        var filename = name + "." + extension;
        if (robot.FileExists(filename) != false)
            throw new InvalidOperationException("Scratch file must definitely be absent on the robot controller; existing or unknown FCHECK response.");
        for (var slot = 1; slot <= taskCount; slot++)
        {
            var current = robot.CurrentProgram(slot);
            if (!Regex.IsMatch(current, @"\A(?:[A-Za-z0-9]{1,8}(?:\.[A-Za-z0-9]{1,8})?)?\z", RegexOptions.CultureInvariant))
                throw new InvalidOperationException($"Unrecognized program name in task slot {slot}; isolation cannot be confirmed.");
            if (string.Equals(current.Split('.')[0], name, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Scratch program is selected in task slot {slot}.");
            if (slot == 1 && current.Length != 0)
                throw new InvalidOperationException("Task slot 1 must have no selected program: the library sends edit commands to slot 1.");
        }

        var editing = false;
        var saved = false;
        WithCleanup(() =>
        {
            robot.DiscardProgram();
            robot.OpenProgram(name);
            editing = true;
            robot.EditLine(config.Line, "END");
            var line = robot.GetLine(config.Line);
            if (!Regex.IsMatch(line, $@"\A0*{config.Line}\s+END\s*\z", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                throw new InvalidOperationException($"Scratch line was not created; LISTI returned: {line}");
            robot.SaveProgram();
            editing = false;
            saved = true;
            if (robot.FileExists(filename) != true)
                throw new InvalidOperationException($"Scratch file {filename} was not confirmed after SAVE.");
            robot.OpenProgram(name);
            editing = true;
            action();
        }, () => WithCleanup(
            () => { if (editing) robot.DiscardProgram(); },
            () =>
            {
                if (!saved) return;
                if (robot.FileExists(filename) != true)
                    throw new InvalidOperationException($"Scratch file {filename} cannot be confirmed for removal.");
                robot.DeleteFile(filename);
                if (robot.FileExists(filename) != false)
                    throw new InvalidOperationException($"Scratch file {filename} removal cannot be confirmed.");
            }));
    }

    internal static void EditLine(IRobotCommands robot, int line)
    {
        var original = robot.GetLine(line);
        if (!Regex.IsMatch(original, $@"\A0*{line}\s+\S", RegexOptions.CultureInvariant))
            throw new InvalidOperationException($"Scratch line {line} was not found or did not match; LISTI returned: {original}");
        const string replacement = "' MELFA TEST SCRATCH";
        if (Regex.IsMatch(original, $@"\A0*{line}\s+' MELFA TEST SCRATCH\s*\z"))
            throw new InvalidOperationException("Scratch line already contains the test text.");
        robot.EditLine(line, replacement);
        Assert.Matches($@"\A0*{line}\s+' MELFA TEST SCRATCH\s*\z", robot.GetLine(line));
    }

    internal static void EditJoint(IRobotCommands robot, string name)
    {
        if (robot.VariableType(name) != typeof(PositionJ))
            throw new InvalidOperationException("The scratch variable must be an existing local joint position.");
        var original = robot.GetJointVariable(name);
        var axes = new[] { original.J1, original.J2, original.J3, original.J4, original.J5, original.J6, original.J7, original.J8 };
        if (axes.Any(x => !double.IsFinite(x) || Math.Abs(x) > 10000) || !new PositionJ(original.ToString()).Equals(original))
            throw new InvalidOperationException("Original joint must have eight finite, exactly two-decimal round-trippable axes.");
        var changed = new PositionJ(original.J1 + 1, original.J2, original.J3, original.J4, original.J5, original.J6, original.J7, original.J8);
        if (!new PositionJ(changed.ToString()).Equals(changed))
            throw new InvalidOperationException("Test joint does not round-trip exactly.");
        var attempted = false;
        WithCleanup(() =>
        {
            attempted = true;
            robot.SetVariable(name, changed.ToString());
            Assert.Equal(changed, robot.GetJointVariable(name));
        }, () =>
        {
            if (!attempted) return;
            robot.SetVariable(name, original.ToString());
            Assert.Equal(original, robot.GetJointVariable(name));
        });
    }
}
