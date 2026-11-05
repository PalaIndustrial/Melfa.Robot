using System.Globalization;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly struct TaskSlotInfo
    {
        public int Slot { get; }
        /// <summary>Program name of slot table</summary>
        public string ProgramName { get; }
        /// <summary>Operation mode of slot table</summary>
        public TaskMode Mode { get; }
        /// <summary>Starting condition of slot table</summary>
        public TaskCondition Condition { get; }
        /// <summary>Priority of slot table (0 ~ 31)</summary>
        public int Priority { get; }

        public TaskSlotInfo(int slot, string programName, TaskMode mode, TaskCondition condition, int priority)
        {
            Slot = slot;
            ProgramName = programName;
            Mode = mode;
            Condition = condition;
            Priority = priority;
        }

        internal TaskSlotInfo(string raw, int slot)
        {
            Slot = slot;
            var values = raw.Split(';');
            ProgramName = values[0];
            switch (values[1])
            {
                case "CYC": Mode = TaskMode.Cycle; break;
                case "REP": Mode = TaskMode.Repeat; break;
                default: Mode = TaskMode.Unknown; break;
            }
            switch (values[2])
            {
                case "START": Condition = TaskCondition.Start; break;
                case "ALWAYS": Condition = TaskCondition.Always; break;
                case "ERROR": Condition = TaskCondition.Error; break;
                default: Condition = TaskCondition.Unknown; break;
            }
            Priority = int.Parse(values[3], CultureInfo.InvariantCulture);
        }

        /// <summary>Formats the slot as an SLOTRD response payload.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3}",
            ProgramName, FormatTaskMode(Mode), FormatTaskCondition(Condition), Priority);
    }
}