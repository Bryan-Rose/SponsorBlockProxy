using System;
using Microsoft.Extensions.Logging;

namespace SponsorBlockProxy;

public interface ILoggy
{
    void LogInformation(string msg);
}

public class Loggy<T> : ILoggy
{
    public Loggy(ILogger<T> logger)
    {
        this.Logger = logger;
    }

    public ILogger<T> Logger { get; }

    public string GetContext()
    {
        return $"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff")} ";
    }

    public void LogInformation(string msg)
    {
        msg = GetContext() + msg;
        this.Logger.LogInformation(msg);
    }
}