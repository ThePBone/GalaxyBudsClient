using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public interface IDeviceSpec
    {
        public enum Feature
        {
            SeamlessConnection,
            AmbientExtraLoud,
            AmbientVoiceFocus,
            AmbientSidetone,
            AmbientPassthrough,
            /// <summary>Device supports Ambient sound only</summary>
            AmbientSound,
            /// <summary>Device supports ANC only</summary>
            Anc,
            /// <summary>Device supports all noise controls</summary>
            NoiseControl,
            BixbyWakeup,
            DetectConversations,
            GamingMode,
            DoubleTapVolume,
            CaseBattery,
            Voltage,
            Current,
            FragmentedMessages,
            StereoPan,
            FirmwareUpdates,
            SpatialSensor,
            AmbientCustomize,
            AmbientCustomizeLegacy,
            AncWithOneEarbud,
            AncNoiseReductionLevels,
            AdvancedTouchLock,
            LegacyNoiseControlMode,
            DebugInfoLegacy,
            GearFitTest,
            DebugSku
        }
        
        public Dictionary<Feature, FeatureRule?> Rules { get; }
        public Models Device { get; }
        public string DeviceBaseName { get; }
        public string FriendlyName => Device.GetModelMetadata()?.Name ?? "null";
        public ITouchOption TouchMap { get; }
        public Guid ServiceUuid { get; }
        public IReadOnlyCollection<ItemType> TrayShortcuts { get; }
        public string IconResourceKey { get; }
        
        public bool Supports(Feature feature)
        {
            // TODO remove this
            return feature != Feature.Anc;
            
            if (!Rules.ContainsKey(feature))
            {
                return false;
            }

            if (Rules[feature] == null)
            {
                return true;
            }

            if (DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision == null)
            {
                Log.Warning("IDeviceSpec: Cannot compare revision. No ExtendedStatusUpdate cached");
                return true;
            }

            return DeviceMessageCache.Instance.ExtendedStatusUpdate.Revision >= Rules[feature]?.MinimumRevision;
        }

        public string RecommendedFwVersion(Feature feature)
        {
            return Rules.ContainsKey(feature) ? Rules[feature]?.RecommendedFirmwareVersion ?? "---" : "???";
        }
    }
    
    public class FeatureRule
    {
        public FeatureRule(int minimumRevision, string recommendedFirmwareVersion)
        {
            MinimumRevision = minimumRevision;
            RecommendedFirmwareVersion = recommendedFirmwareVersion;
        }
        public int MinimumRevision { get; }
        public string RecommendedFirmwareVersion { get; }
    }
}