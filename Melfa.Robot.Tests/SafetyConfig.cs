using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Melfa.Robot.Tests;

internal sealed record SafetyConfig(string Host, int Port, int Robot, int Delay, string Program, int Line, string Joint)
{
    internal static SafetyConfig Load(Func<string, string> get, bool write)
    {
        var readEnabled = Flag(get, "MELFA_TEST_READ");
        var writeEnabled = Flag(get, "MELFA_TEST_WRITE");
        if (!(write ? writeEnabled : readEnabled))
            return null;

        var host = Required(get, "MELFA_TEST_HOST");
        if (host.Length > 253 || host.Any(c => !(c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '-' or ':')))
            throw new InvalidOperationException("MELFA_TEST_HOST must be a plain hostname or IP address.");
        var port = Number(get, "MELFA_TEST_PORT", 10001, 1, 65535);
        var robot = Number(get, "MELFA_TEST_ROBOT", 1, 1, 3);
        var delay = Number(get, "MELFA_TEST_DELAY_MS", 150, 100, 10000);
        if (!write)
            return new(host, port, robot, delay, null, 0, null);

        if (Required(get, "MELFA_TEST_TARGET") != "SIMULATOR" ||
            Required(get, "MELFA_TEST_SIMULATOR_READ_CONFIRMED") != "1" ||
            Required(get, "MELFA_TEST_EXCLUSIVE_SCRATCH_CONFIRMED") != "1")
            throw new InvalidOperationException("Write tests require SIMULATOR and both explicit confirmations.");

        var program = Required(get, "MELFA_TEST_SCRATCH_PROGRAM");
        if (!Regex.IsMatch(program, @"\A[A-Za-z0-9]{1,8}\z", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Scratch program must be 1-8 ASCII alphanumeric characters, without an extension.");
        var line = Number(get, "MELFA_TEST_SCRATCH_LINE", null, 1, 999999);
        var joint = Required(get, "MELFA_TEST_SCRATCH_JOINT");
        if (!Regex.IsMatch(joint, @"\A[A-Za-z][A-Za-z0-9_]*\z", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Scratch joint must be a valid variable name.");
        return new(host, port, robot, delay, program, line, joint);
    }

    private static bool Flag(Func<string, string> get, string name) => get(name) switch
    {
        null or "" or "0" => false,
        "1" => true,
        _ => throw new InvalidOperationException($"{name} must be unset, 0, or 1.")
    };

    private static string Required(Func<string, string> get, string name) =>
        string.IsNullOrEmpty(get(name)) ? throw new InvalidOperationException($"{name} is required.") : get(name)!;

    internal static string ReadProgramName(Func<string, string> get)
    {
        var name = Required(get, "MELFA_TEST_READ_PROGRAM");
        if (!Regex.IsMatch(name, @"\A[A-Za-z0-9]{1,8}\z", RegexOptions.CultureInvariant))
            throw new InvalidOperationException("Read program must be 1-8 ASCII alphanumeric characters, without an extension.");
        return name;
    }

    private static int Number(Func<string, string> get, string name, int? defaultValue, int min, int max)
    {
        var text = get(name);
        if (text is null or "")
            return defaultValue ?? throw new InvalidOperationException($"{name} is required.");
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < min || value > max)
            throw new InvalidOperationException($"{name} must be an integer from {min} to {max}.");
        return value;
    }
}

internal sealed class ReadFactAttribute : FactAttribute
{
    public ReadFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MELFA_TEST_READ") is null or "" or "0")
            Skip = "Set MELFA_TEST_READ=1 and configure a robot or simulator to enable.";
    }
}

internal sealed class ReadProgramFactAttribute : FactAttribute
{
    public ReadProgramFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MELFA_TEST_READ") is null or "" or "0" ||
            Environment.GetEnvironmentVariable("MELFA_TEST_READ_PROGRAM") is null or "")
            Skip = "Set MELFA_TEST_READ=1 and MELFA_TEST_READ_PROGRAM to a manually prepared program.";
    }
}

internal sealed class WriteFactAttribute : FactAttribute
{
    public WriteFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MELFA_TEST_WRITE") is null or "" or "0")
            Skip = "Simulator-only: set MELFA_TEST_WRITE=1 and complete all safety configuration.";
    }
}
