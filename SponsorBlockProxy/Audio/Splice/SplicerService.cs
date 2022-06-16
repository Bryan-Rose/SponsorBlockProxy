using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SponsorBlockProxy.Models;
using Microsoft.Extensions.Options;

using FFmpegEx = Xabe.FFmpeg.FFmpeg;

namespace SponsorBlockProxy.Audio.Splice
{
    public class SplicerService : IDisposable
    {
        public SplicerService(ILogger<SplicerService> logger,
            AppSettingsConfig config)
        {
            this.logger = logger;
            this.WorkDir = Path.Combine(Path.GetTempPath(), "SponsorBlockProxy");
            Directory.CreateDirectory(this.WorkDir);


            FFmpegEx.SetExecutablesPath(config.ffmpegDirectrory);

            // if (AppSettingsConfig.OperatingSystem.IsWindows)
            // {
            //     FFmpegEx.SetExecutablesPath(@"FFmpeg\bin\x64");
            // }
            // else if (AppSettingsConfig.OperatingSystem.IsLinux)
            // {

            //     FFmpegEx.SetExecutablesPath("/usr/lib/x86_64-linux-gnu/");
            // }
        }

        public ILogger<SplicerService> Logger { get; }
        private readonly string WorkDir;

        public async Task<string> Cut(string inputFile, CutOut[] cuts, bool deleteInputfile = true)
        {
            var mediaInfo = await FFmpegEx.GetMediaInfo(inputFile);

            var keeps = new List<Keep>();
            TimeSpan start = TimeSpan.Zero;
            foreach (var cut in cuts)
            {
                keeps.Add(new Keep { Start = start, Stop = cut.Start });
                start = cut.Stop;
            }
            keeps.Add(new Keep { Start = start, Stop = mediaInfo.Duration });


            foreach (var keep in keeps)
            {
                string sectionOutput = GetUniqueFile(this.WorkDir);
                var splitJob = await FFmpegEx.Conversions.FromSnippet.Split(inputFile, sectionOutput, keep.Start, keep.Stop);
                splitJob.UseMultiThread(true);
                this.logger.LogInformation($"Starting split {}");
                var splitTask = splitJob.Start();
                keep.File = sectionOutput;
                keep.CutTask = splitTask;
            }

            await Task.WhenAll(keeps.Select(x => x.CutTask));

            string fullOutput = GetUniqueFile(this.WorkDir);

            var concat = Extensions.ConcatenateAudio(fullOutput, keeps.Select(x => x.File).ToArray());
            concat.UseMultiThread(true);
            await concat.Start();


            if (deleteInputfile) File.Delete(inputFile);
            foreach (var keep in keeps)
            {
                File.Delete(keep.File);
            }

            return fullOutput;
        }

        class Keep
        {
            public TimeSpan Start { get; set; }
            public TimeSpan Stop { get; set; }
            public Task CutTask { get; set; }
            public string File { get; set; }
        }

        public void Dispose()
        {
            Directory.Delete(this.WorkDir, recursive: true);
        }

        private static string GetUniqueFile(string path, string extension = ".mp3")
        {
            for (int i = 0; i < 10; i++)
            {
                string fileName = Path.GetRandomFileName() + extension;
                var full = Path.Combine(path, fileName);
                if (!File.Exists(full))
                {
                    return full;
                }
            }

            throw new Exception();
        }


        public record CutOut(TimeSpan Start, TimeSpan Stop);
    }
}
