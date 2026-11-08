using System;
using System.Globalization;
using System.Resources;

namespace Melfa.Robot;

public sealed partial class MelfaRobotException : Exception
{
    private static readonly ResourceManager[] ErrorResources = CreateErrorResources();
    private static readonly ResourceManager UnknownErrorResources =
        new("Melfa.Robot.Resources.ErrorMessages", typeof(MelfaRobotException).Assembly);

    private static ResourceManager[] CreateErrorResources()
    {
        var resources = new ResourceManager[10];
        for (var i = 0; i < resources.Length; i++)
            resources[i] = new ResourceManager($"Melfa.Robot.Resources.ErrorMessages.{i}000", typeof(MelfaRobotException).Assembly);
        return resources;
    }

    private static string GetErrorResource(int errno, char field)
    {
        if (0 < errno && errno <= 9999)
        {
            var resources = ErrorResources[errno / 1000];
<<<<<<< HEAD
            var value = GetResource(resources, $"{GetCompactField(field)}{errno:0000}", $"Error.{errno:0000}.{field}");
=======
            var value = resources.GetString($"{field}{errno:0000}");
>>>>>>> origin/compact-error-resource-keys
            if (value != null)
                return value;

            var legacyField = field switch
            {
                'M' => "Message",
                'C' => "Cause",
                'S' => "Measures",
                _ => null,
            };
            if (legacyField != null)
            {
                value = resources.GetString($"Error.{errno:0000}.{legacyField}");
                if (value != null)
                    return value;
            }
        }

<<<<<<< HEAD
        return GetResource(UnknownErrorResources, $"{GetCompactField(field)}0000", $"Error.Unknown.{field}");
    }

    private static string GetResource(ResourceManager resources, string compactKey, string legacyKey)
    {
        for (var culture = CultureInfo.CurrentUICulture; culture != CultureInfo.InvariantCulture; culture = culture.Parent)
        {
            var resourceSet = resources.GetResourceSet(culture, true, false);
            var value = resourceSet?.GetString(compactKey) ?? resourceSet?.GetString(legacyKey);
            if (value != null)
                return value;
        }

        return resources.GetString(compactKey) ?? resources.GetString(legacyKey);
    }

    private static char GetCompactField(string field) => field switch
    {
        "Message" => 'M',
        "Cause" => 'C',
        "Measures" => 'S',
        "Level" => 'L',
        _ => throw new ArgumentOutOfRangeException(nameof(field)),
    };

    private static string GetErrorMessage(int errno) => GetErrorResource(errno, "Message");
    private static string GetErrorCause(int errno) => GetErrorResource(errno, "Cause");
    private static string GetErrorMeasures(int errno) => GetErrorResource(errno, "Measures");
    private static ErrorSeverity GetErrorSeverity(int errno) => GetErrorResource(errno, "Level") switch
=======
        return UnknownErrorResources.GetString($"{field}0000");
    }

    private static string GetErrorMessage(int errno) => GetErrorResource(errno, 'M');
    private static string GetErrorCause(int errno) => GetErrorResource(errno, 'C');
    private static string GetErrorMeasures(int errno) => GetErrorResource(errno, 'S');
    private static ErrorSeverity GetErrorSeverity(int errno) => GetErrorResource(errno, 'L') switch
>>>>>>> origin/compact-error-resource-keys
    {
        nameof(ErrorSeverity.High) => ErrorSeverity.High,
        nameof(ErrorSeverity.Low) => ErrorSeverity.Low,
        nameof(ErrorSeverity.Warning) => ErrorSeverity.Warning,
        nameof(ErrorSeverity.Caution) => ErrorSeverity.Caution,
        _ => ErrorSeverity.Unknown,
    };

    public static MelfaRobotException FromCommandResult(string ret) =>
        new(int.Parse(RegexHelper.ErrorRegex().Match(ret).Groups[nameof(ErrorNumber)].Value), null);

    public static MelfaRobotException FromErrorResult(string cmd, string ret, Exception inner = null) =>
        new(int.Parse(ret.Substring(3, 4)), cmd, inner);

    public static MelfaRobotException FromErrorNumber(int err) =>
        new(err, null);

    public string Command { get; }
    public int ErrorNumber { get; }
    public string ErrorMessage { get; }
    public string ErrorCause { get; }
    public string ErrorMeasures { get; }
    public ErrorSeverity ErrorLevel { get; }

    public override string Message => $"{ErrorNumber:0000} ({ErrorLevel}) - {ErrorMessage}";

    private MelfaRobotException(int errno, string cmd, Exception inner = null) :
        base(null, inner)
    {
        ErrorNumber = errno;
        ErrorMessage = GetErrorMessage(errno);
        ErrorCause = GetErrorCause(errno);
        ErrorMeasures = GetErrorMeasures(errno);
        ErrorLevel = GetErrorSeverity(errno);
        Command = cmd;
    }
}