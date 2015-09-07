using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpCadCore.Model
{
    interface ISelectionService
    {
        IList<ISelectable> SelectedObjects { get; }
        void ClearSelection();

        bool HitTest(Point point);
        bool HitTest(Rect rect);
    }
}
