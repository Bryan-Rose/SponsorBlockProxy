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
using SoundFingerprinting.Strides;

namespace SponsorBlockProxy.Audio.FP
{
    public class FPService
    {
        public FPService(ILogger<FPService> logger,
            ILoggerFactory logFactory,
            IOptions<AppSettingsConfig> config)
        {
            this.logger = logger;
            this.config = config;
            this.storageService = new InMemoryModelService();
            this.audioService = new FFmpegAudioService();
        }

        private readonly ILogger<FPService> logger;
        private readonly IOptions<AppSettingsConfig> config;
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
                                        .WithFingerprintConfig(x =>
                                        {
                                            x.Audio.Stride = new IncrementalStaticStride(128);
                                            return x;
                                        }).UsingServices(this.audioService)
                                        .Hash();

            this.storageService.Insert(track, avHashes);
        }

        public async Task Query(string file)
        {
            var result = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(file)
                //.WithQueryConfig(x =>
                //{
                //    x.Audio.Stride = new IncrementalStaticStride(128);
                //    return x;
                //})
                .UsingServices(this.storageService, this.audioService)
                .Query();

        }
    }
}
