using System.Text;
using Xunit;

namespace Melfa.Robot.Tests;

public class SafetyTests
{
    private static SafetyConfig Load(Dictionary<string, string> env, bool write) =>
        SafetyConfig.Load(name => env.GetValueOrDefault(name), write);

    private static Dictionary<string, string> WriteEnvironment() => new()
    {
        ["MELFA_TEST_WRITE"] = "1",
        ["MELFA_TEST_HOST"] = "localhost",
        ["MELFA_TEST_TARGET"] = "SIMULATOR",
        ["MELFA_TEST_SIMULATOR_READ_CONFIRMED"] = "1",
        ["MELFA_TEST_EXCLUSIVE_SCRATCH_CONFIRMED"] = "1",
        ["MELFA_TEST_SCRATCH_PROGRAM"] = "SCRATCH",
        ["MELFA_TEST_SCRATCH_LINE"] = "10",
        ["MELFA_TEST_SCRATCH_JOINT"] = "J1"
    };

    [Fact]
    public void ActualEnvironmentIsValidEvenWhenIntegrationTestsAreSkipped()
    {
        SafetyConfig.Load(Environment.GetEnvironmentVariable, false);
        SafetyConfig.Load(Environment.GetEnvironmentVariable, true);
    }

    [Fact]
    public void IntegrationIsOffByDefaultAndFlagsAreIndependent()
    {
        var env = new Dictionary<string, string>();
        Assert.Null(Load(env, false));
        Assert.Null(Load(env, true));
        env = WriteEnvironment();
        Assert.Null(Load(env, false));
        Assert.NotNull(Load(env, true));
        env["MELFA_TEST_WRITE"] = "0";
        env["MELFA_TEST_READ"] = "1";
        Assert.NotNull(Load(env, false));
        Assert.Null(Load(env, true));
    }

