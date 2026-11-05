using System.Globalization;
using System.Text.RegularExpressions;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    public readonly struct HandState
    {
        /// <summary>Signal number allocated in hand</summary>
        /// <remarks>-1: not used</remarks>
        public int SignalNumber { get; }
        /// <summary>Hand output status</summary>
        public bool? State { get; }
        /// <summary>Hand type</summary>
        public HandType HandType { get; }

        public HandState(int signalNumber, bool? state, HandType handType)
        {
            SignalNumber = signalNumber;
            State = state;
            HandType = handType;
        }

        internal HandState(Match match)
        {
            SignalNumber = int.Parse(match.Groups[nameof(SignalNumber)].Value, CultureInfo.InvariantCulture);
            switch (int.Parse(match.Groups[nameof(State)].Value, CultureInfo.InvariantCulture))
            {
                case 1: State = true; break;
                case 2: State = false; break;
                default: State = null; break;
            }
            HandType = (HandType)int.Parse(match.Groups[nameof(HandType)].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>Formats the hand as one <c>signal;state;type</c> triple of an HNDSTS response.</summary>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}",
            SignalNumber, State switch { true => 1, false => 2, null => -1 }, (int)HandType);
    }
}