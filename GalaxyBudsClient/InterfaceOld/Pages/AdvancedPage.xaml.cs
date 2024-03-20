﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class AdvancedPage : AbstractPage
	{
		public override Pages PageType => Pages.Advanced;
		
		private readonly SwitchDetailListItem _seamlessConnection;
		private readonly SwitchDetailListItem _resumeSensor;
		private readonly SwitchDetailListItem _sidetone;
		private readonly SwitchDetailListItem _passthrough;
		
		public AdvancedPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_seamlessConnection = this.GetControl<SwitchDetailListItem>("SeamlessConnection");
			_resumeSensor = this.GetControl<SwitchDetailListItem>("ResumeSensor");
			_sidetone = this.GetControl<SwitchDetailListItem>("Sidetone");
			_passthrough = this.GetControl<SwitchDetailListItem>("Passthrough");
			
			SppMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
		}

		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_sidetone.IsChecked = e.SideToneEnabled;
			_passthrough.IsChecked = e.RelieveAmbient;

			_seamlessConnection.IsChecked = e.SeamlessConnectionEnabled;
			_resumeSensor.IsChecked = SettingsProvider.Instance.ResumePlaybackOnSensor;
		}

		public override void OnPageShown()
		{
			this.GetControl<Border>("HotkeysBixby").IsVisible =
					PlatformUtils.SupportsHotkeys || BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.BixbyWakeup);
			this.GetControl<Separator>("BixbyRemapS").IsVisible =
				PlatformUtils.SupportsHotkeys && BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.BixbyWakeup);
			this.GetControl<DetailListItem>("Hotkeys").IsVisible = PlatformUtils.SupportsHotkeys;
			this.GetControl<Border>("BixbyRemap").IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.BixbyWakeup);
			this.GetControl<Separator>("SidetoneS").IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone);
			this.GetControl<Separator>("PassthroughS").IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientPassthrough);
			_sidetone.GetVisualParent()!.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone);
			_passthrough.GetVisualParent()!.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientPassthrough);
			this.GetControl<Border>("GearFitTest").IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.GearFitTest);
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private async void SeamlessConnection_OnToggled(object? sender, bool e)
		{
			if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.SeamlessConnection))
			{
				MainWindow.Instance.ShowUnsupportedFeaturePage(
					string.Format(
						Loc.Resolve("adv_required_firmware_later"), 
						BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.SeamlessConnection)));
				return;
			}
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SEAMLESS_CONNECTION, !e);
		}

		private void ResumeSensor_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.ResumePlaybackOnSensor = e;
		}

		private async void Sidetone_OnToggled(object? sender, bool e)
		{
			if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone))
			{
				MainWindow.Instance.ShowUnsupportedFeaturePage(
					string.Format(
						Loc.Resolve("adv_required_firmware_later"),
						BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.AmbientSidetone)));
				return;
			}
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SIDETONE, e);
		}

		private async void Passthrough_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.PASS_THROUGH, e);
		}

		private void Hotkeys_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Hotkeys);
		}

		private void BixbyRemap_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.BixbyRemap);
		}

		private void GearFitTest_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.GearFitTest);
		}
	}
}
