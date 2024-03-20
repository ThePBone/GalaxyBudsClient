﻿using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class BixbyRemapPage : AbstractPage
	{
		public override Pages PageType => Pages.BixbyRemap;
		
		private readonly SwitchDetailListItem _bixbyToggle;
		private readonly MenuDetailListItem _bixbyLang;
		private readonly MenuDetailListItem _bixbyAction;
		
		public BixbyRemapPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_bixbyToggle = this.GetControl<SwitchDetailListItem>("BixbyToggle");
			_bixbyLang = this.GetControl<MenuDetailListItem>("BixbyLanguage");
			_bixbyAction = this.GetControl<MenuDetailListItem>("BixbyRemapAction");
			
			SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
			Loc.LanguageUpdated += UpdateMenu;
			
			UpdateMenu();
		}

		private void UpdateMenu()
		{
			var items = new Dictionary<string, EventHandler<RoutedEventArgs>?>{};
			foreach (var (id, name) in Bixby.Languages)
			{
				items.Add(name, async (sender, args) =>
				{
					_bixbyLang.Description = name;
					await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.VOICE_WAKE_UP_LANGUAGE, id);
				});
			}

			_bixbyLang.Items = items;
			
			var items_act = new Dictionary<string, EventHandler<RoutedEventArgs>?>{};
			foreach (var id in ((EventDispatcher.Event[]) Enum.GetValues(typeof(EventDispatcher.Event))))
			{
				if (!EventDispatcher.CheckDeviceSupport(id)) continue;
				items_act.Add(id.GetDescription(), (sender, args) =>
				{
					if (id == EventDispatcher.Event.Connect)
					{
						return;
					}
					
					_bixbyAction.Description = id.GetDescription();
					Settings.Instance.BixbyRemapEvent = id;
				});
			}
			_bixbyAction.Items = items_act;
		}
		
		private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_bixbyToggle.IsChecked = e.VoiceWakeUp;
			try
			{
				_bixbyLang.Description = Bixby.Languages[e.VoiceWakeUpLang].Item2;
			}
			catch (Exception ex)
			{
				_bixbyLang.Description = Loc.Resolve("unknown");
				Log.Error(ex, "BixbyRemapPage");
			}
		}

		public override void OnPageShown()
		{
			if (DeviceMessageCache.Instance.ExtendedStatusUpdate != null)
			{
				OnExtendedStatusUpdate(null, DeviceMessageCache.Instance.ExtendedStatusUpdate);
			}
			_bixbyAction.Description = Settings.Instance.BixbyRemapEvent.GetDescription();
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Advanced);
		}
		
		private async void BixbyEnable_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_VOICE_WAKE_UP, e);
		}
	}
}
