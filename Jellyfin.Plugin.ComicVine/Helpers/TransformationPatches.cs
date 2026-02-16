using Jellyfin.Plugin.ComicVine.Models;

namespace Jellyfin.Plugin.ComicVine.Helpers;

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class TransformationPatches
{
    public static string IndexHtml(PatchRequestPayload content)
    {
        if (string.IsNullOrEmpty(content.Contents))
        {
            return content.Contents ?? string.Empty;
        }

        var pluginName = "Comic Vine";
        var pluginVersion = Plugin.Instance?.Version.ToString() ?? "unknown";

        var scriptUrl = "../ComicVine/script";
        var scriptTag = $"<script plugin=\"{pluginName}\" version=\"{pluginVersion}\" src=\"{scriptUrl}\" defer></script>";

        var regex = new Regex($"<script[^>]*plugin=[\"']{pluginName}[\"'][^>]*>\\s*</script>\\n?");
        var updatedContent = regex.Replace(content.Contents, string.Empty);

        // 3. Inject the new script tag.
        if (updatedContent.Contains("</body>", StringComparison.Ordinal))
        {
            return updatedContent.Replace("</body>", $"{scriptTag}\n</body>", StringComparison.Ordinal);
        }

        return updatedContent;
    }
}
