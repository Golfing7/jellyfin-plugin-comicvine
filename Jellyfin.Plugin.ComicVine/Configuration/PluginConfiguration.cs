using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ComicVine.Configuration
{
    /// <summary>
    /// Instance of the empty plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the Comic Vine API key.
        /// </summary>
        /// <remarks>The rate limit is 200 requests per resource, per hour.</remarks>
        public string ComicVineApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the issue number of a comic should be included on its name.
        /// </summary>
        /// <remarks>This will format the name like: Title (IssueNumber).</remarks>
        public bool IncludeIssueNumberOnName { get; set; } = false;
    }
}
