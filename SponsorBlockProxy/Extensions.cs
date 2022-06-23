using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SoundFingerprinting.Query;
using SponsorBlockProxy.Models;
using Xabe.FFmpeg;

using FFmpegX = Xabe.FFmpeg.FFmpeg;

namespace SponsorBlockProxy
{
    public static class Extensions
    {
        public static string GetPodcast(this SoundFingerprinting.Query.ResultEntry r)
        {
            return r.Track.MetaFields["podcast"];
        }

        public static string GetSkipPair(this SoundFingerprinting.Query.ResultEntry r)
        {
            return r.Track.MetaFields["skipPairId"];
        }

        public static StartEndEnum GetStartEnd(this ResultEntry r)
        {
            return Enum.Parse<StartEndEnum>(r.Track.MetaFields["startEnd"]);
        }


        public static void ForEach<T>(this IEnumerable<T> e, Action<T> act)
        {
            foreach (var t in e)
            {
                act(t);
            }
        }
        
        public static string GetUniqueFile(string path, string extension = ".mp3")
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

            throw new Exception("Could not find a unique file");
        }

        public static IConversion ConcatenateAudio(string output, params string[] inputAudio)
        {
            if (inputAudio.Length < 2)
            {
                throw new ArgumentException("You must provide at least 2 files for the concatenation to work", nameof(inputAudio));
            }

            IConversion conversion = FFmpegX.Conversions.New();

            conversion.UseMultiThread(true);
            conversion.AddParameter($"-i concat:{string.Join("|", inputAudio.Select(x => x.Escape()))}");
            conversion.AddParameter("-c copy");
            conversion.SetOutput(output);

            return conversion;
        }

    }
}
