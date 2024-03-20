﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
    public class HotkeyPage : AbstractPage
    {
        public override Pages PageType => Pages.Hotkeys;
		
        private readonly ItemsControl _itemsControl;
        private HotkeyActionBuilder? _builder;

        public HotkeyPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _itemsControl = this.GetControl<ItemsControl>("ItemsControl");
        }
		
        public override void OnPageShown()
        {
            ReloadList();
        }

        private void UpdateEmptyView(bool empty)
        {
            this.GetControl<Border>("EmptyViewBorder").IsVisible = empty;
            this.GetControl<Border>("ContainerBorder").IsVisible = !empty;
        }
		
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Advanced);
        }

        private async void Add_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _builder?.Close();
            _builder = new HotkeyActionBuilder();
            await _builder.ShowDialog(MainWindow.Instance);
			
            if (_builder.Result && _builder.Hotkey != null)
            {
                SettingsProvider.Instance.Hotkeys = (SettingsProvider.Instance?.Hotkeys).Add(_builder.Hotkey);
                ReloadList();
                HotkeyReceiverImpl.Instance.Update();

                if (PlatformUtils.SupportsTrayIcon)
                {
                    SettingsProvider.Instance!.MinimizeToTray = true;
                }
            }
        }

        private void ReloadList()
        {
            var hotkeys = SettingsProvider.Instance?.Hotkeys ?? Array.Empty<Hotkey>();
            if (hotkeys.Length < 1)
            {
                UpdateEmptyView(true);
                return;
            }

            UpdateEmptyView(false);
            
            var items = new List<Hotkey>(hotkeys);
            items.ForEach(x => x.IsLastInList = false);
            items.Last().IsLastInList = true;
            _itemsControl.ItemsSource = items;
        }

        private void Item_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Control control)
            {
                if (control.Parent!.DataContext is Hotkey hotkey)
                {
                    MenuFactory.BuildContextMenu(new Dictionary<string, EventHandler<RoutedEventArgs>?>()
                    {
                        {
                            Loc.Resolve("hotkey_edit"),
                            async (_, args) =>
                            {
                                var index = SettingsProvider.Instance.Hotkeys.IndexOf(hotkey);
                                if (index < 0)
                                {
                                    Log.Debug("HotkeyPage.Edit: Cannot find hotkey in configuration");
                                    return;
                                }
                                
                                _builder?.Close();
                                _builder = new HotkeyActionBuilder(hotkey);
                                await _builder.ShowDialog(MainWindow.Instance);
			
                                if (_builder.Result && _builder.Hotkey != null)
                                {
                                    var saved = (SettingsProvider.Instance?.Hotkeys ?? Array.Empty<Hotkey>());
                                    var temp = new Hotkey[saved.Length];
                                    for (var i = 0; i < saved.Length; i++)
                                    {
                                        if (saved[i].Compare(hotkey))
                                        {
                                            Log.Debug("HotkeyPage.Edit: Updated hotkey '{Old}' to '{New}'", saved[i], _builder.Hotkey);
                                            temp[i] = _builder.Hotkey;
                                        }
                                        else
                                        {
                                            temp[i] = saved[i];
                                        }
                                    }

                                    SettingsProvider.Instance!.Hotkeys = temp;
                                    ReloadList();
                                    HotkeyReceiverImpl.Instance.Update();
                                }
                            }
                        },
                        {
                            Loc.Resolve("hotkey_delete"),
                            (_, args) =>
                            {
                                var saved = (SettingsProvider.Instance?.Hotkeys ?? Array.Empty<Hotkey>());
                                if (saved.Length <= 1)
                                {
                                    Log.Debug("HotkeyPage.Delete: Removed hotkey '{Key}'", saved[0]);
                                    SettingsProvider.Instance!.Hotkeys = Array.Empty<Hotkey>();
                                    ReloadList();
                                    HotkeyReceiverImpl.Instance.Update();
                                    return;
                                }
                                
                                Hotkey?[] temp = new Hotkey[saved.Length];
                                for (var i = 0; i < saved.Length; i++)
                                {
                                    if (saved[i].Compare(hotkey))
                                    {
                                        Log.Debug("HotkeyPage.Delete: Removed hotkey '{Key}'", saved[i]);
                                        temp[i] = null;
                                    }
                                    else
                                    {
                                        temp[i] = saved[i];
                                    }
                                }

                                SettingsProvider.Instance!.Hotkeys = temp.Where(x => x != null).Cast<Hotkey>().ToArray();
                                ReloadList();
                                HotkeyReceiverImpl.Instance.Update();
                            }
                        },
                    }, control).Open(control);
                } 
            }
        }
    }
}