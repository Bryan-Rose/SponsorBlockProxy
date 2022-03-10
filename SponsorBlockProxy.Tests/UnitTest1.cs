using NUnit.Framework;
using System.Threading.Tasks;

using SponsorBlockProxy.Audio.FP;
using SponsorBlockProxy.RSS;
using Microsoft.Extensions.Options;

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
        public async Task Query()
        {
            var s = new FPService(null, null, null);
            await s.StartupRegisterAll("C:\\code\\SponsorBlockProxy\\Samples");
            await s.Query(@"C:\Users\bryan\Downloads\d69ad425-b889-4498-833a-ae43a802b0b8.mp3");
        }
    }
}