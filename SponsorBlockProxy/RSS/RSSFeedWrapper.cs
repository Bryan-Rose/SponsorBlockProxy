using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.IO;
using System.Xml;

namespace SponsorBlockProxy.RSS
{
    public class RSSFeedWrapper
    {
        public RSSFeedWrapper(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        private readonly SyndicationFeed feed;


        public void Write(Stream stream)
        {
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                CloseOutput = false,
                Encoding = Encoding.UTF8,
            });
            this.feed.SaveAsRss20(writer);
        }
    }
}
