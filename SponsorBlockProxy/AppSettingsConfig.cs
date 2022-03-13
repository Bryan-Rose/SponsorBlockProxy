using System.Runtime.InteropServices;

namespace SponsorBlockProxy
{
    public class AppSettingsConfig
    {
        public string BaseUrl { get; set; }
        public string SamplesDirectory { get; set; }




        public static class OperatingSystem
        {
            public static bool IsWindows =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            public static bool IsLinux =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}
