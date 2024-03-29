using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class Buds2DeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, null },
        { Features.StereoPan, null},
        { Features.FirmwareUpdates, null },
        { Features.NoiseControl, null },
        { Features.AmbientSound, null },
        { Features.Anc, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.BixbyWakeup, null },
        { Features.GearFitTest, null },
        { Features.DoubleTapVolume, new FeatureRule(5, "R177XXU0AUI2") },
        { Features.AdvancedTouchLock, new FeatureRule(4, "R177XXU0AUH1") },
        { Features.AdvancedTouchLockForCalls, new FeatureRule(7) },
        { Features.AncWithOneEarbud, new FeatureRule(3, "R177XXU0AUH1") },
        { Features.AmbientCustomize, new FeatureRule(5, "R177XXU0AUH1") },
        { Features.AmbientSidetone, new FeatureRule(6, "R177XXU0AUI2")  },
        { Features.FmgRingWhileWearing, new FeatureRule(9) },
        { Features.DebugSku, null },
        { Features.CallPathControl, new FeatureRule(7) },
        { Features.ChargingState, new FeatureRule(10) },
        { Features.NoiseControlModeDualSide, new FeatureRule(5) },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null }
    };
        
    public Models Device => Models.Buds2;
    public string DeviceBaseName => "Buds2";
    public ITouchMap TouchMap => new StandardTouchMap();
    public Guid ServiceUuid => Uuids.Buds2;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleNoiseControl,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 2;
}