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
using SponsorBlockProxy.Models;

namespace SponsorBlockProxy.RSS
{
    public class RSSProxyService
    {
        private readonly ILogger<RSSProxyService> logger;
        private readonly AppSettingsConfig config;

        public RSSProxyService(ILogger<RSSProxyService> logger, AppSettingsConfig config)
        {
            this.logger = logger;
            this.config = config;
            this.Podcasts = config.Podcasts.ToDictionary(k => k.Name, StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, PodcastInfo> Podcasts { get; set; }

        public RSSFeedWrapper GetFeed(string podcast)
        {
            this.logger.LogInformation($"Getting feed for {podcast}");
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
                        link.Uri = new Uri(new Uri(this.config.BaseUrl), $"RSS/download/{podcast}/{guid}");
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
            var link = episode.Links.FirstOrDefault(x => x.RelationshipType == "enclosure" && x.MediaType == "audio/mp3").Uri;

            var client = HttpClientFactory.Create();
            var stream = await client.GetStreamAsync(link);


            return ($"{episodeId}.mp3", stream);
        }
    }
}
