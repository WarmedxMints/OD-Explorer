using EliteJournalReader;
using ODExplorer.Controls;
using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Database.DTOs;
using ODUtils.Database.Interfaces;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace ODExplorer.Stores
{
    public sealed class SettingsStore
    {
        public event EventHandler<bool>? MinimiseToTrayChaned;
        public event EventHandler? MinExoValueChanged;

        public SettingsStore(IOdToolsDatabaseProvider odToolsDatabaseProvider)
        {
            databaseProvider = odToolsDatabaseProvider;
            Instance ??= this;
        }

        private readonly IOdToolsDatabaseProvider databaseProvider;

        private static SettingsStore? instance;
        public static SettingsStore? Instance { get => instance; set => instance = value; }
        public int SelectedCommanderID { get; set; } = 0;
        public WindowPositionViewModel WindowPosition { get; set; } = new();
        public JournalLogAge JournalAge { get; set; } = JournalLogAge.Oneyear;
        public ExoBiologyViewState BiologyViewState { get; set; } = ExoBiologyViewState.CheckList;
        public CartoViewState CartoViewState { get; set; } = CartoViewState.DetailedView;
        public CartoDetailsViewState CartoDetailsViewState { get; set; } = CartoDetailsViewState.Unsold;
        public GalacticRegions ExoCheckListRegion { get; set; } = GalacticRegions.Unknown;
        public ActiveViewModel ActiveView { get; set; } = ActiveViewModel.Carto;
        public CodexEntryHistory CodexEntryHistory { get; set; } = CodexEntryHistory.Regional;
        public GridSize CartoHorizontalGridSize { get; set; } = new() { GridLengths = [new(1, GridUnitType.Star), new(23, GridUnitType.Pixel), new(1, GridUnitType.Star)] };
        public GridSize CartoDetailedGridSize { get; set; } = new()
        {
            GridLengths = [new(1, GridUnitType.Star), new(23, GridUnitType.Pixel), new(1, GridUnitType.Star),
                            new(1, GridUnitType.Star),new(23, GridUnitType.Pixel), new(4, GridUnitType.Star)]
        };
        public GridSize ExtendedBodyInfoGridSize { get; set; } = new() { GridLengths = [new(3, GridUnitType.Star), new(23, GridUnitType.Pixel), new(1, GridUnitType.Star)] };
        public GridSize CurrentExoGridSize { get; set; } = new() { GridLengths = [new(2, GridUnitType.Star), new(23, GridUnitType.Pixel), new(1, GridUnitType.Star)] };
        public SystemGridSettings SystemGridSetting { get; set; } = SystemGridSettings.DefaultValues();
        public NotificationSettings NotificationSettings { get; set; } = NotificationSettings.GetDefault();
        public NotificationOptions NotificationOptions { get; set; } = NotificationOptions.All;
        public double UiScale { get; set; } = 1;
        public SpanshCSVSettings SpanshCSVSettings { get; set; } = new();
        public NotableNotificationOptions NotableSettings { get; set; } = new();
        public DateTime JournalAgeDateTime
        {
            get
            {
                return JournalAge switch
                {
                    JournalLogAge.All => DateTime.MinValue,
                    JournalLogAge.SevenDays => DateTime.UtcNow.AddDays(-7),
                    JournalLogAge.ThirtyDays => DateTime.UtcNow.AddDays(-30),
                    JournalLogAge.SixtyDays => DateTime.UtcNow.AddDays(-60),
                    JournalLogAge.OneHundredEightyDays => DateTime.UtcNow.AddDays(-180),
                    _ => DateTime.UtcNow.AddYears(-((int)JournalAge - 4)),
                };
            }
        }
        public Dictionary<int, List<PopOutParams>> PopOutParams { get; set; } = [];
        public DateTime IgnoredCartoDate { get; set; } = DateTime.MinValue;
        public DateTime IgnoredExoDate { get; set; } = DateTime.MinValue;
        public bool MinimiseToTray { get; set; }
        #region Persistance
        public void LoadSettings()
        {
            var settings = databaseProvider.GetAllSettings();

            if (settings != null && settings.Count != 0)
            {
                SelectedCommanderID = SettingsDTO.SettingsDtoToInt(settings.GetSettingDTO(nameof(SelectedCommanderID)));
                UiScale = SettingsDTO.SettingsDtoToDouble(settings.GetSettingDTO(nameof(UiScale)), 1);
                WindowPosition = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(WindowPosition)), WindowPosition);
                CartoHorizontalGridSize = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(CartoHorizontalGridSize)), CartoHorizontalGridSize);
                CartoDetailedGridSize = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(CartoDetailedGridSize)), CartoDetailedGridSize);
                SystemGridSetting = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(SystemGridSetting)), SystemGridSetting);
                CurrentExoGridSize = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(CurrentExoGridSize)), CurrentExoGridSize);
                ExtendedBodyInfoGridSize = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(ExtendedBodyInfoGridSize)), ExtendedBodyInfoGridSize);
                NotificationSettings = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(NotificationSettings)), NotificationSettings);
                SpanshCSVSettings = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(SpanshCSVSettings)), SpanshCSVSettings);
                PopOutParams = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(PopOutParams)), PopOutParams);
                NotableSettings = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(NotableSettings)), NotableSettings);
                JournalAge = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(JournalAge)), JournalAge);
                BiologyViewState = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(BiologyViewState)), BiologyViewState);
                CartoViewState = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(CartoViewState)), CartoViewState);
                CartoDetailsViewState = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(CartoDetailsViewState)), CartoDetailsViewState);
                ExoCheckListRegion = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(ExoCheckListRegion)), ExoCheckListRegion);
                ActiveView = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(ActiveView)), ActiveView);
                NotificationOptions = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(NotificationOptions)), NotificationOptions);
                CodexEntryHistory = SettingsDTO.SettingDtoToEnum(settings.GetSettingDTO(nameof(CodexEntryHistory)), CodexEntryHistory);
                IgnoredCartoDate = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(IgnoredCartoDate)), DateTime.MinValue);
                IgnoredExoDate = SettingsDTO.SettingDtoToJson(settings.GetSettingDTO(nameof(IgnoredExoDate)), DateTime.MinValue);
                MinimiseToTray = SettingsDTO.SettingsDtoToBool(settings.GetSettingDTO(nameof(MinimiseToTray)), false);
            }

            if (WindowPosition.IsZero)
            {
                ResetWindowPosition();
            }
        }

        public void SaveSettings()
        {
            var settings = new List<SettingsDTO>
            {
                //Just in case someone closes the app while scanning a new directory
                SettingsDTO.IntToSettingsDTO(nameof(SelectedCommanderID), SelectedCommanderID > 0 ? SelectedCommanderID : 0),
                SettingsDTO.DoubleToSettingsDTO(nameof(UiScale), UiScale),
                SettingsDTO.ObjectToJsonStringDto(nameof(WindowPosition), WindowPosition),
                SettingsDTO.ObjectToJsonStringDto(nameof(CartoHorizontalGridSize), CartoHorizontalGridSize),
                SettingsDTO.ObjectToJsonStringDto(nameof(CartoDetailedGridSize), CartoDetailedGridSize),
                SettingsDTO.ObjectToJsonStringDto(nameof(SystemGridSetting), SystemGridSetting),
                SettingsDTO.ObjectToJsonStringDto(nameof(CurrentExoGridSize), CurrentExoGridSize),
                SettingsDTO.ObjectToJsonStringDto(nameof(ExtendedBodyInfoGridSize), ExtendedBodyInfoGridSize),
                SettingsDTO.ObjectToJsonStringDto(nameof(NotificationSettings), NotificationSettings),
                SettingsDTO.ObjectToJsonStringDto(nameof(SpanshCSVSettings), SpanshCSVSettings),
                SettingsDTO.ObjectToJsonStringDto(nameof(PopOutParams), PopOutParams),
                SettingsDTO.ObjectToJsonStringDto(nameof(NotableSettings), NotableSettings),
                SettingsDTO.EnumToSettingsDto(nameof(JournalAge), JournalAge),
                SettingsDTO.EnumToSettingsDto(nameof(BiologyViewState), BiologyViewState),
                SettingsDTO.EnumToSettingsDto(nameof(CartoViewState), CartoViewState),
                SettingsDTO.EnumToSettingsDto(nameof(CartoDetailsViewState), CartoDetailsViewState),
                SettingsDTO.EnumToSettingsDto(nameof(ExoCheckListRegion), ExoCheckListRegion),
                SettingsDTO.EnumToSettingsDto(nameof(ActiveView), ActiveView),
                SettingsDTO.EnumToSettingsDto(nameof(NotificationOptions), NotificationOptions),
                SettingsDTO.EnumToSettingsDto(nameof(CodexEntryHistory), CodexEntryHistory),
                SettingsDTO.ObjectToJsonStringDto(nameof(IgnoredCartoDate), IgnoredCartoDate),
                SettingsDTO.ObjectToJsonStringDto(nameof(IgnoredExoDate), IgnoredExoDate),
                SettingsDTO.BoolToSettingsDTO(nameof(MinimiseToTray), MinimiseToTray)
            };

            databaseProvider.AddSettings(settings);
        }
        #endregion

        #region Window Position 
        public void ResetWindowPosition()
        {
            ResetWindowPositionActual(WindowPosition);
        }

        public static void ResetWindowPositionActual(WindowPositionViewModel windowPosition, double windowWidth = 1800, double windowHeight = 1050)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                var left = (screenWidth / 2) - (windowWidth / 2);
                var top = (screenHeight / 2) - (windowHeight / 2);

                if (windowHeight > SystemParameters.VirtualScreenHeight)
                {
                    windowHeight = SystemParameters.VirtualScreenHeight;
                }

                if (windowWidth > SystemParameters.VirtualScreenWidth)
                {
                    windowWidth = SystemParameters.VirtualScreenWidth;
                }

                windowPosition.Top = top;
                windowPosition.Left = left;
                windowPosition.Width = windowWidth;
                windowPosition.Height = windowHeight;
                windowPosition.State = WindowState.Normal;
                return;
            }

            windowPosition.Top = 10;
            windowPosition.Left = 10;
            windowPosition.Width = 800;
            windowPosition.Height = 600;
            windowPosition.State = WindowState.Normal;
        }
        #endregion

        #region Popouts
        public List<PopOutParams> GetCommanderPopOutParams(int commanderId)
        {
            if (PopOutParams.TryGetValue(commanderId, out var outParams))
            {
                return outParams;
            }
            return [];
        }

        public PopOutParams GetParams(PopOutBase popOut, int knownCount, int commanderId)
        {
            var popOutParams = GetCommanderPopOutParams(commanderId);

            var count = popOutParams.Count(x => x.Title == popOut.Title);

            if (count == 0)
            {
                var ret = Models.PopOutParams.CreateParams(popOut, 1, true);
                ResetWindowPositionActual(ret.Position, 800, 450);
                popOutParams.Add(ret);
                return ret;
            }

            if (knownCount > 0)
            {
                var known = popOutParams.FirstOrDefault(x => x.Title == popOut.Title && x.Count == knownCount);

                if (known != null)
                {
                    return known;
                }
            }
            var haveParams = popOutParams.FirstOrDefault(x => x.Title == popOut.Title && x.Active == false);

            if (haveParams != null)
            {
                if (haveParams.Position.IsZero)
                    ResetWindowPositionActual(haveParams.Position, 800, 450);
                haveParams.Active = true;
                return haveParams;
            }

            haveParams = Models.PopOutParams.CreateParams(popOut, count + 1, true);
            if (haveParams.Position.IsZero)
                ResetWindowPositionActual(haveParams.Position, 800, 450);
            popOutParams.Add(haveParams);
            PopOutParams.TryAdd(commanderId, popOutParams);
            return haveParams;
        }

        public void SaveParams(PopOutBase popOut, bool active, int commanderId)
        {
            var popOutParams = GetCommanderPopOutParams(commanderId);

            var known = popOutParams.FirstOrDefault(x => x.Title == popOut.Title && x.Count == popOut.Count);

            if (known != null)
            {
                known.UpdateParams(popOut, active);
                return;
            }

            known = Models.PopOutParams.CreateParams(popOut, popOut.Count, active);
            popOutParams.Add(known);
            PopOutParams.TryAdd(commanderId, popOutParams);
        }

        public event EventHandler? OnSystemGridSettingsUpdatedEvent;
        internal void OnSystemGridSettingsUpdated()
        {
            OnSystemGridSettingsUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public void SetMinimiseToTray(bool value)
        {
            MinimiseToTray = value;
            MinimiseToTrayChaned?.Invoke(this, value);
        }

        internal void OnExoMinValueChanged()
        {
            MinExoValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
