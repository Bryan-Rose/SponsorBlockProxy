﻿using System.Runtime.InteropServices;
using SponsorBlockProxy.Models;
using SponsorBlockProxy.RSS;

namespace SponsorBlockProxy.Models
{
    public class AppSettingsConfig
    {
        public string BaseUrl { get; set; }
        public string SamplesDirectory { get; set; }
        public PodcastInfo[] Podcasts { get; set; } = new PodcastInfo[0];



        public static class OperatingSystem
        {
            public static bool IsWindows =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            public static bool IsLinux =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}
