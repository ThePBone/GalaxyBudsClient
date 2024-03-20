using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsProDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules => new()
            {
                { IDeviceSpec.Feature.SeamlessConnection, null },
                { IDeviceSpec.Feature.StereoPan, new FeatureRule(5, "R190XXU0AUA5") },
                { IDeviceSpec.Feature.DoubleTapVolume, new FeatureRule(7, "R190XXU0AUD1") },
                { IDeviceSpec.Feature.FirmwareUpdates, null },
                { IDeviceSpec.Feature.DetectConversations, null },
                { IDeviceSpec.Feature.NoiseControl, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.SpatialSensor, null },
                { IDeviceSpec.Feature.Voltage, null },
                { IDeviceSpec.Feature.BixbyWakeup, null },
                { IDeviceSpec.Feature.AncNoiseReductionLevels, null },
                { IDeviceSpec.Feature.LegacyNoiseControlMode, null },
                { IDeviceSpec.Feature.DebugInfoLegacy, null },
                { IDeviceSpec.Feature.AmbientSidetone, new FeatureRule(8, "R190XXU0AUI2")  },
                { IDeviceSpec.Feature.AmbientCustomize, new FeatureRule(8, "R190XXU0AUI2") },
                { IDeviceSpec.Feature.AmbientCustomizeLegacy, new FeatureRule(8, "R190XXU0AUI2") },
                { IDeviceSpec.Feature.DebugSku, null }
            };
        
        public Models Device => Models.BudsPro;
        public string DeviceBaseName => "Buds Pro";
        public ITouchOption TouchMap => new StandardTouchOption();
        public Guid ServiceUuid => Uuids.BudsPro;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            [
                ItemType.ToggleNoiseControl,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Pro";
    }
}