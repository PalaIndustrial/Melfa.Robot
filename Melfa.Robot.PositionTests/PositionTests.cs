using System.Globalization;
using Melfa.Robot;
using Xunit;

namespace Melfa.Robot.PositionTests;

public class PositionTests
{
    private const string JointResponse = "J1;+1.25;J2;-2.50;J3;3;J4;.5;J5;5.0;J6;6.0;;****,****;100;0.00;00000000";
    private const string CartesianResponse = "X;+1.25;Y;-2.50;Z;3;A;270;B;-270;C;540;L1;7.0;;6,4294967295;100;0.00;00000000";

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("tr-TR")]
    public void ParseAndFormatInvariantlyAcrossCultures(string culture)
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var j = new PositionJ("(+1.25, -2.50, .5, 3., 1e1, NaN, +Inf, -Inf)");
            Assert.Equal(1.25, j.J1);
            Assert.Equal(-2.5, j.J2);
            Assert.Equal(0.5, j.J3);
            Assert.Equal(10, j.J5);
            Assert.True(double.IsNaN(j.J6));
            Assert.Equal(double.PositiveInfinity, j.J7);
            Assert.Equal(j, new PositionJ(j.ToString()));

            var p = new PositionP("(+1.25, -2.50, .5, 270, -270, 540, NaN, Inf) (0x06, 0xFFFFFFFF)");
            Assert.Equal(PositionConfig.RAF, p.FLG1);
            Assert.Equal(uint.MaxValue, p.FLG2);
            Assert.Equal(-90, p.A);
            Assert.Equal(90, p.B);
            Assert.Equal(-180, p.C);
            Assert.Equal(p, new PositionP(p.ToString()));
            Assert.Contains("1.25", p.ToString());
            Assert.Contains("1.25", j.ToString());

            var fromJ = PositionJ.FromCommand("QoK" + JointResponse);
            var fromP = PositionP.FromCommand("QoK" + CartesianResponse);
            Assert.Equal(1.25, fromJ.J1);
            Assert.Equal(0.5, fromJ.J4);
            Assert.True(double.IsNaN(fromJ.J7));
            Assert.Equal(1.25, fromP.X);
            Assert.Equal(-90, fromP.A);
            Assert.True(double.IsNaN(fromP.L2));
            Assert.Equal(uint.MaxValue, fromP.FLG2);
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Theory]
    [InlineData("")]
    [InlineData("(, 1, 2, 3, 4, 5, 6, 7)")]
    [InlineData("(., 1, 2, 3, 4, 5, 6, 7)")]
    [InlineData("(1x2, 1, 2, 3, 4, 5, 6, 7)")]
    [InlineData("(1e9999, 1, 2, 3, 4, 5, 6, 7)")]
    [InlineData("(1, 2, 3, 4, 5, 6, 7, 8) trailing")]
    public void JointStringRejectsMalformedValues(string raw) =>
        Assert.Throws<PositionParseException>(() => new PositionJ(raw));

    [Theory]
    [InlineData("(, 1, 2, 3, 4, 5, 6, 7) (0, 0)")]
    [InlineData("(inf, 1, 2, 3, 4, 5, 6, 7) (256, 0)")]
    [InlineData("(1, 2, 3, 4, 5, 6, 7, 8) (0x100, 0)")]
    [InlineData("(1, 2, 3, 4, 5, 6, 7, 8) (0, 4294967296)")]
    [InlineData("(1e9999, 2, 3, 4, 5, 6, 7, 8) (0, 0)")]
    [InlineData("(1, 2, 3, 4, 5, 6, 7, 8) (0, 0)garbage")]
    public void CartesianStringRejectsMalformedValues(string raw) =>
        Assert.Throws<PositionParseException>(() => new PositionP(raw));

    [Fact]
    public void StringFlagsAndSpecialValuesAreAccepted()
    {
        var p = new PositionP("(inf, -inf, NaN, 0, 0, 0, 0, 0) (rAf, 0xFF)");
        Assert.Equal(double.PositiveInfinity, p.X);
        Assert.Equal(double.NegativeInfinity, p.Y);
        Assert.True(double.IsNaN(p.Z));
        Assert.Equal(PositionConfig.RAF, p.FLG1);
        Assert.Equal(255u, p.FLG2);
    }

    [Fact]
    public void DocumentedListpPositionAllowsOmittedExternalAxes()
    {
        const string response = "P1=(+400.13,+0.00,+644.62,+180.00,+0.00,+180.00)(7,0)";
        var position = new PositionP(response.Substring(response.IndexOf('=') + 1));
        Assert.Equal(400.13, position.X);
        Assert.Equal(644.62, position.Z);
        Assert.Equal(-180, position.A);
        Assert.Equal(-180, position.C);
        Assert.Equal(PositionConfig.RAN, position.FLG1);
        Assert.Equal(0u, position.FLG2);
        Assert.True(double.IsNaN(position.L1));
        Assert.True(double.IsNaN(position.L2));
        Assert.Equal(position, new PositionP(position.ToString()));

        var withL1 = new PositionP("(1,2,3,4,5,6,+7.25)(7,0)");
        Assert.Equal(7.25, withL1.L1);
        Assert.True(double.IsNaN(withL1.L2));
        Assert.Throws<PositionParseException>(() => new PositionP("(1,2,3,4,5,6,)(7,0)"));
    }

    [Fact]
    public void CommandResponsesSupportSixThroughEightAxesAndOptionalPrefix()
    {
        Assert.True(double.IsNaN(PositionJ.FromCommand(JointResponse).J8));
        var j = PositionJ.FromCommand(JointResponse.Replace("J6;6.0;", "J6;6.0;J7;7.25;J8;8.5;"));
        Assert.Equal(7.25, j.J7);
        Assert.Equal(8.5, j.J8);
        var p = PositionP.FromCommand(CartesianResponse.Replace("L1;7.0;", "L1;7.0;L2;8.0;"));
        Assert.Equal(8, p.L2);
        Assert.Equal(PositionConfig.RAF, p.FLG1);
    }

    [Fact]
    public void CommandResponsesRejectMalformedAndDuplicateAxes()
    {
        string[] invalidJ = {
            "", "garbage" + JointResponse, JointResponse + "tail",
            JointResponse.Replace("J1;+1.25;", "J1;1x25;"),
            JointResponse.Replace("J2;-2.50;", "J1;-2.50;"),
            JointResponse.Replace("J6;6.0;", ""),
            JointResponse.Replace("J1;+1.25;", "J7;7.0;"),
            JointResponse.Replace("J3;3;", "J7;7.0;J8;8.0;"),
            JointResponse.Replace("J1;+1.25;", "J1;1e9999;"),
            JointResponse.Replace("0.00;00000000", "0x00;00000000"),
            JointResponse.Replace("****,****", "****,***")
        };
        foreach (var raw in invalidJ)
        {
            var error = Assert.Throws<PositionParseException>(() => PositionJ.FromCommand(raw));
            Assert.Equal(raw, error.RawValue);
            Assert.Equal(typeof(PositionJ), error.TargetType);
        }
        string[] invalidP = {
            "", "bad" + CartesianResponse, CartesianResponse + "\n",
            CartesianResponse.Replace("X;+1.25;", "X;1x25;"),
            CartesianResponse.Replace("Y;-2.50;", "X;-2.50;"),
            CartesianResponse.Replace("C;540;", "").Replace("L1;7.0;", ""),
            CartesianResponse.Replace("X;+1.25;", "L2;8.0;"),
            CartesianResponse.Replace("Z;3;", "L2;8.0;"),
            CartesianResponse.Replace("6,4294967295", "256,0"),
            CartesianResponse.Replace("6,4294967295", "6,4294967296"),
            CartesianResponse.Replace("0.00;00000000", "1e9999;00000000")
        };
        foreach (var raw in invalidP)
        {
            var error = Assert.Throws<PositionParseException>(() => PositionP.FromCommand(raw));
            Assert.Equal(raw, error.RawValue);
            Assert.Equal(typeof(PositionP), error.TargetType);
        }
    }

    [Theory]
    [InlineData(270, -90)]
    [InlineData(-270, 90)]
    [InlineData(540, -180)]
    [InlineData(-540, -180)]
    [InlineData(180, -180)]
    [InlineData(360, 0)]
    [InlineData(-720, 0)]
    public void AnglesNormalizeAndHashConsistently(double input, double normalized)
    {
        var p = new PositionP(1, 2, 3, input, input, input, 4, 5, PositionConfig.RAF, 42);
        var equivalent = new PositionP(1, 2, 3, normalized, normalized, normalized, 4, 5, PositionConfig.RAF, 42);
        Assert.Equal(normalized, p.A);
        Assert.True(p == equivalent);
        Assert.True(p.Equals((object)equivalent));
        Assert.Equal(p.GetHashCode(), equivalent.GetHashCode());
        Assert.Contains(equivalent, new HashSet<PositionP> { p });
        Assert.False(p.Equals(null));
        Assert.False(p.Equals("not a position"));
        Assert.True(p != new PositionP(9, 2, 3, input, input, input, 4, 5, PositionConfig.RAF, 42));
    }

    [Fact]
    public void NanAndInfinityPreserveReflexiveEqualityAndHashing()
    {
        var j = new PositionJ(double.NaN, double.PositiveInfinity, -0.0, 0, 0, 0, 0, 0);
        var equalJ = new PositionJ(double.NaN, double.PositiveInfinity, 0.0, 0, 0, 0, 0, 0);
        Assert.True(j == equalJ);
        Assert.Equal(j.GetHashCode(), equalJ.GetHashCode());
        Assert.Contains(equalJ, new HashSet<PositionJ> { j });
        Assert.False(j.Equals("other"));
        Assert.True(j != new PositionJ(1, double.PositiveInfinity, 0, 0, 0, 0, 0, 0));

        var p = new PositionP(double.NaN, 0, 0, double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0, 0, 0, 0);
        var equalP = new PositionP(double.NaN, 0, 0, double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0, 0, 0, 0);
        Assert.Equal(p, equalP);
        Assert.Equal(p.GetHashCode(), equalP.GetHashCode());
        Assert.Contains(equalP, new HashSet<PositionP> { p });
    }
}
