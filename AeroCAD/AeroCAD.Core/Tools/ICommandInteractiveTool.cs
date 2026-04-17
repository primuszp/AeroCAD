using Primusz.AeroCAD.Core.Editor;
using System.Windows;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface ICommandInteractiveTool
    {
        bool TrySubmitToken(CommandInputToken token);

        bool TrySubmitText(string input);

        bool TrySubmitPoint(Point point);

        bool TryComplete();

        bool TryCancel();
    }
}

