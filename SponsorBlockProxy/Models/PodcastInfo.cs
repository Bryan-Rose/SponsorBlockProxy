using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SponsorBlockProxy.Models
{
    public class PodcastInfo
    {
        public string Name { get; set; }
        public Uri RSSUrl { get; set; }
        public SkipPair[] SkipPairs { get; set; } = new SkipPair[0];
    }
}
