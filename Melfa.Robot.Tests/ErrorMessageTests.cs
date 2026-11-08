using System.Globalization;
using System.Collections;
using System.Resources;
using System.Text.RegularExpressions;
using Xunit;

namespace Melfa.Robot.Tests;

public class ErrorMessageTests
{
    [Theory]
    [InlineData(1, "Fail safe error", ErrorSeverity.High)]
    [InlineData(1030, "Power supply over-regeneration", ErrorSeverity.High)]
    [InlineData(2000, "servo is OFF", ErrorSeverity.Low)]
    [InlineData(3100, "Processor Call Stack over", ErrorSeverity.Low)]
    [InlineData(4000, "System error (time out)", ErrorSeverity.Low)]
    [InlineData(5000, "TB Enable key is ON", ErrorSeverity.High)]
    [InlineData(5620, "Cannot execute during CSTOP ON", ErrorSeverity.Low)]
    [InlineData(6010, "Illegal command", ErrorSeverity.Low)]
    [InlineData(7000, "Copy source file was not found", ErrorSeverity.Caution)]
    [InlineData(8300, "There are a lot of GETPOS define", ErrorSeverity.Low)]
    [InlineData(4140, "The program was not found", ErrorSeverity.Low)]
    [InlineData(9000, "User High level error", ErrorSeverity.High)]
    [InlineData(9099, "User High level error", ErrorSeverity.High)]
    [InlineData(9199, "User Low level error", ErrorSeverity.Low)]
    [InlineData(9299, "User Caution level error", ErrorSeverity.Caution)]
    public void LooksUpSplitResourcesAndRanges(int code, string message, ErrorSeverity level)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var error = MelfaRobotException.FromErrorNumber(code);
            Assert.Contains(message, error.ErrorMessage);
            Assert.Equal(level, error.ErrorLevel);
            Assert.NotEmpty(error.ErrorCause);
            Assert.NotEmpty(error.ErrorMeasures);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Fact]
    public void ResolvesExpandedCodesAndRejectsOutOfRangeDigits()
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Contains("PIO I/F", MelfaRobotException.FromErrorNumber(161).ErrorMessage);
            Assert.Contains("Unknown error", MelfaRobotException.FromErrorNumber(164).ErrorMessage);
            Assert.Contains("Servo amplifier motor overheat", MelfaRobotException.FromErrorNumber(431).ErrorMessage);
            Assert.Contains("Unknown error", MelfaRobotException.FromErrorNumber(439).ErrorMessage);
            Assert.Contains("Motor ground fault", MelfaRobotException.FromErrorNumber(820).ErrorMessage);
            Assert.Contains("Designate", MelfaRobotException.FromErrorNumber(4140).ErrorMeasures);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void ExpandsAxisNumberInBothCultures(int axis)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var culture in new[] { "en-US", "zh-Hant" })
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                var error = MelfaRobotException.FromErrorResult("MOV P1", $"QeR213{axis}00000");
                Assert.Equal(2130 + axis, error.ErrorNumber);
                Assert.Contains($"J{axis}", error.ErrorMessage);
                Assert.Equal(ErrorSeverity.High, error.ErrorLevel);
            }
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Fact]
    public void ParsesFourDigitErrorFromControllerReply()
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var error = MelfaRobotException.FromErrorResult("RUN", "QeR602000000");
            Assert.Equal(6020, error.ErrorNumber);
            Assert.Equal("The operation is disabled", error.ErrorMessage);
            Assert.Equal(ErrorSeverity.Low, error.ErrorLevel);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData(2160, "J0")]
    [InlineData(2170, "J0")]
    [InlineData(2311, "Composite speed")]
    [InlineData(2317, "Z-")]
    public void ExpandsSpecialPlaceholderValues(int code, string expected)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Contains(expected, MelfaRobotException.FromErrorNumber(code).ErrorMessage);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData("")]
    [InlineData("zh-Hant")]
    public void ResourcesContainOnlyConcreteNumbersAndInvariantLevels(string culture)
    {
        var assembly = typeof(MelfaRobotException).Assembly;
        for (var group = 0; group < 10; group++)
        {
            var manager = new ResourceManager($"Melfa.Robot.Resources.ErrorMessages.{group}000", assembly);
            var set = manager.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false);
            Assert.NotNull(set);
            foreach (DictionaryEntry item in set)
            {
                var key = Assert.IsType<string>(item.Key);
                if (new[] { "Name1", "Color1", "Bitmap1", "Icon1" }.Contains(key))
                    continue;
                if (string.IsNullOrEmpty(culture))
                {
                    Assert.Matches(@"^[MCSL]\d{4}$", key);
                    if (key[0] == 'L')
                        Assert.Contains(Assert.IsType<string>(item.Value), new[] { "High", "Low", "Caution", "Unknown" });
                }
                else
                    Assert.Matches(@"^Error\.\d{4}\.(Message|Cause|Measures)$", key);
            }

            if (string.IsNullOrEmpty(culture))
            {
                var entries = set.Cast<DictionaryEntry>()
                    .Select(item => Assert.IsType<string>(item.Key))
                    .GroupBy(key => key.Substring(1));

                foreach (var error in entries)
                    Assert.Equal(new[] { 'C', 'L', 'M', 'S' }, error.Select(key => key[0]).OrderBy(field => field));
        }
    }

    [Fact]
    public void UnknownErrorResourcesUseCompactKeys()
    {
        var manager = new ResourceManager("Melfa.Robot.Resources.ErrorMessages", typeof(MelfaRobotException).Assembly);
        var set = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false);

        Assert.NotNull(set);
        Assert.Equal(new[] { "C0000", "L0000", "M0000", "S0000" },
            set.Cast<DictionaryEntry>().Select(item => Assert.IsType<string>(item.Key)).OrderBy(key => key));
        foreach (var field in new[] { 'C', 'L', 'M', 'S' })
            Assert.False(string.IsNullOrEmpty(manager.GetString($"{field}0000", CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void EveryExpandedErrorHasTraditionalChineseFields()
    {
        var assembly = typeof(MelfaRobotException).Assembly;
        var chinese = CultureInfo.GetCultureInfo("zh-Hant");
        for (var group = 0; group < 10; group++)
        {
            var manager = new ResourceManager($"Melfa.Robot.Resources.ErrorMessages.{group}000", assembly);
            var neutral = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            var localized = manager.GetResourceSet(chinese, true, false);
            Assert.NotNull(neutral);
            Assert.NotNull(localized);
            foreach (DictionaryEntry item in neutral)
            {
                var key = Assert.IsType<string>(item.Key);
                if (new[] { "Name1", "Color1", "Bitmap1", "Icon1" }.Contains(key) || !Regex.IsMatch(key, @"^[MCSL]\d{4}$"))
                    continue;
                if (key[0] == 'L')
                {
                    Assert.Null(localized.GetString(key));
                    var level = Assert.IsType<string>(item.Value);
                    Assert.Equal(level, manager.GetString(key, chinese));
                    Assert.Equal(level, manager.GetString(key, CultureInfo.GetCultureInfo("zh-TW")));
                }
                else
                {
                    var legacyField = key[0] switch
                    {
                        'M' => "Message",
                        'C' => "Cause",
                        'S' => "Measures",
                        _ => throw new InvalidOperationException(),
                    };
                    var legacyKey = $"Error.{key.Substring(1)}.{legacyField}";
                    Assert.False(string.IsNullOrEmpty(localized.GetString(legacyKey)), $"Missing Traditional Chinese resource: {legacyKey}");
                }
            }
        }
    }

    [Fact]
    public void PreservesAmbiguousVariantsAndTheirMeasures()
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var error = MelfaRobotException.FromErrorNumber(9);
            Assert.Contains("Version UP (ALL)", error.ErrorMessage);
            Assert.Contains("Rewriting failed servo", error.ErrorMessage);
            Assert.Contains("Version UP (ALL)", error.ErrorMeasures);
            Assert.Contains("Rewriting failed servo", error.ErrorMeasures);
            Assert.Equal(ErrorSeverity.Unknown, MelfaRobotException.FromErrorNumber(99).ErrorLevel);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData(9, 5)]
    [InlineData(811, 2)]
    [InlineData(2282, 4)]
    [InlineData(2613, 6)]
    [InlineData(3987, 15)]
    [InlineData(8878, 2)]
    public void NumbersMessageVariantsAndTheirCausesAndMeasures(int code, int count)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var culture in new[] { "en-US", "zh-Hant" })
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                var error = MelfaRobotException.FromErrorNumber(code);
                foreach (var text in new[] { error.ErrorMessage, error.ErrorCause, error.ErrorMeasures })
                {
                    var numbers = Regex.Matches(text, @"(?m)^(\d+)\. ")
                        .Select(match => int.Parse(match.Groups[1].Value));
                    Assert.Equal(Enumerable.Range(1, count), numbers);
                }
            }
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Fact]
    public void FormatsSosVariantsInManualOrder()
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var error = MelfaRobotException.FromErrorResult("RUN", "QeR228200000");
            Assert.Equal(
                "1. SOS (Position error)\n2. SOS (Speed error)\n3. SOS (Position command error)\n4. SOS (Speed command error)",
                error.ErrorMessage.Replace("\r\n", "\n"));
            Assert.Contains("1. SOS (Position error):", error.ErrorCause);
            Assert.Contains("4. SOS (Speed command error):", error.ErrorMeasures);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData("zh-Hant")]
    [InlineData("zh-TW")]
    public void UsesTraditionalChineseAndPreservesEnglishFallback(string culture)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var error = MelfaRobotException.FromErrorNumber(51);
            Assert.Contains("急停", error.ErrorMessage);
            Assert.Equal(ErrorSeverity.High, error.ErrorLevel);
            Assert.Equal(ErrorSeverity.Low, MelfaRobotException.FromErrorNumber(5620).ErrorLevel);
            Assert.Contains("SRVOFF 信號 ON", MelfaRobotException.FromErrorNumber(5630).ErrorMessage);
            Assert.Contains("GETPOS", MelfaRobotException.FromErrorNumber(8300).ErrorMessage);
            Assert.Contains("定義過多", MelfaRobotException.FromErrorNumber(8300).ErrorMessage);
            Assert.Contains("Unknown", MelfaRobotException.FromErrorNumber(10000).ErrorLevel.ToString());
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }

    [Theory]
    [InlineData(1030, "電源再生過量")]
    [InlineData(161, "無法同時使用平行 I/O")]
    [InlineData(2613, "區塊資料計算錯誤")]
    [InlineData(3100, "處理器呼叫堆疊溢位")]
    [InlineData(4000, "系統錯誤")]
    [InlineData(5000, "啟用開關")]
    [InlineData(6010, "無效的命令")]
    [InlineData(7000, "找不到複製來源檔案")]
    [InlineData(8300, "GETPOS 定義過多")]
    [InlineData(9001, "使用者高階錯誤")]
    public void LooksUpTraditionalChineseAcrossRanges(int code, string message)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-Hant");
            Assert.Contains(message, MelfaRobotException.FromErrorNumber(code).ErrorMessage);
        }
        finally { CultureInfo.CurrentUICulture = old; }
    }
}
