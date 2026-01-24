namespace Jellyfin.Plugin.ComicVine.Providers
{
    /// <summary>
    /// API key provider for Comic Vine.
    /// </summary>
    public interface IComicVineApiKeyProvider
    {
        /// <summary>
        /// Get the Comic Vine API key from the configuration.
        /// </summary>
        /// <returns>The API key or null.</returns>
        public string? GetApiKey();
    }
}
