using Microsoft.EntityFrameworkCore;
using ODExplorer.Database;
using ODUtils.APis;
using ODUtils.Database.Interfaces;
using ODUtils.Dialogs.ViewModels;
using ODUtils.IO;
using ODUtils.Models;
using System;
using System.Threading.Tasks;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class LoaderViewModel(IOdExplorerDBContextFactory dbContextFactory,
                                        EdAstroApiService edAstroApi,
                                        IOdToolsDatabaseProvider provider) : OdViewModelBase
    {
        private readonly IOdExplorerDBContextFactory dbContextFactory = dbContextFactory;
        private readonly EdAstroApiService edAstroApi = edAstroApi;
        private readonly IOdToolsDatabaseProvider provider = provider;

        private string statusText = "Loading...";
        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public Action? OnUpdateComplete;
        public Action<UpdateInfo>? OnUpdateAvailable;

        public async Task Run()
        {
            UpdateStatusText("Checking For App Updates");
            await Task.Delay(1000);

            var updateInfo = await Json.GetJsonFromUrlAndDeserialise<UpdateInfo>("https://raw.githubusercontent.com", "/WarmedxMints/ODUpdates/main/ODExplorerUpdate.json");

            if (updateInfo.Version > App.AppVersion)
            {
                OnUpdateAvailable?.Invoke(updateInfo);
                return;
            }

            await MigrateDatabase();
        }

        public async Task MigrateDatabase()
        {
            UpdateStatusText("Migrating Database");
            await Task.Delay(1000);
            using var dbContext = dbContextFactory.CreateDbContext();
            await dbContext.Database.MigrateAsync();

            if (provider is OdExplorerDatabaseProvider e)
            {
                try
                {
                    UpdateStatusText("Updating Points of Interest");
                    var pois = await edAstroApi.GetPois().ConfigureAwait(true);

                    if (pois.Count > 0)
                    {
                        e.AddEdAstroPois(pois);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatusText("Error updating Points of Interest");
                    App.Logger.Error(ex);
                }
            }
            OnUpdateComplete?.Invoke();
        }

        private void UpdateStatusText(string text)
        {
            StatusText = text;
        }
    }
}
