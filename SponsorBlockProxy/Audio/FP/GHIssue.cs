using System.Threading.Tasks;

using SoundFingerprinting;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
using SoundFingerprinting.InMemory;

namespace SponsorBlockProxy.Audio.FP
{
    public class GHIssue
    {
        public string sampleFile = @"C:\code\SponsorBlockProxy\Samples\sample-15s.mp3";

        private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
        private readonly FFmpegAudioService ffmpegService = new FFmpegAudioService(); // default audio library

        public async Task StoreForLaterRetrieval()
        {
            var track = new TrackInfo("Sample1", "Title1", "Artist1");

            // create fingerprints
            var avHashes = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(sampleFile, MediaType.Audio)
                                        .UsingServices(ffmpegService)
                                        .Hash();

            // store hashes in the database for later retrieval
            modelService.Insert(track, avHashes);
        }
    }
}
