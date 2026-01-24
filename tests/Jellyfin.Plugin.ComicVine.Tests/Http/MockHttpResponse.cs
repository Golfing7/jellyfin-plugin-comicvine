using System.Net;

namespace Jellyfin.Plugin.ComicVine.Tests.Http
{
    internal record MockHttpResponse(HttpStatusCode StatusCode, string Response);
}
