﻿using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class EqualizerPageViewModel : MainPageViewModelBase
{
    public EqualizerPageViewModel()
    {
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsEqEnabled) or nameof(EqPreset):
                await MessageComposer.SetEqualizer(IsEqEnabled, (EqPresets)EqPreset, false);
                break;
            case nameof(StereoBalance):
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.SET_HEARING_ENHANCEMENTS, (byte)StereoBalance);
                break;
        }
    }

    protected override void OnEventReceived(Event type, object? parameter)
    {
        switch (type)
        {
            case Event.EqualizerToggle:
                IsEqEnabled = !IsEqEnabled;
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case Event.EqualizerNextPreset:
            {
                IsEqEnabled = true;
                EqPreset++;
                if (EqPreset > MaximumEqPreset)
                {
                    EqPreset = 0;
                }
                break;
            }
        }
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
    {
        using var suppressor = SuppressChangeNotifications();
        
        if (BluetoothService.ActiveModel == Models.Buds)
        {
            IsEqEnabled = e.EqualizerEnabled;
				
            var preset = e.EqualizerMode;
            if (preset > MaximumEqPreset)
            {
                /* 0 - 4: regular presets, 5 - 9: presets used when Dolby Atmos is enabled on the phone
                   There is no audible difference. */
                preset -= 5;
            }

            EqPreset = preset;
        }
        else
        {
            IsEqEnabled = e.EqualizerMode != 0;
            // If EQ disabled, set to Dynamic (2) by default
            EqPreset = e.EqualizerMode == 0 ? 2 : e.EqualizerMode - 1;
        }
        
        StereoBalance = e.HearingEnhancements;
    }

    public override Control CreateView() => new EqualizerPage();
    
    [Reactive] public bool IsEqEnabled { set; get; }
    [Reactive] public int EqPreset { set; get; }
    [Reactive] public int StereoBalance { set; get; }

    public int MaximumEqPreset => 4;
    public override string TitleKey => "eq_header";
    public override Symbol IconKey => Symbol.DeviceEq;
    public override bool ShowsInFooter => false;
}
