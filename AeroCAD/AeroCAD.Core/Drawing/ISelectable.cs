using System;

namespace Primusz.AeroCAD.Core.Drawing
{
    interface ISelectable
    {
        void Select();

        void Unselect();

        bool IsSelected { get; }
    }
}
