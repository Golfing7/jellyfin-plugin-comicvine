using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ComicVine.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.ComicVine;

public class FileTransformationStartupHook : IScheduledTask
{
    private readonly ILogger<FileTransformationStartupHook> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTransformationStartupHook"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public FileTransformationStartupHook(ILogger<FileTransformationStartupHook> logger)
    {
        this._logger = logger;
    }

    public string Name => "Comic Vine Startup";

    public string Key => "ComicVineStartup";

    public string Description => "Injects the ComicVine script using the File Transformation plugin and performs necessary cleanups.";

    public string Category => "Comic Vine";

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        await Task.Run(
            () =>
            {
                _logger.LogInformation("ComicVine Startup Task run successfully.");
                RegisterFileTransformation();
            },
            cancellationToken)
            .ConfigureAwait(false);
    }

    private void RegisterFileTransformation()
    {
        Assembly? fileTransformationAssembly =
            AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x =>
                x.FullName?.Contains(".FileTransformation", StringComparison.Ordinal) ?? false);

        if (fileTransformationAssembly != null)
        {
            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");

            if (pluginInterfaceType != null)
            {
                var payload = new JObject
                {
                    { "id", "309ec7e5-4981-4e8c-992f-8e4dde9591e0" }, // Using the plugin's GUID as a unique ID
                    { "fileNamePattern", "index.html" },
                    { "callbackAssembly", GetType().Assembly.FullName },
                    { "callbackClass", typeof(TransformationPatches).FullName },
                    { "callbackMethod", nameof(TransformationPatches.IndexHtml) }
                };

                pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { payload });
                _logger.LogInformation("Successfully registered Comic Vine Script Injection with File Transformation Plugin.");
            }
        }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo() { Type = TaskTriggerInfoType.StartupTrigger };
    }
}
