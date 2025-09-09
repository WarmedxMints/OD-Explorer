using ODExplorer.Controls;
using ODExplorer.Models;
using ODUtils.Windows;
using System;
using System.Timers;
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
        private bool closing = false;
        private Rect windowRect = new();
        private readonly Timer checkForMouseTimer;
        private readonly Thickness twenty = new(20, 0, 20, 20);

        public new Thickness MaximisedThickness = new(5);
        public new Thickness NonMaximisedThickness = new(1);
        public PopOutBase PopOutBase { get; }

        public PopOutWindow(PopOutBase popoutBase)
        {
            PopOutBase = popoutBase;
            PopOutBase.OnPopOutModeReset += PopOutBase_OnPopOutModeReset;
            checkForMouseTimer = new(500);
            checkForMouseTimer.Elapsed += OnMouseTimer;
            Closing += PopOutWindow_Closing;
            InitializeComponent();
        }

        private void PopOutWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            checkForMouseTimer.Stop();
            checkForMouseTimer?.Dispose();
        }

        private void OnMouseTimer(object? sender, ElapsedEventArgs e)
        {
            //Get the mouse position
            var mousePos = System.Windows.Forms.Control.MousePosition;
            //Check if the mouse is within the window
            var containsMouse = windowRect.Contains(mousePos.X, mousePos.Y);

            if (containsMouse == false || PopOutBase.Mode != PopOutMode.Transparent)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    this.SetWindowNormal();
                    checkForMouseTimer.Stop();
                    SetNormal();
                    return;
                }
            });
        }

        private void PopOutBase_OnPopOutModeReset(object? sender, PopOutMode e)
        {
            this.SetWindowNormal();
            SetNormal();
        }

        private void PopOut_Loaded(object sender, RoutedEventArgs e)
        {
            Point screenCoordinates = this.PointToScreen(new Point(0, 0));
            windowRect = new(screenCoordinates.X, screenCoordinates.Y, this.Width, this.Height);
            Topmost = PopOutBase.AlwaysOnTop;
            MainGrid.Children.Add(PopOutBase);
            if (IsMouseOver is false)
            {
                ApplyStyles(false);
            }
        }

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
            if (!closing)
            {
                Point screenCoordinates = this.PointToScreen(new Point(0, 0));
                windowRect = new(screenCoordinates.X, screenCoordinates.Y, this.Width, this.Height);
                ApplyStyles(false);
            }
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
                    case PopOutMode.Semitransparent:
                        transparent = FindResource("SemiTransparent") as SolidColorBrush;
                        MainGrid.Background = transparent;
                        TitlePanel.Visibility = PopOutBase.ShowTitle ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case PopOutMode.Transparent:
                        if (mouseEnter == false)
                        {
                            this.SetWindowExTransparent();
                            checkForMouseTimer.Start();
                        }
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
