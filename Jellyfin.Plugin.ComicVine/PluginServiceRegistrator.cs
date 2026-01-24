using Jellyfin.Plugin.ComicVine.Cache;
using Jellyfin.Plugin.ComicVine.Providers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ComicVine
{
    /// <summary>
    /// Register Bookshelf services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<IComicVineMetadataCacheManager, ComicVineMetadataCacheManager>();
            serviceCollection.AddSingleton<IComicVineApiKeyProvider, ComicVineApiKeyProvider>();
        }
    }
}
