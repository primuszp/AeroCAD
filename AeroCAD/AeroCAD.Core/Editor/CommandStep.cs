using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandStep
    {
        public CommandStep(
            string id,
            string prompt,
            IEnumerable<string> options = null,
            IEnumerable<CommandKeywordOption> keywords = null,
            CommandInputMode inputMode = CommandInputMode.Point)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "Step" : id.Trim();
            InputMode = inputMode;
            Keywords = (keywords ?? Enumerable.Empty<CommandKeywordOption>())
                .Where(keyword => keyword != null)
                .ToList()
                .AsReadOnly();

            var promptOptions = (options ?? Enumerable.Empty<string>())
                .Concat(Keywords.Select(keyword => keyword.DisplayName))
                .Distinct();

            Prompt = new CommandPrompt(prompt, promptOptions);
        }

        public string Id { get; }

        public CommandInputMode InputMode { get; }

        public CommandPrompt Prompt { get; }

        public IReadOnlyList<CommandKeywordOption> Keywords { get; }

        public bool TryResolveKeyword(CommandInputToken token, out CommandKeywordOption keyword)
        {
            keyword = null;
            if (token == null || Keywords.Count == 0)
                return false;

            var value = token.TextValue ?? token.RawText;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            keyword = Keywords.FirstOrDefault(option => option.Matches(value));
            return keyword != null;
        }
    }
}

