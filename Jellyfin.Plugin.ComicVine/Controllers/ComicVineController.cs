using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using J2N.Globalization;
using Jellyfin.Data;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.ComicVine.Providers;
using Jellyfin.Plugin.ComicVine.Services;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ComicVine.Controllers;

[Route("ComicVine")]
[ApiController]
public class ComicVineController : ControllerBase
{
    private static readonly Regex _fileNameIssueNumber = new Regex(@"(?<issueNumber>[0-9]+)");
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ComicVineController> _logger;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly ComicVineApiService _cvService;
    private readonly ILibraryManager _libraryManager;

    public ComicVineController(
        IHttpClientFactory httpClientFactory,
        ILogger<ComicVineController> logger,
        IUserManager userManager,
        IUserDataManager userDataManager,
        ComicVineApiService cvService,
        ILibraryManager libraryManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _cvService = cvService;
        _libraryManager = libraryManager;
    }

    [HttpPost("TagFolder")]
    public async Task<ActionResult> TagFolder([FromQuery] string folderId, [FromQuery] string cvVolumeId)
    {
        var folder = _libraryManager.GetItemById(new Guid(folderId)) as Folder;
        if (folder == null)
        {
            return NotFound("Folder not found");
        }

        int results = 0, totalMatches = 0;
        int currentOffset = 0;
        do
        {
            (results, int matches) = await TagFolderInternal(folder, cvVolumeId, currentOffset)
                .ConfigureAwait(false);
            totalMatches += matches;
            currentOffset += 100;
        }
        while (currentOffset < results);

        _logger.LogInformation("Tagged {TotalMatches} items!", totalMatches);
        return Ok("Tagged items successfully.");
    }

    private async Task<(int NumberOfResults, int TotalMatches)> TagFolderInternal(Folder folder, string cvVolumeId, int offset)
    {
        // 1. Fetch all issues for this volume from ComicVine
        // Note: Ensure your service handles pagination if the volume has >100 issues
        var cvIssues = await _cvService.GetIssuesByVolumeId(cvVolumeId, offset, CancellationToken.None)
            .ConfigureAwait(false);

        if (cvIssues.BaseResponse == null)
        {
            return (-1, -1);
        }

        // 2. Get local items in the folder
        var user = _userManager.Users.First(source => source.HasPermission(PermissionKind.IsAdministrator));
        var localItems = folder.GetChildren(user, false, new InternalItemsQuery
        {
            Recursive = false,
            IncludeItemTypes = new[] { BaseItemKind.Book } // Adjust based on how you categorize comics
        });

        int total = cvIssues.BaseResponse.NumberOfTotalResults;
        int matches = 0;

        foreach (var item in localItems)
        {
            // 3. Match Logic: This is the critical part.
            // You usually match based on "IndexNumber" (Issue Number)
            // Local items might need regex parsing if IndexNumber isn't already set.

            if (item.Name == null)
            {
                continue;
            }

            float? issueNumber = item.IndexNumber ?? GetIssueNumberByName(item.FileNameWithoutExtension);

            // simple match example
            var match = cvIssues.Issues.FirstOrDefault(x => x.IssueNumber == issueNumber?.ToString(CultureInfo.CurrentCulture));

            if (match != null)
            {
                // 4. Update Metadata
                item.SetProviderId("ComicVine", $"4000-{match.Id}");
                item.Name = match.Name;
                item.Overview = match.Description;
                item.ProductionYear = ComicVineMetadataProvider.GetYearFromCoverDate(match.CoverDate);
                item.ForcedSortName = ComicVineMetadataProvider.GetSortName(match);
                if (int.TryParse(match.IssueNumber, out matches))
                {
                    item.IndexNumber = matches;
                }

                // Save changes to DB and refresh
                await _libraryManager.UpdateItemAsync(item, item, ItemUpdateType.MetadataImport, CancellationToken.None)
                    .ConfigureAwait(false);
                matches++;
            }
        }

        return (total, matches);
    }

    [HttpGet("script")]
    public ActionResult GetMainScript() => GetScriptResource("js/plugin.js");

    private ActionResult GetScriptResource(string resourcePath)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Jellyfin.Plugin.ComicVine.{resourcePath.Replace('/', '.')}");
        return stream == null ? NotFound() : new FileStreamResult(stream, "application/javascript");
    }

    private float? GetIssueNumberByName(string comicName)
    {
        var match = _fileNameIssueNumber.Match(comicName);
        if (!match.Success)
        {
            return null;
        }

        if (match.Groups.TryGetValue("issueNumber", out Group? issueGroup) && issueGroup.Success)
        {
            return int.Parse(issueGroup.Value, NumberStyles.Any, CultureInfo.CurrentCulture);
        }

        return null;
    }
}
