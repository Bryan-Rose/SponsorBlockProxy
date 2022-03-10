using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Xml;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.IO;

namespace SponsorBlockProxy.RSS
{
    public class RSSProxyService
    {
        private readonly ILogger<RSSProxyService> logger;
        private readonly IOptions<AppSeettingsConfig> config;

        public RSSProxyService(ILogger<RSSProxyService> logger, IOptions<AppSeettingsConfig> config)
        {
            this.logger = logger;
            this.config = config;
        }

        public Dictionary<string, PodcastInfo> Podcasts = new Dictionary<string, PodcastInfo>(StringComparer.OrdinalIgnoreCase) {
            { "LinuxUnplugged", new PodcastInfo { Name = "Linux Unplugged", RSSUrl = new Uri(@"https://feeds.fireside.fm/linuxunplugged/rss") } },
            { "JupiterExtras", new PodcastInfo { Name = "Jupiter EXTRAS", RSSUrl = new Uri(@"https://feeds.fireside.fm/extras/rss") } },
            { "LinuxActionNews", new PodcastInfo { Name = "Linux Action News", RSSUrl = new Uri(@"https://feeds.fireside.fm/linuxactionnews/rss") } },
            { "SelfHosted", new PodcastInfo { Name = "SelfHosted", RSSUrl = new Uri(@"https://feeds.fireside.fm/selfhosted/rss") } },
        };

        public RSSFeedWrapper GetFeed(string podcast)
        {
            var info = this.Podcasts.GetValueOrDefault(podcast);
            if (info is null)
            {
                throw new Exception($"Podcast \"{podcast}\" not found");
            }

            using var reader = XmlReader.Create(info.RSSUrl.ToString());
            var feed = SyndicationFeed.Load(reader);

            foreach (var item in feed.Items)
            {
                var guid = item.Id;
                foreach (var link in item.Links)
                {
                    if (link.RelationshipType == "enclosure" && link.MediaType == "audio/mp3")
                    {
                        link.Uri = new Uri(new Uri(this.config.Value.BaseUrl), $"download/{podcast}/{guid}");
                    }
                }
            }

            var wrapper = new RSSFeedWrapper(feed);
            return wrapper;

        }

        public async Task<(string filename, Stream stream)> Download(string podcast, string episodeId)
        {
            var info = this.Podcasts.GetValueOrDefault(podcast);
            if (info is null)
            {
                throw new Exception($"Podcast \"{podcast}\" not found");
            }

            using var reader = XmlReader.Create(info.RSSUrl.ToString());
            var feed = SyndicationFeed.Load(reader);

            var episode = feed.Items.FirstOrDefault(x => x.Id == episodeId);
            var link = episode.Links.FirstOrDefault(x => x.RelationshipType == "enclosure").Uri;

            var client = HttpClientFactory.Create();
            var stream = await client.GetStreamAsync(link);


            return ($"{episodeId}.mp3", stream);
        }
    }
}
