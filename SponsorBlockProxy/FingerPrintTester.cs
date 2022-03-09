using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
using SoundFingerprinting.InMemory;

namespace SponsorBlockProxy
{
    public class FingerPrintTester
    {
        public string sampleFile = @"C:\code\SponsorBlockProxy\Samples\LUP_Sponsor1_32bit.wav";
        public string podcast = @"C:\Users\bryan\Downloads\d1332bec-e021-4ce8-920a-6397693df187.mp3";


        private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
        private readonly SoundFingerprintingAudioService fingerPrintService = new SoundFingerprintingAudioService(); // default audio library
        private readonly FFmpegAudioService ffmpegService = new FFmpegAudioService(); // default audio library

        public async Task StoreForLaterRetrieval()
        {
            var track = new TrackInfo("LUP_Sponsor_1", "LUP_Sponsor_1", "LinuxUnpluugged");

            // create fingerprints
            var avHashes = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(sampleFile, MediaType.Audio)
                                        .UsingServices(fingerPrintService)
                                        .Hash();

            // store hashes in the database for later retrieval
            modelService.Insert(track, avHashes);
        }


        public async Task<TrackData> GetBestMatchForSong()
        {
            // query the underlying database for similar audio sub-fingerprints
            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                                 .From(podcast)
                                                 .UsingServices(modelService, ffmpegService)
                                                 .Query();

            return queryResult.BestMatch.Audio.Track;
        }
    }
}
