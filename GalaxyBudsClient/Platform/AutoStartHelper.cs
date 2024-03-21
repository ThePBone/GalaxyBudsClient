using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

namespace GalaxyBudsClient.Platform
{
    public static class AutoStartHelper
    {
        public static readonly IAutoStartHelper Instance;
        
        static AutoStartHelper()
        {
            if (PlatformUtils.IsWindows)
            {
                Instance = new Windows.AutoStartHelper();
            }
            else if (PlatformUtils.IsLinux)
            {
                Instance = new Linux.AutoStartHelper();
            }
            else if (PlatformUtils.IsOSX)
            {
                Instance = new OSX.AutoStartHelper();
            }
            else
            {
                Log.Warning("AutoStartHelper.Dummy: Platform not supported");
                Instance = new Dummy.AutoStartHelper();
            }
        }
    }
}