using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly partial struct RunState
    {
        private readonly string _protocolPayload;
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

        internal RunState(string ret, int slot)
        {
            _protocolPayload = ret;
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

        public override string ToString() => RequireProtocolPayload(_protocolPayload);
    }
}