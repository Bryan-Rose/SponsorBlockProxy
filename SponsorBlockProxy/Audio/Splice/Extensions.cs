using System;
using System.Linq;
using System.Threading.Tasks;

using Xabe.FFmpeg;

using FFmpegX = Xabe.FFmpeg.FFmpeg;

namespace SponsorBlockProxy.Audio.Splice
{
    public class Extensions
    {
        public static async Task<IConversion> ConcatenateAudio(string output, params string[] inputAudio)
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
