using NUnit.Framework;
using System.Threading.Tasks;

namespace SponsorBlockProxy.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RSSTest1()
        {
            var r = new RSSTester();
            r.Run();
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