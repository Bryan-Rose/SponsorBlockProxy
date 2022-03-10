using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SponsorBlockProxy.RSS;

namespace SponsorBlockProxy.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RSSController : ControllerBase
    {
        private readonly ILogger<RSSController> logger;
        private readonly RSSProxyService proxyService;

        public RSSController(ILogger<RSSController> logger, RSSProxyService proxyService)
        {
            this.logger = logger;
            this.proxyService = proxyService;
        }

        [HttpGet("{*podcast}")]
        public IActionResult Get()
        {
            string podcast = this.RouteData.Values["podcast"].ToString().Trim();
            var feed = this.proxyService.GetFeed(podcast);
            var stream = new MemoryStream();
            feed.Write(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var contentType = "application/xml";
            return this.File(stream, contentType);
        }

        [HttpGet("download/{podcast}/{*episode}")]
        public async Task<IActionResult> Download()
        {
            string podcast = this.RouteData.Values["podcast"].ToString().Trim();
            string episode = this.RouteData.Values["episode"].ToString().Trim();

            var (fileName, stream) = await this.proxyService.Download(podcast, episode);
            return this.File(stream, "application/octet-stream", fileName);
        }
    }
}
