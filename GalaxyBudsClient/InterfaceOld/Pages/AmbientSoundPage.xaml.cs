﻿using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
    public class AmbientSoundPage : AbstractPage
    {
        public override Pages PageType => Pages.AmbientSound;
        public bool AmbientEnabled => _ambientSwitch.IsChecked;
        
        private readonly SwitchListItem _ambientSwitch;
        private readonly SwitchListItem _voiceFocusSwitch;
        private readonly SliderListItem _volumeSlider;
        private readonly SwitchDetailListItem _extraLoud;
		
        private readonly Border _voiceFocusBorder;
        private readonly Border _extraLoudBorder;
		
        public AmbientSoundPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _ambientSwitch = this.GetControl<SwitchListItem>("AmbientToggle");
            _voiceFocusSwitch = this.GetControl<SwitchListItem>("AmbientVoiceFocusToggle");
            _volumeSlider = this.GetControl<SliderListItem>("AmbientVolume");
            _extraLoud = this.GetControl<SwitchDetailListItem>("AmbientExtraLoud");
			
            _voiceFocusBorder = this.GetControl<Border>("AmbientVoiceFocusBorder");
            _extraLoudBorder = this.GetControl<Border>("AmbientExtraLoudBorder");

            SppMessageHandler.Instance.AmbientEnabledUpdateResponse += (sender, b) => _ambientSwitch.IsChecked = b; 
            SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            
            Loc.LanguageUpdated += UpdateStrings;
            UpdateStrings();
        }

        private void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSound))
            {
                return;
            }

            Dispatcher.UIThread.Post(async () =>
            {
                switch (e)
                {
                    case EventDispatcher.Event.AmbientToggle:
                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE,
                            !_ambientSwitch.IsChecked);
                        _ambientSwitch.Toggle();
                        EventDispatcher.Instance.Dispatch(EventDispatcher.Event.UpdateTrayIcon);
                        break;
                    case EventDispatcher.Event.AmbientVolumeUp:
                        _ambientSwitch.IsChecked = true;
                        if (_volumeSlider.Value != _volumeSlider.Maximum)
                        {
                            _volumeSlider.Value += 1;
                        }

                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE,
                            true);
                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.AMBIENT_VOLUME,
                            (byte) _volumeSlider.Value);
                        EventDispatcher.Instance.Dispatch(EventDispatcher.Event.UpdateTrayIcon);
                        break;
                    case EventDispatcher.Event.AmbientVolumeDown:
                        if (_volumeSlider.Value <= 0)
                        {
                            _ambientSwitch.IsChecked = false;
                            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE,
                                false);
                        }
                        else
                        {
                            _ambientSwitch.IsChecked = true;
                            _volumeSlider.Value -= 1;

                            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE,
                                true);
                            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.AMBIENT_VOLUME,
                                (byte) _volumeSlider.Value);
                        }
                        EventDispatcher.Instance.Dispatch(EventDispatcher.Event.UpdateTrayIcon);

                        break;
                }
            });
        }

        private void UpdateStrings()
        {
            _volumeSlider.SubtitleDictionary = new Dictionary<int, string>()
            {
                { 0, Loc.Resolve("as_scale_very_low") },
                { 1, Loc.Resolve("as_scale_low") },
                { 2, Loc.Resolve("as_scale_moderate") },
                { 3, Loc.Resolve("as_scale_high") },
                { 4, Loc.Resolve("as_scale_extraloud") }
            };
        }

        public override void OnPageShown()
        {
            var supportsExtraLoud = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientExtraLoud);
            
            _voiceFocusBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientVoiceFocus);
            _extraLoudBorder.IsVisible = supportsExtraLoud;
			
            if (!supportsExtraLoud)
            {
                _volumeSlider.Maximum = 4;
            }
        }
		

        private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        { 
            if (BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientExtraLoud))
            {
                _extraLoud.IsChecked = e.ExtraHighAmbientEnabled;
                _volumeSlider.Maximum = e.ExtraHighAmbientEnabled ? 3 : 2;
            }
            
            if(BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientVoiceFocus))
            {
                _voiceFocusSwitch.IsChecked = e.AmbientSoundMode == AmbientType.VoiceFocus;
            }
			
            _ambientSwitch.IsChecked = e.AmbientSoundEnabled;
            _volumeSlider.Value = e.AmbientSoundVolume;
        }
		
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Home);
        }
		
        private async void AmbientToggle_OnToggled(object? sender, bool e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE, e);
            EventDispatcher.Instance.Dispatch(EventDispatcher.Event.UpdateTrayIcon);
        }

        private async void VoiceFocusToggle_OnToggled(object? sender, bool e)
        {
            var type = _voiceFocusSwitch.IsChecked ? AmbientType.VoiceFocus : AmbientType.Default;
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.AMBIENT_VOICE_FOCUS, e);
        }

        private async void VolumeSlider_OnChanged(object? sender, int e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.AMBIENT_VOLUME, (byte)e);
        }

        private async void ExtraLoud_OnToggled(object? sender, bool e)
        {
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientExtraLoud))
            {
                MainWindow.Instance.ShowUnsupportedFeaturePage(
                    string.Format(
                        Loc.Resolve("adv_required_firmware_later"), 
                        BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.AmbientExtraLoud)));
                return;
            }
			
            _volumeSlider.Maximum = e ? 3 : 2;
            if (e || _volumeSlider.Value >= 3)
                _volumeSlider.Value = _volumeSlider.Maximum;
			
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.EXTRA_HIGH_AMBIENT, e);
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.AMBIENT_VOLUME, (byte)_volumeSlider.Value);
        }
    }
}