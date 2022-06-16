using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SponsorBlockProxy;

public class PerfLogger : IDisposable
{
    public PerfLogger(ILogger logger, string name)
    {
        this.Name = name;
        this.Logger = logger;
        this.SW = new Stopwatch();
        this.SW.Start();
    }

    public string Name { get; }
    public ILogger Logger { get; }
    public Stopwatch SW { get; }

    public void Dispose()
    {
        this.SW.Stop();
        this.Logger.LogInformation($"{this.Name} - {this.SW.ElapsedMilliseconds}ms");
    }
}