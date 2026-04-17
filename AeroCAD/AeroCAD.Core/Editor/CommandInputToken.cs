using System.Globalization;
using System.Windows;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandInputToken
    {
        private CommandInputToken(string rawText, CommandInputTokenKind kind, Point? pointValue, double? scalarValue, string textValue)
        {
            RawText = rawText ?? string.Empty;
            Kind = kind;
            PointValue = pointValue;
            ScalarValue = scalarValue;
            TextValue = textValue;
        }

        public string RawText { get; }

        public CommandInputTokenKind Kind { get; }

        public Point? PointValue { get; }

        public double? ScalarValue { get; }

        public string TextValue { get; }

        public bool IsEmpty => Kind == CommandInputTokenKind.Empty;

        public static CommandInputToken Empty()
        {
            return new CommandInputToken(string.Empty, CommandInputTokenKind.Empty, null, null, string.Empty);
        }

        public static CommandInputToken Point(string rawText, Point pointValue)
        {
            return new CommandInputToken(rawText, CommandInputTokenKind.Point, pointValue, null, null);
        }

        public static CommandInputToken Scalar(string rawText, double scalarValue)
        {
            return new CommandInputToken(rawText, CommandInputTokenKind.Scalar, null, scalarValue, null);
        }

        public static CommandInputToken Keyword(string rawText, string keyword)
        {
            return new CommandInputToken(rawText, CommandInputTokenKind.Keyword, null, null, keyword);
        }

        public static CommandInputToken Text(string rawText, string text)
        {
            return new CommandInputToken(rawText, CommandInputTokenKind.Text, null, null, text);
        }

        public string FormatForDisplay()
        {
            if (Kind == CommandInputTokenKind.Point && PointValue.HasValue)
                return string.Format(CultureInfo.InvariantCulture, "{0:0.###},{1:0.###}", PointValue.Value.X, PointValue.Value.Y);

            if (Kind == CommandInputTokenKind.Scalar && ScalarValue.HasValue)
                return ScalarValue.Value.ToString("0.###", CultureInfo.InvariantCulture);

            return RawText;
        }
    }
}

