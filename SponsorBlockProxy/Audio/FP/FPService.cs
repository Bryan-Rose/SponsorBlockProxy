using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Emy;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Data;
using SoundFingerprinting.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SponsorBlockProxy.Audio.FP
{
    public class FPService
    {
        public FPService(ILogger<FPService> logger,
            IOptions<AppSeettingsConfig> config,
            IModelService storageService,
            SoundFingerprintingAudioService fingerPrintService)
        {
            this.logger = logger;
            this.config = config;
            this.storageService = storageService;
            this.audioService = fingerPrintService;
        }

        private readonly ILogger<FPService> logger;
        private readonly IOptions<AppSeettingsConfig> config;
        private readonly IModelService storageService;
        private readonly IAudioService audioService;

        public async Task StartupRegisterAll(string path = null)
        {
            path ??= this.config.Value.SamplesDirectory;
            await Task.WhenAll(Directory.EnumerateFiles(path).Select(x => this.RegisterSample(x)).ToArray());
        }

        public async Task RegisterSample(string file)
        {
            string fileName = Path.GetFileName(file);
            var track = new TrackInfo($"Sample_{fileName}", $"Title_{fileName}", "Sample");

            var avHashes = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(file, MediaType.Audio)
                                        .UsingServices(this.audioService)
                                        .Hash();

            this.storageService.Insert(track, avHashes);
        }
    }
}
