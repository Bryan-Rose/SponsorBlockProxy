using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Query;
using SoundFingerprinting.Strides;
using SponsorBlockProxy.Models;

namespace SponsorBlockProxy.Audio.FP
{
    public class FPService
    {
        public FPService(ILogger<FPService> logger,
            ILoggerFactory logFactory,
            AppSettingsConfig config)
        {
            this.logger = logger;
            this.config = config;
            this.storageService = new InMemoryModelService();
            this.audioService = new FFmpegAudioService();
        }

        private readonly ILogger<FPService> logger;
        private readonly AppSettingsConfig config;
        private readonly IModelService storageService;
        private readonly IAudioService audioService;

        public async Task StartupRegisterAll(string path = null)
        {
            path ??= this.config.SamplesDirectory;
            this.logger.LogInformation($"Registering samples at {path}");

            foreach (var p in this.config.Podcasts)
            {
                this.logger.LogInformation($"Registering {p.SkipPairs.Length} samples for {p.Name}");

                foreach (var s in p.SkipPairs)
                {
                    await this.RegisterSample(p.Name, s.Id, StartEndEnum.Start, Path.Combine(path, s.Start_Filename));
                    await this.RegisterSample(p.Name, s.Id, StartEndEnum.End, Path.Combine(path, s.End_Filename));
                }
            }
        }

        public async Task RegisterSample(string podcast, string skipPairId, StartEndEnum startEnd, string file)
        {
            string fileName = Path.GetFileName(file);
            this.logger.LogInformation($"Registering sample for {podcast} - {file}");

            var track = new TrackInfo($"{podcast}_{fileName}", fileName, "Sample",
                metaFields: new Dictionary<string, string> {
                    { "podcast", podcast },
                    { "skipPairId", skipPairId },
                    { "startEnd", startEnd.ToString() }
                 });

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

        public async Task<List<ResultModel>> Query(string file, PodcastInfo podcast)
        {
            var result = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(file)
                 .WithQueryConfig(config =>
                {
                    config.Audio.AllowMultipleMatchesOfTheSameTrackInQuery = true;
                    return config;
                })
                .UsingServices(this.storageService, this.audioService)
                .Query();

            var audioResult = result.Audio;

            if (!audioResult.ContainsMatches)
            {
                throw new Exception("No matches found!");
            }

            this.logger.LogInformation($"Raw query - found {audioResult.ResultEntries.Count()}");

            var working = audioResult.ResultEntries.Where(x => x.GetPodcast() == podcast.Name)
                .OrderBy(x => x.QueryMatchStartsAt)
                .ToList();

            this.logger.LogInformation($"Filtered to podcast - found {audioResult.ResultEntries.Count()}");
            foreach (var r in working)
            {
                this.logger.LogInformation($"{r.Track.Title} SkipPairId:{r.GetSkipPair()} StartEnd:{r.GetStartEnd()} StartTime:{(int)r.QueryMatchStartsAt}");
            }

            var pairs = new List<ResultModel>();
            (ResultEntry, int) GetEnd(List<ResultEntry> q, int startIndex, ResultEntry start, SkipPair skip)
            {
                for (int i = startIndex; i < q.Count; i++)
                {
                    var r = q[i];

                    var endSkip = r.GetSkipPair();
                    if (endSkip != skip.Id) continue;

                    if (r.GetStartEnd() != StartEndEnum.End) continue;

                    if (r.QueryMatchStartsAt - start.QueryMatchStartsAt < skip.MinTimeSeconds) continue;
                    if (r.QueryMatchStartsAt - start.QueryMatchStartsAt > skip.MaxTimeSeconds) break;

                    return (r, i);
                }

                return (null, -1);
            }

            ResultEntry start;
            for (int currentIndex = 0; currentIndex < working.Count; currentIndex++)
            {
                start = working[currentIndex];
                if (start.GetStartEnd() != StartEndEnum.Start) continue;

                var startPair = podcast.SkipPairs.First(x => x.Id == start.GetSkipPair());
                var (end, foundEndIndex) = GetEnd(working, currentIndex + 1, start, startPair);
                if (end == null) continue;

                pairs.Add(new ResultModel(start, end));
                currentIndex = foundEndIndex;
            }

            return pairs;
        }


        public class ResultModel
        {
            public ResultModel(TimeSpan start, TimeSpan end)
            {
                this.Start = start;
                this.End = end;
            }

            public ResultModel(ResultEntry start, ResultEntry end)
            {
                this.Start = TimeSpan.FromSeconds(start.QueryMatchStartsAt);
                this.End = TimeSpan.FromSeconds(end.QueryMatchStartsAt);
            }

            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
        }
    }
}
