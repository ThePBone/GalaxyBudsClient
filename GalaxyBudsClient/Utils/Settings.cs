using System.ComponentModel;
using Config.Net;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Utils;

public static class Settings
{
    public static ISettings Instance { get; }
    private static string SettingsPath => PlatformUtils.CombineDataPath("config.json");

    static Settings()
    {
        Log.Information("Using settings file at: {SettingsPath}", SettingsPath);
        Instance = new ConfigurationBuilder<ISettings>()
            .UseJsonFile(SettingsPath)
            .UseTypeParser(new ConfigArrayParser<long>())
            .UseTypeParser(new HotkeyArrayParser())
            .UseTypeParser(new AccentColorParser())
            .Build();
            
        Instance.PropertyChanged += OnMainSettingsChanged;
    }

    private static void OnMainSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISettings.Locale):
                Loc.Load();
                break;
            case nameof(ISettings.DynamicTrayIconMode):
            {
                var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
                if (Instance.DynamicTrayIconMode != DynamicTrayIconModes.Disabled && BluetoothService.Instance.IsConnected && cache != null)
                    WindowIconRenderer.UpdateDynamicIcon(cache);
                else
                    WindowIconRenderer.ResetIconToDefault();
                break;
            }
        }
    }
}