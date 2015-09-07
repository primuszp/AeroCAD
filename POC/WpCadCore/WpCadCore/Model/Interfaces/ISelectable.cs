using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpCadCore.Model
{
    interface ISelectable
    {
        void Select();
        void Unselect();
        bool IsSelected { get; }
    }
}
