using ODExplorer.Controls;
using ODExplorer.ViewModels.ModelVMs;

namespace ODExplorer.Models
{
    public sealed class PopOutParams
    {
        public string Title { get; set; } = string.Empty;
        public int Count { get; set; }
        public WindowPositionViewModel Position { get; set; } = new();
        public PopOutMode Mode { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool ShowTitle { get; set; } = true;
        public bool ShowInTaskBar { get; set; } = true;
        public bool Active { get; set; }
        public object? AdditionalSettings { get; set; }
        public double ZoomLevel { get; set; } = 1d;

        public static PopOutParams CreateParams(PopOutBase popOut, int count, bool active)
        {
            return new()
            {
                Title = popOut.Title,
                Count = count,
                Position = popOut.Position.Clone(),
                Mode = popOut.Mode,
                AlwaysOnTop = popOut.AlwaysOnTop,
                ShowTitle = popOut.ShowTitle,
                ShowInTaskBar = popOut.ShowInTaskBar,
                Active = active,
                AdditionalSettings = popOut.AdditionalSettings,
                ZoomLevel = popOut.ZoomLevel,
            };
        }

        public void UpdateParams(PopOutBase popOut, bool active)
        {
            Position = popOut.Position.Clone();
            Mode = popOut.Mode;
            AlwaysOnTop = popOut.AlwaysOnTop;
            ShowTitle = popOut.ShowTitle;
            ShowInTaskBar = popOut.ShowInTaskBar;
            Active = active;
            AdditionalSettings = popOut.AdditionalSettings;
            ZoomLevel = popOut.ZoomLevel;
        }
    }
}
