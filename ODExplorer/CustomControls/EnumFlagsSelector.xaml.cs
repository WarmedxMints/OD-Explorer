using ODExplorer.Utils.Converters;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Interaction logic for EnumFlagsSelector.xaml
    /// </summary>
    public partial class EnumFlagsSelector : Button
    {
        private readonly ObservableCollection<MenuItem> menuItems = new();

        #region DependencyProperty EnumValue
        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register("EnumValue", typeof(int), typeof(EnumFlagsSelector),
            new PropertyMetadata(0, new PropertyChangedCallback(EnumValueChanged)));
        private static void EnumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EnumFlagsSelector)d).EnumValue = (int)e.NewValue;
        }
        /// <summary>
        /// Gets and sets the int representation of the checked items. Bind to this, using the FlagsIntConverter.
        /// </summary>
        public int EnumValue
        {
            get => (int)GetValue(EnumValueProperty);
            set
            {
                if (menuItems.Count == 0)
                {
                    return;
                }

                SetValue(EnumValueProperty, value);
                foreach (MenuItem item in menuItems) //This is redundant when item.Click triggered it and no overlapping values exist.
                {
                    item.IsChecked = (value & (int)item.Tag) == (int)item.Tag;
                }
            }
        }
        #endregion DependencyProperty EnumValue

        #region DependencyProperty Check
        public static readonly DependencyProperty CheckHandlerProperty = DependencyProperty.Register("Check", typeof(RoutedEventHandler), typeof(EnumFlagsSelector));

        /// <summary>The custom event handler to execute whenever a selection is checked/unchecked.
        /// </summary>
        public RoutedEventHandler Check
        {
            get => (RoutedEventHandler)GetValue(CheckHandlerProperty);
            set => SetValue(CheckHandlerProperty, value);
        }
        #endregion DependencyProperty Check

        public EnumFlagsSelector()
        {
            InitializeComponent();
            menu.ItemsSource = menuItems;
        }

        public Type ChoicesSource
        {
            set
            {
                if (!value.IsEnum || value.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
                {
                    throw new ArgumentException($"Type '{value.Name}' is not an enum with the [Flags] attribute.", nameof(ChoicesSource));
                }

                if (menuItems.Count == 0)
                {
                    Array enumVals = Enum.GetValues(value);

                    Array.Sort(enumVals);

                    // Create a MenuItem for each defined value of the enum
                    foreach (object val in enumVals)
                    {
                        MenuItem menuitem = new()
                        {
                            Header = EnumDescriptionConverter.GetEnumDescription((Enum)val),
                            IsCheckable = true,
                            StaysOpenOnClick = true,
                            Tag = Enum.ToObject(value, val),
                            Style = this.FindResource("EnumFlagMenuItem") as Style
                        };
                        menuitem.Click += Menuitem_Click;
                        menuItems.Add(menuitem);
                    }
                    // In an expandable grid row, EnumValue gets set before ChoicesSource is set.
                    // Repeat the assignment now to get the new menuItems appropriately checked and the button label updated.
                    if (EnumValue != 0)
                    {
                        EnumValue = (int)GetValue(EnumValueProperty); //This is intentionally a redundant assignment.
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot redefine ChoicesSource.");
                }
            }
        }

        /// <summary>Called whenever a dropdown item is clicked, changing its IsChecked state.
        /// </summary>
        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {
            Check?.Invoke(sender, e); //Call any added Check handler.
            // Update EnumValue
            MenuItem item = sender as MenuItem;
            SetValue(EnumValueProperty, item.IsChecked ? EnumValue | (int)item.Tag : EnumValue & ~(int)item.Tag);

            e.Handled = true;
        }

        /// <summary>Called when the context menu closes. Update the source bound to EnumValue.
        /// </summary>
        private void Menu_Closed(object sender, RoutedEventArgs e)
        {
            if(EnumValue == 0)
            {
                EnumValue = -1;
            }

            GetBindingExpression(EnumValueProperty).UpdateSource();
        }

        /// <summary>Handle the button click event. Open the context menu if it was not already open.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // IsOpen will always be false here, but IsVisible will give us the menu state at the time the button was clicked.
            if (!menu.IsVisible)
            {
                menu.PlacementTarget = (Button)sender;
                menu.VerticalOffset = 5;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                //menu.Width = ((Button)sender).ActualWidth + 5;
                menu.IsOpen = true;
            }
        }

        //Disable right click to open context menu
        private void Root_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
