using System.Windows;
using System.Windows.Controls;

namespace WpCadCore.Controls
{
    public interface IModelSpace : IInputElement
    {
        UIElementCollection Children { get; }
    }
}
