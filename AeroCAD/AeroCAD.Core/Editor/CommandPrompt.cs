using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandPrompt
    {
        public static readonly CommandPrompt Default = new CommandPrompt("Command:");

        public CommandPrompt(string text, IEnumerable<string> options = null)
        {
            Text = string.IsNullOrWhiteSpace(text) ? Default.Text : text.Trim();
            Options = (options ?? Enumerable.Empty<string>())
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option.Trim())
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        public string Text { get; }

        public IReadOnlyList<string> Options { get; }

        public string ToDisplayString()
        {
            if (Options.Count == 0)
                return Text;

            return string.Format("{0} [{1}]", Text, string.Join("/", Options));
        }
    }
}

