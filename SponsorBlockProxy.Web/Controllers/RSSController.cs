using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SponsorBlockProxy.Audio.FP;
using SponsorBlockProxy.Audio.Splice;
using SponsorBlockProxy.Models;
using SponsorBlockProxy.RSS;

namespace SponsorBlockProxy.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RSSController : ControllerBase
    {
        public RSSController(ILogger<RSSController> logger,
            AppSettingsConfig config,
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
        private readonly AppSettingsConfig config;
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
            using var pRequest = new PerfLogger(this.logger, "WholeRequest");

            string podcastName = this.RouteData.Values["podcast"].ToString().Trim();
            string episode = this.RouteData.Values["episode"].ToString().Trim();

            this.logger.LogInformation($"Downloading {podcastName} - {episode}");
            var podcast = this.config.Podcasts.First(x => x.Name.Equals(podcastName, System.StringComparison.OrdinalIgnoreCase));

            var p1 = new PerfLogger(logger, "Download");
            var (fileName, stream) = await this.proxyService.Download(podcastName, episode);
            string file = Path.GetTempFileName();
            using (var fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                await stream.CopyToAsync(fs);
            }
            p1.Dispose();


            this.logger.LogInformation($"Episode downloaded, querying fingerprint service");

            var p2 = new PerfLogger(this.logger, "FPQuery");
            var queryResult = await this.fpService.Query(file, podcast);
            p2.Dispose();

            this.logger.LogInformation($"Found cuts: {string.Join(",", queryResult.Select(x => $"{x.Start}-{x.End}"))}");
            var p3 = new PerfLogger(this.logger, "SplicerCut");
            string finalFile = await this.splicerService.Cut(file, queryResult.Select(x => new SplicerService.CutOut(x.Start, x.End)).ToArray());
            p3.Dispose();

            return this.PhysicalFile(finalFile, "application/octet-stream", fileName);
        }
    }
}
