using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SponsorBlockProxy.Models;

namespace SponsorBlockProxy.Audio.Splice;

public class Mp3SpltCutter : ICutter
{
    public Mp3SpltCutter(AppSettingsConfig config, Loggy<Mp3SpltCutter> logger)
    {
        this.Config = config;
        this.Logger = logger;

    }

    public AppSettingsConfig Config { get; }
    public Loggy<Mp3SpltCutter> Logger { get; }

    public const string BinName = "mp3splt";

    public string GetExe()
    {
        if (!string.IsNullOrWhiteSpace(this.Config.MediaToolPath))
        {
            return Path.Combine(this.Config.MediaToolPath, BinName);
        }
        else
        {
            return BinName;
        }
    }


    public async Task<string> Cut(string inputFile, string workDir, TimeSpan start, TimeSpan stop)
    {
        var outputFile = Extensions.GetUniqueFile(workDir, "");
        var outputFileName = Path.GetFileName(outputFile);
        var argsList = new List<string>() {
            "-f", // Process all frames
            //"-Q", // Quiet
            $"-d \"{workDir}\"",
            $"-o \"{outputFileName}\"",
            $"\"{inputFile}\"",
            $"{(int)start.TotalMinutes}.{start.Seconds}",
            $"{(int)stop.TotalMinutes}.{stop.Seconds}",
        };
        var argsStr = String.Join(" ", argsList);

        var startInfo = new ProcessStartInfo
        {
            Arguments = argsStr,
            CreateNoWindow = true,
            FileName = this.GetExe(),
        };

        this.Logger.LogInformation($"Proc: {startInfo.FileName}");
        this.Logger.LogInformation($"Args: {startInfo.Arguments}");

        var process = Process.Start(startInfo);
        await process.WaitForExitAsync();

        return Path.ChangeExtension(outputFile, "mp3");
    }
}