namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly struct TaskSlotInfo
    {
        private readonly string _protocolPayload;
        public int Slot { get; }
        /// <summary>Program name of slot table</summary>
        public string ProgramName { get; }
        /// <summary>Operation mode of slot table</summary>
        public TaskMode Mode { get; }
        /// <summary>Starting condition of slot table</summary>
        public TaskCondition Condition { get; }
        /// <summary>Priority of slot table (0 ~ 31)</summary>
        public int Priority { get; }

        internal TaskSlotInfo(string raw, int slot)
        {
            _protocolPayload = raw;
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
            Priority = int.Parse(values[3]);
        }

        public override string ToString() => RequireProtocolPayload(_protocolPayload);
    }
}