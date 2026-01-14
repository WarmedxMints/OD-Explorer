using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ODExplorer.Database;
using ODExplorer.Windows;
using ODUtils.Database.Interfaces;
using ODUtils.ViewModelNavigation;
using ODExplorer.Stores;
using ODUtils.Journal;
using ODUtils.Dialogs.ViewModels;
using System;
using ODExplorer.ViewModels.ViewVMs;
using ODUtils.APis;
using System.Net.Http.Headers;
using System.Net.Http;
using ODUtils.Exobiology;

namespace ODExplorer.Extensions
{
    public static class HostBuilderExtentions
    {
        public static IHostBuilder AddDatabase(this IHostBuilder hostBuilder, string connectionString)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //Database
                services.AddSingleton<IOdExplorerDBContextFactory>(new OdExplorerDbContextFactory(connectionString));
                services.AddSingleton<IOdToolsDatabaseProvider, OdExplorerDatabaseProvider>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<LoaderViewModel>();
                services.AddTransient<LoadingViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<NavigationViewModel>();
                services.AddTransient<CartographicViewModel>();
                services.AddTransient<OrganicViewModel>();
                services.AddTransient<DisplaySettingsViewModel>();
                services.AddTransient<CartoDetailsViewModel>();
                services.AddTransient<SpanshViewModel>();
                services.AddTransient<EdAstroViewModel>();
                services.AddTransient(s => CreateSettingViewModel(s));
            });

            return hostBuilder;
        }

        public static IHostBuilder AddWindows(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<LoaderWindow>();
                services.AddSingleton<MainWindow>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                //Stores
                services.AddSingleton<OdNavigationStore>();
                services.AddSingleton<SettingsStore>();
                services.AddSingleton<JournalParserStore>();
                services.AddSingleton<ExplorationDataStore>();
                services.AddSingleton<OrganicCheckListDataStore>();
                services.AddSingleton<NotificationStore>();
                services.AddSingleton<SpanshCsvStore>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<JournalEventParser>();
                services.AddSingleton<ExoData>();
            });
            return hostBuilder;
        }

        public static IHostBuilder AddNavigation(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<OdNavigationStore>();
                AddViewModelNavigation<LoadingViewModel>(services);
                AddViewModelNavigation<CartographicViewModel>(services);
                AddViewModelNavigation<OrganicViewModel>(services);
                AddViewModelNavigation<DisplaySettingsViewModel>(services);
                AddViewModelNavigation<CartoDetailsViewModel>(services);
                AddViewModelNavigation<SpanshViewModel>(services);
                AddViewModelNavigation<EdAstroViewModel>(services);
                services.AddSingleton<Func<SettingsViewModel>>((s) => () => CreateSettingViewModel(s));
                services.AddSingleton<OdNavigationService<SettingsViewModel>>();
            });

            return hostBuilder;
        }

        private static void AddViewModelNavigation<TViewModel>(IServiceCollection services) where TViewModel : OdViewModelBase
        {
            services.AddSingleton<Func<TViewModel>>((s) => () => s.GetRequiredService<TViewModel>());
            services.AddSingleton<OdNavigationService<TViewModel>>();
        }

        private static SettingsViewModel CreateSettingViewModel(IServiceProvider services)
        {
            return SettingsViewModel.CreateViewModel(services.GetRequiredService<SettingsStore>(),
                services.GetRequiredService<IOdToolsDatabaseProvider>(),
                services.GetRequiredService<NavigationViewModel>(),
                services.GetRequiredService<JournalParserStore>());
        }

        public static IHostBuilder AddHttpClients(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient<EdsmApiService>((httpClient) =>
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.BaseAddress = new Uri("https://www.edsm.net/");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromSeconds(5),
                        ConnectTimeout = TimeSpan.FromSeconds(10),
                    };
                });

                services.AddHttpClient<EdAstroApiService>((httpClient) =>
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.BaseAddress = new Uri("https://edastro.com/gec/");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromSeconds(5),
                        ConnectTimeout = TimeSpan.FromSeconds(10),
                    };
                });
            });
            return hostBuilder;
        }
    }
}
