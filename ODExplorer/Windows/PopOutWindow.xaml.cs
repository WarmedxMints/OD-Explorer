using ODExplorer.Controls;
using ODExplorer.Models;
using ODUtils.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ODExplorer.Windows
{
    /// <summary>
    /// Interaction logic for PopOutWindow.xaml
    /// </summary>
    public partial class PopOutWindow : WindowBase
    {
        public new Thickness MaximisedThickness = new(5);
        public new Thickness NonMaximisedThickness = new(1);
        public PopOutWindow(PopOutBase popoutBase)
        {
            PopOutBase = popoutBase;
            PopOutBase.OnPopOutModeReset += PopOutBase_OnPopOutModeReset;
            InitializeComponent();            
        }

        private void PopOutBase_OnPopOutModeReset(object? sender, PopOutMode e)
        {
            SetNormal();
        }

        public PopOutBase PopOutBase { get; }

        private void PopOut_Loaded(object sender, RoutedEventArgs e)
        {
            Topmost = PopOutBase.AlwaysOnTop;
            MainGrid.Children.Add(PopOutBase);
            if (IsMouseOver is false)
            {
                ApplyStyles(false);
            }
        }

        private Thickness twenty = new(20, 0, 20, 20);

        private void PopOut_MouseEnter(object sender, MouseEventArgs e)
        {
            SetNormal();
            e.Handled = true;
        }

        private void SetNormal()
        {
            MainGrid.Margin = twenty;
            TitleBarGrid.Visibility = Visibility.Visible;
            TitlePanel.Visibility = Visibility.Collapsed;
            TitleBarGrid.Background = FindResource("TitleBarBackGround") as SolidColorBrush;
            MainGrid.Background = Background = FindResource("PrimaryBackground") as SolidColorBrush;
            MainWindowBorder.BorderBrush = FindResource("PrimaryControlBorder") as SolidColorBrush;
            PopOutBase.ApplyStyles(PopOutMode.Normal, true);
        }

        private void PopOut_MouseLeave(object sender, MouseEventArgs e)
        {
            ApplyStyles(false);
            e.Handled = true;
        }

        private void ApplyStyles(bool mouseEnter)
        {
            if (PopOutBase.Mode != PopOutMode.Normal)
            {
                var transparent = FindResource("Transparent") as SolidColorBrush;

                switch (PopOutBase.Mode)
                {
                    case PopOutMode.Normal:
                        return;
                    case PopOutMode.Opaque:
                        MainGrid.Background = FindResource("WindowBackground75%") as SolidColorBrush;
                        TitlePanel.Visibility = PopOutBase.ShowTitle ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case PopOutMode.Transparent:
                        MainGrid.Background = transparent;
                        TitlePanel.Visibility = PopOutBase.ShowTitle ? Visibility.Visible : Visibility.Collapsed;
                        break;
                }

                TitleBarGrid.Visibility = Visibility.Collapsed;
                TitleBarGrid.Background = transparent;
                MainWindowBorder.BorderBrush = transparent;
                Background = transparent;
            }

            PopOutBase.ApplyStyles(PopOutBase.Mode, mouseEnter);
        }

        private void TitleBarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    return;
                }
                this.DragMove();
                e.Handled = true;
            }
        }

        protected override void StateChangeRaised(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorderThickness = MaximisedThickness;
                RestoreButtonVisibility = Visibility.Visible;
                MaximiseButtonVisibility = Visibility.Collapsed;
                return;
            }
            MainWindowBorderThickness = NonMaximisedThickness;
            RestoreButtonVisibility = Visibility.Collapsed;
            MaximiseButtonVisibility = Visibility.Visible;
        }
    }
}
