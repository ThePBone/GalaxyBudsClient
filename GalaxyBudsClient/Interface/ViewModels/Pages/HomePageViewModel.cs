﻿using System;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new HomePage();
    public override string TitleKey => "mainpage_header";
    public override Symbol IconKey => Symbol.Home;
    public override bool ShowsInFooter => false;
    
    private readonly DispatcherTimer _refreshTimer = new();
    
    public HomePageViewModel()
    {
        // Low refresh rate since we rely on status updates as cues
        _refreshTimer.Interval = new TimeSpan(0, 0, 15);
        _refreshTimer.Tick += async (_, _) =>
        {
            if (BluetoothImpl.Instance.IsConnected)
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
        };
        
        SppMessageHandler.Instance.StatusUpdate += OnStatusUpdateReceived;
    }

    private void OnStatusUpdateReceived(object? sender, StatusUpdateParser e)
    {
        /* Status updates are only sent if something has changed.
           We use this knowledge to request updated debug data. */
        _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
    }

    public override void OnNavigatedTo() => _refreshTimer.Start();
    public override void OnNavigatedFrom() => _refreshTimer.Stop();
}


