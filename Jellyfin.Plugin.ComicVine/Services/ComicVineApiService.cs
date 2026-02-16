using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions.Json;
using Jellyfin.Plugin.ComicVine.Common;
using Jellyfin.Plugin.ComicVine.Models;
using Jellyfin.Plugin.ComicVine.Providers;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ComicVine.Services;

public class ComicVineApiService
{
    private readonly ILogger<ComicVineApiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IComicVineApiKeyProvider _apiKeyProvider;

    public ComicVineApiService(
        ILogger<ComicVineApiService> logger,
        IHttpClientFactory httpClientFactory,
        IComicVineApiKeyProvider apiKeyProvider)
    {
        this._logger = logger;
        this._httpClientFactory = httpClientFactory;
        this._apiKeyProvider = apiKeyProvider;
    }

    /// <summary>
    /// Gets the json options for deserializing the Comic Vine API responses.
    /// </summary>
    protected JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions(JsonDefaults.Options)
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
    };

    public async Task<(IReadOnlyList<IssueSearch> Issues, BaseApiResponse<IssueSearch>? BaseResponse)> GetIssuesByVolumeId(string volumeId, int offset, CancellationToken cancellationToken)
    {
        var apiKey = _apiKeyProvider.GetApiKey();
        string trimmedId = volumeId[(volumeId.LastIndexOf('-') + 1)..];
        string formattedUrl = string.Format(CultureInfo.InvariantCulture, ComicVineApiUrls.IssuesByVolumeUrl, apiKey, WebUtility.UrlEncode(trimmedId), offset);

        var response = await _httpClientFactory
            .CreateClient(NamedClient.Default)
            .GetAsync(formattedUrl, cancellationToken)
            .ConfigureAwait(false);

        var apiResponse = await response.Content.ReadFromJsonAsync<SearchApiResponse<IssueSearch>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (apiResponse == null)
        {
            _logger.LogError("Failed to deserialize Comic Vine API response.");
            return ([], null);
        }

        return (GetFromApiResponse(apiResponse), apiResponse);
    }

    /// <summary>
    /// Get the results from the API response.
    /// </summary>
    /// <typeparam name="T">Type of the results.</typeparam>
    /// <param name="response">API response.</param>
    /// <returns>The results.</returns>
    protected IReadOnlyList<T> GetFromApiResponse<T>(BaseApiResponse<T> response)
    {
        if (response.IsError)
        {
            _logger.LogError("Comic Vine API response received with error code {ErrorCode} : {ErrorMessage}", response.StatusCode, response.Error);
            return Array.Empty<T>();
        }

        if (response is SearchApiResponse<T> searchResponse)
        {
            return searchResponse.Results.ToList();
        }
        else if (response is ItemApiResponse<T> itemResponse)
        {
            return itemResponse.Results == null ? Array.Empty<T>() : [itemResponse.Results];
        }
        else
        {
            return Array.Empty<T>();
        }
    }
}
