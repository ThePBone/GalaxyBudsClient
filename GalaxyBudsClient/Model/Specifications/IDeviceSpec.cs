using System;
using System.Collections.Generic;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications;

public interface IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules { get; }
    public Models Device { get; }
    public string DeviceBaseName { get; }
    public string FriendlyName => Device.GetModelMetadata()?.Name ?? "null";
    public ITouchMap TouchMap { get; }
    public Guid ServiceUuid { get; }
    public IEnumerable<TrayItemTypes> TrayShortcuts { get; }
    public string IconResourceKey { get; }
    public int MaximumAmbientVolume { get; }
        
    public bool Supports(Features feature)
    {
        if (TranslatorTools.GrantAllFeaturesForTesting)
            return true;
            
        if (!Rules.TryGetValue(feature, out var value))
        {
            return false;
        }

        if (value == null)
        {
            return true;
        }

        if (DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision == null)
        {
            Log.Warning("IDeviceSpec: Cannot compare revision for {Feature}. No ExtendedStatusUpdate cached", feature);
            return true;
        }
        
        return DeviceMessageCache.Instance.ExtendedStatusUpdate.Revision >= value.MinimumRevision;
    }

    public string RecommendedFwVersion(Features features)
    {
        return Rules.TryGetValue(features, out var value) ? value?.RecommendedFirmwareVersion ?? "Unset" : "N/A";
    }
}
    
public class FeatureRule(int minimumRevision, string? recommendedFirmwareVersion = null)
{
    public int MinimumRevision { get; } = minimumRevision;
    public string? RecommendedFirmwareVersion { get; } = recommendedFirmwareVersion;
}