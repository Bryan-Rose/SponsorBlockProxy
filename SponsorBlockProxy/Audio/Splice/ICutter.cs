using System;
using System.Threading.Tasks;

namespace SponsorBlockProxy.Audio.Splice;

public interface ICutter
{
    Task Cut(string inputFile, string outputFile, TimeSpan start, TimeSpan stop);
}