using System.Collections.Generic;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class MenuGroupViewModel
    {
        public MenuGroupViewModel(string groupName, IReadOnlyList<MenuItemViewModel> items)
        {
            GroupName = groupName;
            Items = items;
        }

        public string GroupName { get; }
        public IReadOnlyList<MenuItemViewModel> Items { get; }
    }
}
