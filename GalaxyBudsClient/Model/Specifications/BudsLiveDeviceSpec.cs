using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsLiveDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, new FeatureRule(3, "R180XXU0ATF2")  },
        { Features.AmbientPassthrough, null },
        { Features.Anc, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.StereoPan, new FeatureRule(7, "R180XXU0AUB5") },
        { Features.SpatialSensor, new FeatureRule(9) },
        { Features.BixbyWakeup, new FeatureRule(1) },
        { Features.FirmwareUpdates, null },
        { Features.BuildInfo, null },
        { Features.Voltage, null },
        { Features.DebugSku, null },
        { Features.CallPathControl, new FeatureRule(8) },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null }
    };
        
    public Models Device => Models.BudsLive;
    public string DeviceBaseName => "Buds Live";
    public ITouchMap TouchMap => new BudsLiveTouchMap();
    public Guid ServiceUuid => Uuids.BudsLive;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleAnc,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Bean";
    public int MaximumAmbientVolume => 0; /* ambient sound unsupported */
}