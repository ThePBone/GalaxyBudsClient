using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsPlusDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules => new()
            {
                { IDeviceSpec.Feature.AmbientSound, null },
                { IDeviceSpec.Feature.AmbientSidetone, new FeatureRule(8, "R175XXU0ASLE")  },
                { IDeviceSpec.Feature.AmbientExtraLoud, new FeatureRule(9, "R175XXU0ATB3")  },
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(11, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.FirmwareUpdates, new FeatureRule(8, "R175XXU0ASLE") },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.LegacyNoiseControlMode, null },
                { IDeviceSpec.Feature.DebugInfoLegacy, null },
                { IDeviceSpec.Feature.Voltage, null }
            };
        
        public Models Device => Models.BudsPlus;
        public string DeviceBaseName => "Galaxy Buds+ (";
        public ITouchOption TouchMap => new BudsPlusTouchOption();
        public Guid ServiceUuid => Uuids.BudsPlus;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            [
                ItemType.ToggleAmbient,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Bud";
    }
}