﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using MainWindow = GalaxyBudsClient.Interface.MainWindow;

namespace GalaxyBudsClient.Utils.Interface;

internal class TrayManager
{
    private bool _allowUpdate = true;
    private bool _missedUpdate = false;

    private TrayManager()
    {
        // Make sure nobody is updating the menu while it's open
        // because it crashes mac https://github.com/AvaloniaUI/Avalonia/issues/14578
        (Application.Current as App)!.TrayMenu.Opening += (_, _) => _allowUpdate = false;
        
        (Application.Current as App)!.TrayMenu.Closed += (_, _) =>
        {
            _allowUpdate = true;
            if (_missedUpdate)
            {
                _ = RebuildAsync();
            }
        };
        // It's important to trigger a rebuild every time some event happens
        // otherwise tray will show outdated infos
        (Application.Current as App)!.TrayMenu.NeedsUpdate += (sender, args) => _ = RebuildAsync();
        
        BluetoothService.Instance.Connected += (sender, args) => _ = RebuildAsync();
        BluetoothService.Instance.Disconnected += (sender, args) => _ = RebuildAsync();
        EventDispatcher.Instance.EventReceived += (ev, args) =>
        {
            if (ev == Event.UpdateTrayIcon) 
                _ = RebuildAsync();
        };
        // triggering rebuild when battery % changes
        SppMessageHandler.Instance.StatusUpdate += (_, _) => _ = RebuildAsync();
        SppMessageHandler.Instance.ExtendedStatusUpdate += (_, _) => _ = RebuildAsync();
        // triggering rebuild when noise control / ambient / anc state changes is handled in MessageComposer.cs
        // triggering rebuild when lock touchpad changes is handled in TouchpadPage.xaml.cs
        // triggering rebuild when eq state changes is handled in MessageComposer.cs
    }

    private async void OnTrayMenuCommand(object? type)
    {
        if (type is not TrayItemTypes e)
        {
            Log.Error("TrayManager.OnTrayMenuCommand: Unknown item type: {Type}", type);
            return;
        }
            
        switch (e)
        {
            case TrayItemTypes.ToggleNoiseControl:
                var ncVm = MainWindow.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>();
                if (ncVm != null)
                {
                    if (ncVm.IsAmbientSoundEnabled)
                    {
                        /* Ambient is on, use ANC toggle */
                        EventDispatcher.Instance.Dispatch(Event.AncToggle);
                    }
                    else if (ncVm.IsAncEnabled)
                    {
                        /* ANC is on, use ANC toggle to disable itself */
                        EventDispatcher.Instance.Dispatch(Event.AncToggle);
                    }
                    else
                    {
                        /* Nothing is on, use ambient toggle */
                        EventDispatcher.Instance.Dispatch(Event.AmbientToggle);
                    }
                }
                break;
            case TrayItemTypes.LockTouchpad:
                EventDispatcher.Instance.Dispatch(Event.LockTouchpadToggle);
                break;
            case TrayItemTypes.ToggleAnc:
                EventDispatcher.Instance.Dispatch(Event.AncToggle);
                break;
            case TrayItemTypes.ToggleEqualizer:
                EventDispatcher.Instance.Dispatch(Event.EqualizerToggle);
                break;
            case TrayItemTypes.ToggleAmbient:
                EventDispatcher.Instance.Dispatch(Event.AmbientToggle);
                break;
            case TrayItemTypes.Connect:
                if (!BluetoothService.Instance.IsConnectedLegacy && BluetoothService.RegisteredDeviceValid)
                {
                    await BluetoothService.Instance.ConnectAsync();
                }
                break;
            case TrayItemTypes.Open:
                Dispatcher.UIThread.Post(MainWindow.Instance.BringToFront);
                break;
            case TrayItemTypes.Quit:
                Log.Information("TrayManager: Exit requested by user");
                MainWindow.Instance.OverrideMinimizeTray = true;
                Dispatcher.UIThread.Post(MainWindow.Instance.Close);
                break;
        }
            
        await RebuildAsync();
    }

