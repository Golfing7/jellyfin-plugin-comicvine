using Jellyfin.Plugin.ComicVine.Cache;
using Jellyfin.Plugin.ComicVine.Controllers;
using Jellyfin.Plugin.ComicVine.Providers;
using Jellyfin.Plugin.ComicVine.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ComicVine
{
    /// <summary>
    /// Register services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<FileTransformationStartupHook>();
            serviceCollection.AddSingleton<ComicVineApiService>();
            serviceCollection.AddSingleton<IComicVineMetadataCacheManager, ComicVineMetadataCacheManager>();
            serviceCollection.AddSingleton<IComicVineApiKeyProvider, ComicVineApiKeyProvider>();
        }
    }
}
