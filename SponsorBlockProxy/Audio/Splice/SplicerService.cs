﻿using System;
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
        public SplicerService(Loggy<SplicerService> logger,
            AppSettingsConfig config,
            ICutter cutter)
        {
            this.Logger = logger;
            this.Config = config;
            this.Cutter = cutter;
            this.WorkDir = Path.Combine(Path.GetTempPath(), "SponsorBlockProxy");
            Directory.CreateDirectory(this.WorkDir);
        }

        public Loggy<SplicerService> Logger { get; }
        public AppSettingsConfig Config { get; }
        public ICutter Cutter { get; }
        public string WorkDir { get; }

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


            var p1 = new PerfLogger(this.Logger, "Splits");
            foreach (var keep in keeps)
            {
                string sectionOutput = GetUniqueFile(this.WorkDir);
                keep.File = sectionOutput;
                this.Logger.LogInformation($"Starting split {keep.Start}-{keep.Stop}");
                var splitTask = this.Cutter.Cut(inputFile, sectionOutput, keep.Start, keep.Stop)
                    .ContinueWith(t =>
                    {
                        this.Logger.LogInformation($"Split completed {keep.Start}-{keep.Stop}");
                    });
                keep.CutTask = splitTask;
            }

            await Task.WhenAll(keeps.Select(x => x.CutTask));
            p1.Dispose();


            string fullOutput = GetUniqueFile(this.WorkDir);

            var concat = Extensions.ConcatenateAudio(fullOutput, keeps.Select(x => x.File).ToArray());
            concat.UseMultiThread(true);
            var p2 = new PerfLogger(this.Logger, "Concate");
            await concat.Start();
            p2.Dispose();


            if (deleteInputfile) File.Delete(inputFile);
            foreach (var keep in keeps)
            {
                File.Delete(keep.File);
            }

            return fullOutput;
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
