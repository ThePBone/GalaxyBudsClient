using System.Collections;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if the connected device does not support the specified feature.
/// </summary>
public class RequiresAnyFeatureBehavior : Behavior<Control>
{
    public static readonly StyledProperty<IEnumerable> FeaturesProperty =
        AvaloniaProperty.Register<RequiresAnyFeatureBehavior, IEnumerable>(nameof(Features));
    
    public IEnumerable Features
    {
        get => GetValue(FeaturesProperty);
        set => SetValue(FeaturesProperty, value);
    }
    
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateState();
        Settings.Instance.RegisteredDevice.PropertyChanged += OnDevicePropertyChanged;
    }
    
    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        Settings.Instance.RegisteredDevice.PropertyChanged -= OnDevicePropertyChanged;
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateState();
    }
    
    protected virtual void UpdateState()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.IsVisible = Features.Cast<Features>().Any(BluetoothService.Instance.DeviceSpec.Supports);
    }
}