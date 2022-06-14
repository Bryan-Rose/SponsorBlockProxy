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
        public List<SkipPair> SkipPairs { get; set; } = new List<SkipPair>();

        public class SkipPair
        {
            public SkipPair()
            {
                IdSeed++; ;
                this.Id = IdSeed.ToString();
            }

            static int IdSeed = 1;

            public string Id { get; set; }
            public string Start_Filename { get; set; }
            public string End_Filename { get; set; }
            public int MaxTimeSeconds { get; set; } = 5 * 60;
            public int MinTimeSeconds { get; set; } = 30;
        }
    }
}
