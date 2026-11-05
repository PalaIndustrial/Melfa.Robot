using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly partial struct RunState
    {
        /// <summary>The task slot this state is associated with</summary>
        public int TaskSlot { get; }

        /// <summary>Program name loaded into task slot</summary>
        public string ProgramName { get; }
        /// <summary>Execution line number</summary>
        public int LineNumber { get; }
        /// <summary>A present override value is read</summary>
        public int Override { get; }
        public EditStatus EditStatus { get; }
        public RunStatus RunStatus { get; }
        public StopStatus StopStatus { get; }
        /// <summary>Error number (0 for no error)</summary>
        public int ErrorNumber { get; }
        /// <summary>Execution step number</summary>
        public int StepNumber { get; }
        public MechInfo MechInfo { get; }
        /// <summary>Program name of slot table</summary>
        public string TaskProgramName { get; }
        /// <summary>Operation mode of slot table</summary>
        public TaskMode TaskMode { get; }
        /// <summary>Stating conditions of slot table</summary>
        public TaskCondition TaskCondition { get; }
        /// <summary>Priority of slot table (1 ~ 31)</summary>
        public int TaskPriority { get; }
        /// <summary>Mech number under use</summary>
        public int MechNumber { get; }

        public RunState(int taskSlot, string programName, int lineNumber, int @override, EditStatus editStatus,
            RunStatus runStatus, StopStatus stopStatus, int errorNumber, int stepNumber, MechInfo mechInfo,
            string taskProgramName, TaskMode taskMode, TaskCondition taskCondition, int taskPriority, int mechNumber)
        {
            TaskSlot = taskSlot;
            ProgramName = programName;
            LineNumber = lineNumber;
            Override = @override;
            EditStatus = editStatus;
            RunStatus = runStatus;
            StopStatus = stopStatus;
            ErrorNumber = errorNumber;
            StepNumber = stepNumber;
            MechInfo = mechInfo;
            TaskProgramName = taskProgramName;
            TaskMode = taskMode;
            TaskCondition = taskCondition;
            TaskPriority = taskPriority;
            MechNumber = mechNumber;
        }

        internal RunState(string ret, int slot)
        {
            TaskSlot = slot;
            var match = RegexHelper.RunStateRegex().Match(ret);
            ProgramName = match.Groups[nameof(ProgramName)].Value;
            LineNumber = int.Parse(match.Groups[nameof(LineNumber)].Value);
            Override = int.Parse(match.Groups[nameof(Override)].Value);
            EditStatus = (EditStatus)int.Parse(match.Groups[nameof(EditStatus)].Value);
            RunStatus = (RunStatus)int.Parse(match.Groups[nameof(RunStatus)].Value, NumberStyles.HexNumber);
            StopStatus = (StopStatus)int.Parse(match.Groups[nameof(StopStatus)].Value, NumberStyles.HexNumber);
            ErrorNumber = int.Parse(match.Groups[nameof(ErrorNumber)].Value);
            StepNumber = int.Parse(match.Groups[nameof(StepNumber)].Value);
            MechInfo = (MechInfo)int.Parse(match.Groups[nameof(MechInfo)].Value);
            TaskProgramName = match.Groups[nameof(TaskProgramName)].Value;
            switch (match.Groups[nameof(TaskMode)].Value)
            {
                case "REP": TaskMode = TaskMode.Repeat; break;
                case "CYC": TaskMode = TaskMode.Cycle; break;
                default: TaskMode = TaskMode.Unknown; break;
            }
            switch (match.Groups[nameof(TaskCondition)].Value)
            {
                case "START": TaskCondition = TaskCondition.Start; break;
                case "ALWAYS": TaskCondition = TaskCondition.Always; break;
                case "ERROR": TaskCondition = TaskCondition.Error; break;
                default: TaskCondition = TaskCondition.Unknown; break;
            }
            TaskPriority = int.Parse(match.Groups[nameof(TaskPriority)].Value);
            MechNumber = int.Parse(match.Groups[nameof(MechNumber)].Value);
        }

        /// <summary>Formats the state as a STATE response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture,
            "{0};{1};{2};{3};{4:X2}{5:X2}{6};{7};{8};;;;;;;;{9};{10};{11};{12};{13}",
            ProgramName, LineNumber, Override, (int)EditStatus, (byte)RunStatus, (byte)StopStatus, ErrorNumber,
            StepNumber, (int)MechInfo, TaskProgramName, FormatTaskMode(TaskMode), FormatTaskCondition(TaskCondition),
            TaskPriority, MechNumber);
    }
}