    [Theory]
    [InlineData("TEST")]
    [InlineData("1")]
    public void ReadProgramNameAcceptsAProgramName(string name) =>
        Assert.Equal(name, SafetyConfig.ReadProgramName(key => key == "MELFA_TEST_READ_PROGRAM" ? name : null));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TEST.MB6")]
    [InlineData("TEST;FDEL")]
    [InlineData("TOOLONGNAME")]
    public void ReadProgramNameRejectsMissingOrUnsafeNames(string name) =>
        Assert.Throws<InvalidOperationException>(() => SafetyConfig.ReadProgramName(key => key == "MELFA_TEST_READ_PROGRAM" ? name : null));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(15)]
    public void ProgramFilePreviewLimitsLinesAndReportsRemainder(int count)
    {
        var lines = Enumerable.Range(1, count).Select(i => $"{i} Hlt").ToArray();
        using var contents = new MemoryStream(Encoding.ASCII.GetBytes(string.Join("\r\n", lines) + (count > 0 ? "\r\n" : "")));
        var expected = lines.Take(10).ToList();
        if (count > 10)
            expected.Add($"... {count - 10} more lines");
        Assert.Equal(expected, ProgramFilePreview.ReadLines(contents));
    }

    [Fact]
    public void ProgramFilePreviewEscapesNonAsciiBytes()
    {
        using var contents = new MemoryStream(new byte[] { 0x48, 0x69, 0x80, 0xff, 0x35, 0x28 });
        Assert.Equal(new[] { @"Hi\x80\xFF5(" }, ProgramFilePreview.ReadLines(contents));
    }

    [Fact]
    public void ProgramFilePreviewEscapesEveryNonAsciiByte()
    {
        var bytes = Enumerable.Range(128, 128).Select(value => (byte)value).ToArray();
        using var contents = new MemoryStream(bytes);
        Assert.Equal(new[] { string.Concat(bytes.Select(value => $"\\x{value:X2}")) }, ProgramFilePreview.ReadLines(contents));
    }

    [Fact]
    public void ProgramFilePreviewDoesNotAutoDetectUtf8()
    {
        using var contents = new MemoryStream(new byte[] { 0xef, 0xbb, 0xbf, 0x41 });
        Assert.Equal(new[] { @"\xEF\xBB\xBFA" }, ProgramFilePreview.ReadLines(contents));
    }

    [Theory]
    [InlineData("MELFA_TEST_WRITE", "true")]
    [InlineData("MELFA_TEST_READ", "yes")]
    [InlineData("MELFA_TEST_HOST", "localhost;1")]
    [InlineData("MELFA_TEST_PORT", "0")]
    [InlineData("MELFA_TEST_ROBOT", "0")]
    [InlineData("MELFA_TEST_ROBOT", "4")]
    [InlineData("MELFA_TEST_DELAY_MS", "99")]
    [InlineData("MELFA_TEST_TARGET", "ROBOT")]
    [InlineData("MELFA_TEST_SIMULATOR_READ_CONFIRMED", "0")]
    [InlineData("MELFA_TEST_EXCLUSIVE_SCRATCH_CONFIRMED", "0")]
    [InlineData("MELFA_TEST_SCRATCH_PROGRAM", "A;SAVE")]
    [InlineData("MELFA_TEST_SCRATCH_LINE", "0")]
    [InlineData("MELFA_TEST_SCRATCH_JOINT", "J1;SAVE")]
    [InlineData("MELFA_TEST_SCRATCH_JOINT", "1JOINT")]
    [InlineData("MELFA_TEST_SCRATCH_JOINT", "J1\r")]
    public void InvalidConfigurationFailsClosed(string key, string value)
    {
        var env = WriteEnvironment();
        env[key] = value;
        Assert.Throws<InvalidOperationException>(() => Load(env, true));
    }

    [Theory]
    [InlineData("J1")]
    [InlineData("JPOS1")]
    [InlineData("P1")]
    [InlineData("ScratchJoint")]
    [InlineData("J_01")]
    public void ScratchJointAcceptsVariableNamesWithoutAPrefixConvention(string name)
    {
        var env = WriteEnvironment();
        env["MELFA_TEST_SCRATCH_JOINT"] = name;
        var config = Load(env, true)!;
        Assert.Equal(name, config.Joint);
        var fake = new FakeRobot();
        ScratchOperations.EditJoint(fake, config.Joint!, 2);
        Assert.All(fake.VariableNames, actual => Assert.Equal(name, actual));
        Assert.Equal(6, fake.VariableNames.Count);
        Assert.All(fake.VariableSlots, slot => Assert.Equal(2, slot));
    }

    [Theory]
    [InlineData("MELFA_TEST_HOST")]
    [InlineData("MELFA_TEST_TARGET")]
    [InlineData("MELFA_TEST_SIMULATOR_READ_CONFIRMED")]
    [InlineData("MELFA_TEST_EXCLUSIVE_SCRATCH_CONFIRMED")]
    [InlineData("MELFA_TEST_SCRATCH_PROGRAM")]
    [InlineData("MELFA_TEST_SCRATCH_LINE")]
    [InlineData("MELFA_TEST_SCRATCH_JOINT")]
    public void MissingWriteConfigurationFailsClosed(string key)
    {
        var env = WriteEnvironment();
        env.Remove(key);
        Assert.Throws<InvalidOperationException>(() => Load(env, true));
    }

    [Fact]
    public void ExistingUnknownOrSelectedScratchNeverOpens()
    {
        var config = Load(WriteEnvironment(), true)!;
        foreach (var exists in new bool?[] { true, null })
        {
            var fake = new FakeRobot { Exists = exists };
            Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(fake, config, 1, "MB6", () => { }));
            Assert.Equal("SCRATCH.MB6", fake.CheckedFile);
            Assert.DoesNotContain(nameof(FakeRobot.ProgramOpen), fake.Calls);
        }
        var selected = new FakeRobot { Selected = "scratch.MB6" };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(selected, config, 1, "MB6", () => { }));
        Assert.DoesNotContain(nameof(FakeRobot.ProgramOpen), selected.Calls);
        var ambiguous = new FakeRobot { Selected = "OTHER;unexpected" };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(ambiguous, config, 1, "MB6", () => { }));
        Assert.DoesNotContain(nameof(FakeRobot.ProgramOpen), ambiguous.Calls);
        var otherInSlotOne = new FakeRobot { Selected = "OTHER" };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(otherInSlotOne, config, 1, "MB6", () => { }));
        Assert.DoesNotContain(nameof(FakeRobot.ProgramOpen), otherInSlotOne.Calls);
    }

    [Fact]
    public void AbsentScratchIsCreatedAndRemoved()
    {
        var env = WriteEnvironment();
        env["MELFA_TEST_SCRATCH_PROGRAM"] = "1";
        var config = Load(env, true)!;
        var fake = new FakeRobot();
        var ran = false;
        ScratchOperations.WithScratch(fake, config, 1, "MB6", () => ran = true);
        Assert.True(ran);
        Assert.Equal("1.MB6", fake.CheckedFile);
        Assert.Equal(new[] { "FileExists", "GetCurrentProgram", "ProgramNew", "ProgramOpen", "ProgramEdit", "ProgramGetLine", "ProgramSave",
            "FileExists", "ProgramOpen", "ProgramNew", "FileExists", "FileDelete", "FileExists" }, fake.Calls);
        Assert.False(fake.Exists);
    }

    [Fact]
    public void ScratchIsDiscardedAndRemovedWhenTestFails()
    {
        var fake = new FakeRobot();
        var config = Load(WriteEnvironment(), true)!;
        var error = Assert.Throws<InvalidOperationException>(() =>
            ScratchOperations.WithScratch(fake, config, 1, "MB6", () => throw new InvalidOperationException("test failed")));
        Assert.Equal("test failed", error.Message);
        Assert.Contains(nameof(FakeRobot.FileDelete), fake.Calls);
        Assert.False(fake.Exists);
    }

    [Fact]
    public void FailedSaveNeverDeletesAnUnconfirmedFile()
    {
        var fake = new FakeRobot { Fail = nameof(FakeRobot.ProgramSave) };
        var config = Load(WriteEnvironment(), true)!;
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(fake, config, 1, "MB6", () => { }));
        Assert.DoesNotContain(nameof(FakeRobot.FileDelete), fake.Calls);
        Assert.Equal(nameof(FakeRobot.ProgramNew), fake.Calls.Last());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void ProgramOneMb6IsNeverTouchedWhenPresentOrUnknown(bool? exists)
    {
        var env = WriteEnvironment();
        env["MELFA_TEST_SCRATCH_PROGRAM"] = "1";
        var fake = new FakeRobot { Exists = exists };
        var error = Assert.Throws<InvalidOperationException>(() =>
            ScratchOperations.WithScratch(fake, Load(env, true)!, 1, "MB6", () => { }));
        Assert.Contains("definitely be absent", error.Message);
        Assert.Equal("1.MB6", fake.CheckedFile);
        Assert.Equal(new[] { "FileExists" }, fake.Calls);
    }

    [Fact]
    public void ScratchSelectedInAnotherSlotIsRejectedEvenWithDifferentExtension()
    {
        var config = Load(WriteEnvironment(), true)!;
        var fake = new FakeRobot { SelectedBySlot = new Dictionary<int, string> { [2] = "scratch.MB5" } };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(fake, config, 2, "MB6", () => { }));
        Assert.Equal(new[] { "FileExists", "GetCurrentProgram", "GetCurrentProgram" }, fake.Calls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("MB6;FDEL")]
    [InlineData(".MB6")]
    public void AmbiguousExtensionNeverIssuesFileCheck(string extension)
    {
        var fake = new FakeRobot();
        var config = Load(WriteEnvironment(), true)!;
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(fake, config, 1, extension, () => { }));
        Assert.Empty(fake.Calls);
    }

    [Fact]
    public async Task PrimaryAndCleanupFailuresAreBothReported()
    {
        var ex = Assert.ThrowsAsync<AggregateException>(() => ScratchOperations.WithCleanup(
            () => throw new InvalidOperationException("primary"),
            () => throw new InvalidOperationException("cleanup")));
        Assert.Equal(new[] { "primary", "cleanup" }, ex.Exception.InnerExceptions.Select(e => e.Message));
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithCleanup(() => { }, () => throw new Exception("cleanup")));
    }

    [Fact]
    public void ScratchPreflightErrorIsSurfacedWithoutWrites()
    {
        var config = Load(WriteEnvironment(), true)!;
        foreach (var command in new[] { "FileExists", "GetCurrentProgram" })
        {
            var fake = new FakeRobot { Fail = command };
            var error = Assert.Throws<InvalidOperationException>(() => ScratchOperations.WithScratch(fake, config, 1, "MB6", () => { }));
            Assert.Equal(command, error.Message);
            Assert.Equal(command == "FileExists" ? new[] { "FileExists" } : new[] { "FileExists", "GetCurrentProgram" }, fake.Calls);
        }
    }

    private const int EditSlot = 2;
    private const string TypeRead = nameof(FakeRobot.ProgramGetVariableType);
    private const string Read = nameof(FakeRobot.ReadVariable);
    private const string Set = nameof(FakeRobot.ProgramSetVariable);

    [Fact]
    public void JointIsVerifiedRestoredEvenWhenWriteIsUncertain()
    {
        var fake = new FakeRobot { FailFirstSet = true };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.EditJoint(fake, "J1", EditSlot));
        Assert.Equal(new[] { TypeRead, Read, Set, Set, Read }, fake.Calls);
        Assert.Equal(new PositionJ(1, 2, 3, 4, 5, 6, 7, 8), fake.Position);
    }

    [Fact]
    public void JointSuccessVerifiesChangeAndRestoration()
    {
        var fake = new FakeRobot();
        ScratchOperations.EditJoint(fake, "J1", EditSlot);
        Assert.Equal(new[] { TypeRead, Read, Set, Read, Set, Read }, fake.Calls);
        Assert.Equal(new PositionJ(1, 2, 3, 4, 5, 6, 7, 8), fake.Position);
    }

    [Fact]
    public void SuccessfulWriteResponseWithoutChangedValueFailsAndStillRestores()
    {
        var fake = new FakeRobot { IgnoreFirstSet = true };
        Assert.ThrowsAny<Exception>(() => ScratchOperations.EditJoint(fake, "J1", EditSlot));
        Assert.Equal(new[] { TypeRead, Read, Set, Read, Set, Read }, fake.Calls);
        Assert.Equal(new PositionJ(1, 2, 3, 4, 5, 6, 7, 8), fake.Position);
    }

    [Fact]
    public void UnsafeOriginalJointNeverGetsWritten()
    {
        var fake = new FakeRobot { Position = new PositionJ(1.234, 2, 3, 4, 5, 6, 7, 8) };
        Assert.Throws<InvalidOperationException>(() => ScratchOperations.EditJoint(fake, "J1", EditSlot));
        Assert.DoesNotContain(Set, fake.Calls);
    }

    [Fact]
    public void FakeRobotRejectsCommandsOutsideTheScratchWorkflow()
    {
        var fake = new FakeRobot();
        Assert.Throws<NotSupportedException>(() => fake.RunProgram("SCRATCH"));
        Assert.Throws<NotSupportedException>(() => fake.Override = 10);
        Assert.Throws<NotSupportedException>(() => _ = fake.CurrentPositionJ);
        Assert.Equal(new[] { nameof(FakeRobot.RunProgram), nameof(FakeRobot.Override), nameof(FakeRobot.CurrentPositionJ) }, fake.Calls);
    }
}
