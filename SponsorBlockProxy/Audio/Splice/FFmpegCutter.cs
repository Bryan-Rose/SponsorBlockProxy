using System;
using System.Threading.Tasks;
using SponsorBlockProxy.Models;
using FFmpegEx = Xabe.FFmpeg.FFmpeg;

namespace SponsorBlockProxy.Audio.Splice;

public class FFmpegCutter : ICutter
{
    public FFmpegCutter(AppSettingsConfig config, Loggy<FFmpegCutter> logger)
    {
        this.Config = config;
        this.Logger = logger;
        FFmpegEx.SetExecutablesPath(config.MediaToolPath);
    }

    public AppSettingsConfig Config { get; }
    public Loggy<FFmpegCutter> Logger { get; }

    public async Task<string> Cut(string inputFile, string workDir, TimeSpan start, TimeSpan stop)
    {
        var duration = (stop - start);
        var outputFile = Extensions.GetUniqueFile(workDir);
        var splitJob = await FFmpegEx.Conversions.FromSnippet.Split(inputFile, outputFile, start, duration);
        splitJob.UseMultiThread(true);
        var splitTask = splitJob.Start();

        await splitTask;
        
        return outputFile;
    }
}