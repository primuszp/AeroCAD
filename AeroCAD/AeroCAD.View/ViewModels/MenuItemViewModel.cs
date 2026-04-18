using System.Windows.Input;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class MenuItemViewModel
    {
        public MenuItemViewModel(string label, ICommand command)
        {
            Label = label;
            Command = command;
        }

        public string Label { get; }
        public ICommand Command { get; }

        /// <summary>Label without the WPF access-key underscore prefix, suitable for toolbar buttons.</summary>
        public string DisplayLabel => Label?.TrimStart('_') ?? string.Empty;
    }
}
