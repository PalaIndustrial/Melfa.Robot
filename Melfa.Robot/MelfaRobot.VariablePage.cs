using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Melfa.Robot;

public partial class MelfaRobot
{
    /// <summary>A page of variable data returned by VALS or GVALS.</summary>
    public sealed class VariablePage
    {
        public VariablePage(ImmutableArray<KeyValuePair<string, string>> variables, bool hasMore)
        {
            Variables = variables;
            HasMore = hasMore;
        }

        /// <summary>Variable names and values in controller order.</summary>
        public ImmutableArray<KeyValuePair<string, string>> Variables { get; }

        /// <summary>Whether the controller reports more variables.</summary>
        public bool HasMore { get; }

        /// <summary>Formats the page as a VALS or GVALS response payload, with entries separated by VT.</summary>
        public override string ToString()
        {
            var variables = Variables.IsDefault ? ImmutableArray<KeyValuePair<string, string>>.Empty : Variables;
            return string.Join("\x0b", variables.Select(variable => variable.Key + "=" + variable.Value)) +
                string.Format(CultureInfo.InvariantCulture, ";{0};{1}", variables.Length, HasMore ? 1 : 0);
        }
    }
}
