using System.Collections.Generic;
using Jellyfin.Plugin.ComicVine.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ComicVine.Providers;

/// <summary>
/// Url provider for ComicVine.
/// </summary>
public class ComicVineExternalUrlProvider : IExternalUrlProvider
{
    private readonly ILogger<ComicVineExternalUrlProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComicVineExternalUrlProvider"/> class.
    /// </summary>
    /// <param name="logger">Some logger.</param>
    public ComicVineExternalUrlProvider(ILogger<ComicVineExternalUrlProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string Name => ComicVineConstants.ProviderName;

    /// <inheritdoc />
    public IEnumerable<string> GetExternalUrls(BaseItem item)
    {
        _logger.LogInformation("Getting information for {Item}", item.Name);
        if (item.TryGetProviderId(ComicVineConstants.ProviderId, out var externalId))
        {
            switch (item)
            {
                case Person:
                case Book:
                    yield return $"{ComicVineApiUrls.BaseWebsiteUrl}/{externalId}";
                    break;
            }
        }
    }
}
