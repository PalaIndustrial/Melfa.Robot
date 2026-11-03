using System.Collections.Generic;
using System.Collections.Immutable;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A page of variable data returned by VALS or GVALS.</summary>
    public sealed class VariablePage
    {
        internal VariablePage(ImmutableArray<KeyValuePair<string, string>> variables, bool hasMore)
        {
            Variables = variables;
            HasMore = hasMore;
        }

        /// <summary>Variable names and values in controller order.</summary>
        public ImmutableArray<KeyValuePair<string, string>> Variables { get; }

        /// <summary>Whether the controller reports more variables.</summary>
        public bool HasMore { get; }
    }
}