    private static List<NativeMenuItemBase?> RebuildBatteryInfo()
    {
        var bsu = DeviceMessageCache.Instance.BasicStatusUpdate!;
        if (bsu.BatteryCase > 100)
        {
            bsu.BatteryCase = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bsu.BatteryCase;
        }
            
        return
        [
            bsu.BatteryL > 0
                ? new NativeMenuItem($"{Loc.Resolve("left")}: {bsu.BatteryL}%") { IsEnabled = false }
                : null,
            bsu.BatteryR > 0
                ? new NativeMenuItem($"{Loc.Resolve("right")}: {bsu.BatteryR}%") { IsEnabled = false }
                : null,
            bsu.BatteryCase is > 0 and <= 100 && BluetoothService.Instance.DeviceSpec.Supports(Features.CaseBattery)
                ? new NativeMenuItem($"{Loc.Resolve("case")}: {bsu.BatteryCase}%") { IsEnabled = false }
                : null,

            new NativeMenuItemSeparator()

        ];
    }

    private IEnumerable<NativeMenuItemBase> RebuildDynamicActions()
    {
        return from type in BluetoothService.Instance.DeviceSpec.TrayShortcuts
            let resourceKey = type switch
            {
                TrayItemTypes.ToggleNoiseControl => "tray_switch_noise",
                TrayItemTypes.ToggleEqualizer => MainWindow.Instance.MainView.ResolveViewModelByType<EqualizerPageViewModel>()?.IsEqEnabled ?? false
                    ? "tray_disable_eq"
                    : "tray_enable_eq",
                TrayItemTypes.ToggleAmbient => MainWindow.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAmbientSoundEnabled ?? false
                    ? "tray_disable_ambient_sound"
                    : "tray_enable_ambient_sound",
                TrayItemTypes.ToggleAnc => MainWindow.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>()?.IsAncEnabled ?? false
                    ? "tray_disable_anc"
                    : "tray_enable_anc",
                TrayItemTypes.LockTouchpad => MainWindow.Instance.MainView.ResolveViewModelByType<TouchpadPageViewModel>()?.IsTouchpadLocked ?? false
                    ? "tray_unlock_touchpad"
                    : "tray_lock_touchpad",
                _ => "unknown"
            }
            select new NativeMenuItem(Loc.Resolve(resourceKey))
            {
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = type
            };
    }

    public async Task RebuildAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!_allowUpdate)
            {
                _missedUpdate = true;
                return;
            }
            _missedUpdate = false;
            var items = new List<NativeMenuItemBase>();
            if (PlatformUtils.IsOSX || PlatformUtils.IsLinux)
            {
                items.Add(new NativeMenuItem(Loc.Resolve("window_open"))
                {
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = TrayItemTypes.Open
                });
                items.Add(new NativeMenuItemSeparator());
            }
            if (BluetoothService.Instance.IsConnectedLegacy && DeviceMessageCache.Instance.BasicStatusUpdate != null)
            {
                items.AddRange(RebuildBatteryInfo().OfType<NativeMenuItemBase>());
                items.AddRange(RebuildDynamicActions());
                items.Add(new NativeMenuItemSeparator());
            }
            else if (BluetoothService.RegisteredDeviceValid)
            {
                items.Add(new NativeMenuItem(Loc.Resolve("connlost_connect"))
                {
                    Command = new MiniCommand(OnTrayMenuCommand),
                    CommandParameter = TrayItemTypes.Connect
                });
                items.Add(new NativeMenuItemSeparator());
            }
                
            items.Add(new NativeMenuItem(Loc.Resolve("tray_quit"))
            {
                Command = new MiniCommand(OnTrayMenuCommand),
                CommandParameter = TrayItemTypes.Quit
            });
                
            (Application.Current as App)?.TrayMenu.Items.Clear();
            foreach (var item in items)
            {
                (Application.Current as App)?.TrayMenu.Items.Add(item);
            }
        }, DispatcherPriority.Normal);
    }

    #region Singleton
    private static readonly object Padlock = new();
    private static TrayManager? _instance;
    public static TrayManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new TrayManager();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        {
            _instance ??= new TrayManager();
        }
    }
    #endregion
}