using System;

namespace Primusz.Cadves.Core.Drawing
{
    interface ISelectable
    {
        void Select();

        void Unselect();

        bool IsSelected { get; }
    }
}