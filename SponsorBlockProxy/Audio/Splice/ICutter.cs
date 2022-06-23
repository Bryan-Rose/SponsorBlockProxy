using System;
using System.Threading.Tasks;

namespace SponsorBlockProxy.Audio.Splice;

public interface ICutter
{
    Task<string> Cut(string inputFile, string workDir, TimeSpan start, TimeSpan stop);
}