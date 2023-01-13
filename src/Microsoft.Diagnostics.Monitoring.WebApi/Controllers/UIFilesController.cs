// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;


namespace Microsoft.Diagnostics.Monitoring.WebApi.Controllers
{
    [Route("/ui/")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UIFilesController : ControllerBase
    {
        private readonly ILogger<UIFilesController> _logger;

        public UIFilesController(ILogger<UIFilesController> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get the swagger files embedded into the assembly
        /// </summary>
        [HttpGet("{filename}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public FileStreamResult GetFile(string filename)
        {
            _logger.LogInformation("Fetching {Filename} from resources", filename);

            if (string.IsNullOrEmpty(filename)) filename = "index.html";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Diagnostics.Monitoring.WebApi.dotnet_monitor_ui." + filename);

            return new FileStreamResult(stream, MimeMap(filename))
            {
               // FileDownloadName = filename
            };
        }

        private static string MimeMap(string filename)
        {
            switch (filename.Substring(filename.LastIndexOf(".")))
            {
                case ".js": return "text/json";
                case ".yaml": return "text/yaml";
                case ".html": return "text/html";
                case ".map": return "application/json";
                case ".ico": return "image/vnd.microsoft.icon";
                default: return "text/plain";
            }
        }
    }
}
