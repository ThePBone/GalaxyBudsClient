using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class StubDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules => new();
        public Models Device => Models.NULL;
        public string DeviceBaseName => "STUB_DEVICE";
        public ITouchOption TouchMap => new StubTouchOption();
        public Guid ServiceUuid => new("{00000000-0000-0000-0000-000000000000}");
        public IReadOnlyCollection<ItemType> TrayShortcuts => new List<ItemType>();
        public string IconResourceKey => "Bud";
    }
}