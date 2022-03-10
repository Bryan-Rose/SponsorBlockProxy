using NUnit.Framework;
using System.Threading.Tasks;

using SponsorBlockProxy.Audio.FP;
using SponsorBlockProxy.RSS;
namespace SponsorBlockProxy.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task FingerTest1()
        {
            var r = new FingerPrintTester();
            await r.StoreForLaterRetrieval();
            var result = await r.GetBestMatchForSong();
        }



        [Test]
        public async Task sample()
        {
            var r = new GHIssue();
            await r.StoreForLaterRetrieval();
        }
    }
}