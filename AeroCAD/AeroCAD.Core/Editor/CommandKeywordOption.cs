using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandKeywordOption
    {
        public CommandKeywordOption(string name, IEnumerable<string> aliases = null, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Keyword name is required.", nameof(name));

            Name = name.Trim().ToUpperInvariant();
            Aliases = (aliases ?? Enumerable.Empty<string>())
                .Append(Name)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => alias.Trim().ToUpperInvariant())
                .Distinct()
                .ToList()
                .AsReadOnly();
            Description = description ?? string.Empty;
        }

        public string Name { get; }

        public IReadOnlyList<string> Aliases { get; }

        public string Description { get; }

        /// <summary>
        /// AutoCAD-style display name: the shortest alias is shown in UPPERCASE,
        /// remaining characters in lowercase. E.g. "CLOSE"+"C" → "Close", "CLOSE"+"CL" → "CLose".
        /// </summary>
        public string DisplayName
        {
            get
            {
                var shortAlias = Aliases
                    .Where(a => a != Name)
                    .OrderBy(a => a.Length)
                    .FirstOrDefault();

                int upperLen = shortAlias != null
                    ? Math.Min(shortAlias.Length, Name.Length)
                    : 1;

                return Name.Substring(0, upperLen) + Name.Substring(upperLen).ToLowerInvariant();
            }
        }

        public bool Matches(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim().ToUpperInvariant();
            return Aliases.Contains(normalized);
        }
    }
}

