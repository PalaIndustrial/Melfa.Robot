using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly partial struct RobotInformation
    {
        /// <summary>XYZ axes pattern</summary>
        /// <remarks>
        ///     Bit 0: X<br />
        ///     Bit 1: Y<br />
        ///     Bit 2: Z<br />
        ///     Bit 3: A<br />
        ///     Bit 4: B<br />
        ///     Bit 5: C<br />
        ///     Bit 6: L1<br />
        ///     Bit 7: L2
        /// </remarks>
        public byte AxisPatternXYZ { get; }

        /// <summary>Joint axes pattern</summary>
        /// <remarks>
        ///     Bit 0: J1<br />
        ///     Bit 1: J2<br />
        ///     Bit 2: J3<br />
        ///     Bit 3: J4<br />
        ///     Bit 4: J5<br />
        ///     Bit 5: J6<br />
        ///     Bit 6: J7<br />
        ///     Bit 7: J8
        /// </remarks>
        public byte AxisPatternJoint { get; }
        public int StructuralFlag1 { get; }
        public int StructuralFlag2 { get; }
        public ImmutableArray<byte> JogSpeed { get; }
        public string ProgramExtension { get; }
        public string ParameterExtension { get; }
        public string RobotType { get; }
        public string Controller { get; }
        public string Series { get; }
        public DateTime ReleaseDate { get; }
        public string Version { get; }
        public string Language { get; }
        public string SerialNumber { get; }
        public string Copyright { get; }
        public string RobotInfo { get; }
        public int MaximumTaskCount { get; }

        public RobotInformation(byte axisPatternXYZ, byte axisPatternJoint, int structuralFlag1, int structuralFlag2,
            ImmutableArray<byte> jogSpeed, string programExtension, string parameterExtension, string robotType,
            string controller, string series, DateTime releaseDate, string version, string language,
            string copyright, string robotInfo, string serialNumber, int maximumTaskCount)
        {
            AxisPatternXYZ = axisPatternXYZ;
            AxisPatternJoint = axisPatternJoint;
            StructuralFlag1 = structuralFlag1;
            StructuralFlag2 = structuralFlag2;
            JogSpeed = jogSpeed;
            ProgramExtension = programExtension;
            ParameterExtension = parameterExtension;
            RobotType = robotType;
            Controller = controller;
            Series = series;
            ReleaseDate = releaseDate;
            Version = version;
            Language = language;
            Copyright = copyright;
            RobotInfo = robotInfo;
            SerialNumber = serialNumber;
            MaximumTaskCount = maximumTaskCount;
        }

        // QokFF;FF;7,0;3,5,A,1E,32,46,64;MB5;PRM;RV-13FLM-D;CRnX-7xx;MELFA;17-10-06;Ver.S7e;ENG;COPYRIGHT(C)2008-2017 MITSUBISHI ELECTRIC CORPORATION ALL RIGHTS RESERVED;1;3;8;
        public RobotInformation(string raw)
        {
            var match = RegexHelper.InfoRegex().Match(raw);
            AxisPatternXYZ = byte.Parse(match.Groups["P_AXIS_PATTERN"].Value, NumberStyles.HexNumber);
            AxisPatternJoint = byte.Parse(match.Groups["J_AXIS_PATTERN"].Value, NumberStyles.HexNumber);
            StructuralFlag1 = int.Parse(match.Groups["FLG1"].Value);
            StructuralFlag2 = int.Parse(match.Groups["FLG2"].Value);
            JogSpeed = new byte[] {
                byte.Parse(match.Groups["JSPD1"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD2"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD3"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD4"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD5"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD6"].Value, NumberStyles.HexNumber),
                byte.Parse(match.Groups["JSPD7"].Value, NumberStyles.HexNumber),
            }.ToImmutableArray();
            ProgramExtension = match.Groups["PROG_EXT"].Value;
            ParameterExtension = match.Groups["PRM_EXT"].Value;
            RobotType = match.Groups[nameof(RobotType)].Value;
            Controller = match.Groups[nameof(Controller)].Value;
            Series = match.Groups[nameof(Series)].Value;
            ReleaseDate = new DateTime(
                int.Parse(match.Groups["DATE_YEAR"].Value) + 2000,
                int.Parse(match.Groups["DATE_MONTH"].Value),
                int.Parse(match.Groups["DATE_DAY"].Value)
            );
            Version = match.Groups[nameof(Version)].Value;
            Language = match.Groups[nameof(Language)].Value;
            SerialNumber = match.Groups[nameof(SerialNumber)].Value;
            Copyright = match.Groups[nameof(Copyright)].Value;
            RobotInfo = match.Groups["ROBOT_INFO"].Value;
            MaximumTaskCount = int.Parse(match.Groups["TASKMAX"].Value);
        }

        /// <summary>Formats the information as an OPEN response payload.</summary>
        public override string ToString()
        {
            var jogSpeed = JogSpeed.IsDefault ? "" : string.Join(",", JogSpeed.Select(speed => speed.ToString("X", CultureInfo.InvariantCulture)));
            return string.Format(CultureInfo.InvariantCulture,
                "{0:X2};{1:X2};{2},{3};{4};{5};{6};{7};{8};{9};{10:yy-MM-dd};{11};{12};{13};{14};{15};{16};",
                AxisPatternXYZ, AxisPatternJoint, StructuralFlag1, StructuralFlag2, jogSpeed,
                ProgramExtension, ParameterExtension, RobotType, Controller, Series, ReleaseDate,
                Version, Language, Copyright, RobotInfo, SerialNumber, MaximumTaskCount);
        }
    }
}