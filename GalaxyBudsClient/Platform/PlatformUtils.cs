using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Serilog;

namespace GalaxyBudsClient.Platform;

public static class PlatformUtils
{
    public enum Platforms
    {
        Windows,
        Linux,
        OSX,
        Other
    }

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsRunningInFlatpak => Environment.GetEnvironmentVariable("container") != null;

    public static bool SupportsTrayIcon => IsWindows || IsLinux || IsOSX;
    public static bool SupportsAutoboot => IsWindows || (IsLinux && !IsRunningInFlatpak) || IsOSX;
    public static bool SupportsHotkeys => IsWindows || IsLinux || IsOSX;
    public static bool SupportsHotkeysBroadcast => IsWindows || IsLinux || IsOSX;
        
    public static Platforms Platform
    {
        get
        {
            if (IsWindows)
            {
                return Platforms.Windows;
            }
            if (IsLinux)
            {
                return Platforms.Linux;
            }
            if (IsOSX)
            {
                return Platforms.OSX;
            }

            return Platforms.Other;
        }
    }
        
    public static bool IsWindowsContractsSdkSupported
    {
        get
        {
            if (!IsWindows)
            {
                return false;
            }
                
            try
            {
#pragma warning disable CA1416
                var release = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "ReleaseId", "")?.ToString();
                var major = Convert.ToInt32(release?.Substring(0, 2));
                var minor = Convert.ToInt32(release?.Substring(2, 2));
                return major >= 18 && minor >= 03;
#pragma warning restore CA1416
            }
            catch (Exception ex)
            {
                Log.Error("PlatformUtils: Cannot determine build version: {ExMessage}", ex.Message);
                return false;
            }
        }
    }
        
    public static string CombineDataPath(string postfix)
    {
        return Path.Combine(AppDataPath, postfix);
    }

    public static string AppDataPath =>
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/GalaxyBudsClient/";
}