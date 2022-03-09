using System;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SponsorBlockProxy
{
    public class RSSTester
    {
        public RSSTester()
        {

        }

        private string url = "https://feeds.fireside.fm/linuxunplugged/rss";

        public void Run()
        {
            SyndicationFeed feed = null;

            try
            {
                using (var reader = XmlReader.Create(url))
                {
                    feed = SyndicationFeed.Load(reader);
                }
            }
            catch { } // TODO: Deal with unavailable resource.

            if (feed != null)
            {
                foreach (var element in feed.Items)
                {
                    Console.WriteLine($"Title: {element.Title.Text}");
                    Console.WriteLine($"Summary: {element.Summary.Text}");
                }
            }
        }
    }
}
