using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SponsorBlockProxy.Audio.FP;
using SponsorBlockProxy.Audio.Splice;
using SponsorBlockProxy.RSS;

namespace SponsorBlockProxy.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RSSController : ControllerBase
    {
        public RSSController(ILogger<RSSController> logger,
            IOptionsSnapshot<AppSettingsConfig> config,
            RSSProxyService proxyService,
            FPService fpService,
            SplicerService spicerService)
        {
            this.logger = logger;
            this.config = config;
            this.proxyService = proxyService;
            this.fpService = fpService;
            this.splicerService = spicerService;

        }

        private readonly ILogger<RSSController> logger;
        private readonly IOptionsSnapshot<AppSettingsConfig> config;
        private readonly RSSProxyService proxyService;
        private readonly FPService fpService;
        private readonly SplicerService splicerService;

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
            string podcastName = this.RouteData.Values["podcast"].ToString().Trim();
            string episode = this.RouteData.Values["episode"].ToString().Trim();

            var podcast = this.config.Value.Podcasts.First(x => x.Name.Equals(podcastName, System.StringComparison.OrdinalIgnoreCase));

            var (fileName, stream) = await this.proxyService.Download(podcastName, episode);
            string file = Path.GetTempFileName() + ".mp3";
            using (var fs = new FileStream(file, FileMode.CreateNew))
            {
                await stream.CopyToAsync(fs);
            }

            var queryResult = await this.fpService.Query(file, podcast);
            this.logger.LogInformation($"Found cuts: {string.Join(",", queryResult.Select(x => $"{x.Start}-{x.End}"))}");
            string finalFile = await this.splicerService.Cut(file, queryResult.Select(x => new SplicerService.CutOut(x.Start, x.End)).ToArray());


            return this.PhysicalFile(finalFile, "application/octet-stream", fileName);
        }
    }
}
