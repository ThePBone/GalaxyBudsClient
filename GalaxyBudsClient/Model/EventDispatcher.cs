using System;
using System.Runtime.Serialization;
using Avalonia.Threading;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Model;

public enum Event
{
    [LocalizedDescription("event_none")]
    None,
    [LocalizedDescription("event_ambient_toggle")]
    AmbientToggle,
    [LocalizedDescription("event_ambient_volume_up")]
    AmbientVolumeUp,
    [LocalizedDescription("event_ambient_volume_down")]
    AmbientVolumeDown,
    [LocalizedDescription("event_anc_toggle")]
    AncToggle,
    [LocalizedDescription("event_anc_switch_sensitivity")]
    SwitchAncSensitivity,
    [LocalizedDescription("event_anc_switch_one")]
    SwitchAncOne,
    [LocalizedDescription("event_eq_toggle")]
    EqualizerToggle,
    [LocalizedDescription("event_eq_switch")]
    EqualizerNextPreset,
    [LocalizedDescription("event_touch_lock_toggle")]
    LockTouchpadToggle,
    [LocalizedDescription("event_start_stop_find")]
    StartStopFind,
    [LocalizedDescription("event_start_find")]
    StartFind,
    [LocalizedDescription("event_stop_find")]
    StopFind,
    [LocalizedDescription("event_double_edge_touch_toggle")]
    ToggleDoubleEdgeTouch,
    [LocalizedDescription("event_conversation_toggle")]
    ToggleConversationDetect,
    [LocalizedDescription("event_paring_mode")]
    PairingMode,
    [LocalizedDescription("event_manager_visible")]
    ToggleManagerVisibility,
    [LocalizedDescription("event_display_battery_popup")]
    ShowBatteryPopup,
    [LocalizedDescription("event_media_play_pause")]
    TogglePlayPause,
    [LocalizedDescription("event_media_play")]
    Play,
    [LocalizedDescription("event_media_pause")]
    Pause,
    [LocalizedDescription("event_connect")]
    Connect,
            
    /* INTERNAL */
    [IgnoreDataMember]
    UpdateTrayIcon,
    [IgnoreDataMember]
    SetNoiseControlState
}

    
public class EventDispatcher
{
    public static bool CheckTouchOptionEligibility(Event arg)
    {
        switch (arg)
        {
            case Event.AmbientToggle:
            case Event.AncToggle:
            case Event.LockTouchpadToggle:
            case Event.StartStopFind:
            case Event.StartFind:
            case Event.StopFind:
            case Event.Connect:
                return false;
            default:
                return true;
        }
    }
        
    public static bool CheckDeviceSupport(Event arg)
    {
        switch (arg)
        {
            case Event.AmbientToggle:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.AmbientSound) ||
                       BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl);
            case Event.AmbientVolumeUp:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.AmbientSound) ||
                       BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl);
            case Event.AmbientVolumeDown:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.AmbientSound) ||
                       BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl);
            case Event.AncToggle:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.Anc) ||
                       BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl);
            case Event.SwitchAncSensitivity:
                return (BluetoothService.Instance.DeviceSpec.Supports(Features.Anc) ||
                        BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl))
                       && BluetoothService.Instance.DeviceSpec.Supports(Features.AncNoiseReductionLevels);
            case Event.SwitchAncOne:
                return (BluetoothService.Instance.DeviceSpec.Supports(Features.Anc) ||
                        BluetoothService.Instance.DeviceSpec.Supports(Features.NoiseControl))
                       && BluetoothService.Instance.DeviceSpec.Supports(Features.AncWithOneEarbud);
            case Event.ToggleDoubleEdgeTouch:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.DoubleTapVolume);
            case Event.ToggleConversationDetect:
                return BluetoothService.Instance.DeviceSpec.Supports(Features.DetectConversations);
                
            /* INTERNAL */
            case Event.UpdateTrayIcon:
            case Event.SetNoiseControlState:
                return false;
        }

        return true;
    }

    public event Action<Event, object?>? EventReceived;

    public void Dispatch(Event @event, object? extra = null)
    {
        Dispatcher.UIThread.Post(() => EventReceived?.Invoke(@event, extra));
    }
            
    #region Singleton
    private static readonly object Padlock = new();
    private static EventDispatcher? _instance;
    public static EventDispatcher Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new EventDispatcher();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        { 
            _instance ??= new EventDispatcher();
        }
    }
    #endregion
}