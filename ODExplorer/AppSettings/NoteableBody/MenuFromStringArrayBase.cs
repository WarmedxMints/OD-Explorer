using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class MenuFromMenuInfoArrayBase : PropertyChangeNotify
    {
        public MenuFromMenuInfoArrayBase()
        {
            BuildMenu();
        }

        private ObservableCollection<MenuItem> menuItems = new();
        [IgnoreDataMember]
        public ObservableCollection<MenuItem> MenuItems { get => menuItems; set { menuItems = value; OnPropertyChanged(); } }
        [IgnoreDataMember]
        public virtual string[] MenuInfoArray { get; }

        [IgnoreDataMember]
        public string this[int index] => index < 0 || index > MenuInfoArray.Length ? null : MenuInfoArray[index];

        [IgnoreDataMember]
        protected virtual string InfoNullReturn => "Unknown";

        [IgnoreDataMember]
        public int Length => MenuInfoArray.Length;

        private List<int> indexes = new();
        public List<int> Indexes
        {
            get => indexes;
            set { indexes = value; UpdateMenuItems(); }
        }

        private void BuildMenu()
        {
            if (MenuInfoArray == null || MenuInfoArray.Length == 0)
            {
                return;
            }
           
            MenuItem selectallItem = new()
            {
                Header = "Select All",
                IsCheckable = true,
                StaysOpenOnClick = true,
                Tag = -1,               
            };

            selectallItem.Click += Menuitem_Click;
            menuItems.Add(selectallItem);

            if (MenuInfoArray is null || MenuInfoArray.Length == 0)
            {
                return;
            }

            for (int i = 0; i < MenuInfoArray.Length; i++)
            {
                string val = MenuInfoArray[i];

                MenuItem menuitem = new()
                {
                    Header = val,
                    IsCheckable = true,
                    StaysOpenOnClick = true,
                    Tag = i,
                };
                menuitem.Click += Menuitem_Click;
                menuItems.Add(menuitem);
            }
        }

        public void UpdateMenuItems()
        {
            if(indexes == null)
            {
                return;
            }

            bool selectAllChecked = indexes.Contains(-1) || indexes.Count == 0;

            foreach (MenuItem item in menuItems)
            {
                item.IsChecked = selectAllChecked || indexes.Contains((int)item.Tag);
            }
        }

        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            int index = (int)item.Tag;

            if (index < 0)
            {
                foreach (MenuItem mItem in menuItems)
                {
                    if (mItem == item)
                    {
                        continue;
                    }
                    mItem.IsChecked = item.IsChecked;
                }
            }
            else
            {
                MenuItem selectAll = menuItems.First(x => (int)x.Tag == -1);

                selectAll.IsChecked = false;
            }
            e.Handled = true;

            UpdateList();
        }

        private void UpdateList()
        {
            if (indexes.Count == 0)
            {
                indexes.Add(-1);
            }
            else
            {
                indexes.Clear();
            }

            bool onechecked = false;

            foreach (MenuItem mItem in menuItems)
            {
                if (mItem.IsChecked && (int)mItem.Tag == -1)
                {
                    indexes.Clear();
                    break;
                }

                if (mItem.IsChecked)
                {

                    indexes.Add((int)mItem.Tag);

                    onechecked = true;
                }
            }

            if (onechecked == false)
            {
                foreach (MenuItem item in menuItems)
                {
                    item.IsChecked = true;
                }
                indexes.Add(-1);
            }
        }

        protected virtual string StringInfoOperations(string infoString)
        {
            return infoString;
        }

        public string GetInfoString(string infoString)
        {
            if (string.IsNullOrEmpty(infoString))
            {
                return InfoNullReturn;
            }

            infoString = StringInfoOperations(infoString);

            for (int i = 0; i < Length; i++)
            {
                if (infoString.Equals(this[i], StringComparison.OrdinalIgnoreCase))
                {
                    return this[i];
                }
            }

            return "Unknown";
        }


        public bool StringoInfoSelected(string stringinfo)
        {
            if (indexes.Contains(-1) || Indexes.Count == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(stringinfo))
            {
                return false;
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] < 0)
                {
                    continue;
                }

                if (stringinfo.Equals(this[indexes[i]], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetInfoIndex(string atmosphere)
        {
            if (string.IsNullOrEmpty(atmosphere))
            {
                return -2;
            }

            for (int i = 0; i < Length; i++)
            {
                if (atmosphere.Equals(this[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -2;
        }
    }
}
