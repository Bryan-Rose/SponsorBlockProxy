using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using FFmpegEx = Xabe.FFmpeg.FFmpeg;

namespace SponsorBlockProxy.Audio.Splice
{
    public class SplicerService : IDisposable
    {
        public SplicerService()
        {
            this.WorkDir = Path.Combine(Path.GetTempPath(), "SponsorBlockProxy");
            Directory.CreateDirectory(this.WorkDir);

            if (AppSettingsConfig.OperatingSystem.IsWindows)
            {
                FFmpegEx.SetExecutablesPath(@"FFmpeg\bin\x64");
            }
            else if (AppSettingsConfig.OperatingSystem.IsLinux)
            {

                FFmpegEx.SetExecutablesPath("/usr/lib/x86_64-linux-gnu/");
            }
        }

        private readonly string WorkDir;

        public async Task<string> Cut(string inputFile, CutOut cut, bool deleteInputfile = true)
        {
            var mediaInfo = await FFmpegEx.GetMediaInfo(inputFile);


            string section1Output = GetUniqueFile(this.WorkDir);
            var split1 = await FFmpegEx.Conversions.FromSnippet.Split(inputFile, section1Output, TimeSpan.Zero, cut.Start);
            split1.UseMultiThread(true);
            var split1Task = split1.Start();


            string section2Output = GetUniqueFile(this.WorkDir);
            var split2 = await FFmpegEx.Conversions.FromSnippet.Split(inputFile, section2Output, cut.Stop, mediaInfo.Duration);
            split2.UseMultiThread(true);
            var split2Task = split2.Start();

            await Task.WhenAll(split1Task, split2Task);

            string fullOutput = GetUniqueFile(this.WorkDir);

            var concat = await Extensions.ConcatenateAudio(fullOutput, section1Output, section2Output);
            concat.UseMultiThread(true);
            await concat.Start();


            if (deleteInputfile) File.Delete(inputFile);
            File.Delete(section1Output);
            File.Delete(section2Output);

            return fullOutput;
        }

        public async Task<Stream> Cut(Stream stream, CutOut cut)
        {
            string inputFile = GetUniqueFile(this.WorkDir);
            using (var fs = File.OpenWrite(inputFile))
            {
                await stream.CopyToAsync(fs);
            }

            var outputfile = await Cut(inputFile, cut);

            return File.OpenRead(outputfile);
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



        public class CutOut
        {
            public TimeSpan Start { get; set; }
            public TimeSpan Stop { get; set; }
        }
    }
}
