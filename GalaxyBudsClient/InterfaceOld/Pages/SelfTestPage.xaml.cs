﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Elements;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class SelfTestPage : AbstractPage
	{
		public override Pages PageType => Pages.SelfTest;
		
		private readonly PageHeader _pageHeader;
		private readonly DetailListItem _hwVer;
		private readonly DetailListItem _swVer;
		private readonly DetailListItem _touchFwVer;
		private readonly DetailListItem _btAddr;
		private readonly DetailListItem _proximity;
		private readonly DetailListItem _thermo;
		private readonly DetailListItem _adcSoc;
		private readonly DetailListItem _adcVoltage;
		private readonly DetailListItem _adcCurrent;
		private readonly DetailListItem _hall;
		private readonly DetailListItem _accel;

		private readonly IconListItem _result;

		private string Waiting => Loc.Resolve("system_waiting_for_device");
		private string Left => Loc.Resolve("left");
		private string Right => Loc.Resolve("right");
		
		public SelfTestPage()
		{   
			AvaloniaXamlLoader.Load(this);
			
			_pageHeader = this.GetControl<PageHeader>("PageHeader");
			_hwVer = this.GetControl<DetailListItem>("HwVer");
			_swVer = this.GetControl<DetailListItem>("SwVer");
			_touchFwVer = this.GetControl<DetailListItem>("TouchFwVer");
			_btAddr = this.GetControl<DetailListItem>("BtAddr");
			_proximity = this.GetControl<DetailListItem>("Proximity");
			_thermo = this.GetControl<DetailListItem>("Thermo");
			_adcSoc = this.GetControl<DetailListItem>("AdcSoc");
			_adcVoltage = this.GetControl<DetailListItem>("AdcVoltage");
			_adcCurrent = this.GetControl<DetailListItem>("AdcCurrent");
			_hall = this.GetControl<DetailListItem>("Hall");
			_accel = this.GetControl<DetailListItem>("Accelerator");
			
			_result = this.GetControl<IconListItem>("SelfTestResult");
			
			SppMessageHandler.Instance.SelfTestResponse += InstanceOnSelfTestResponse;
		}

		private void InstanceOnSelfTestResponse(object? sender, SelfTestParser e)
		{
			_hwVer.Description = strfy(e.HardwareVersion);
			_swVer.Description = strfy(e.SoftwareVersion);
			_touchFwVer.Description = strfy(e.TouchFirmwareVersion);
			_btAddr.Description = $"{Left}: {strfy(e.LeftBluetoothAddress)}, {Right}: {strfy(e.RightBluetoothAddress)}";
			_proximity.Description = $"{Left}: {strfy(e.LeftProximity)}, {Right}: {strfy(e.RightProximity)}";
			_thermo.Description = $"{Left}: {strfy(e.LeftThermistor)}, {Right}: {strfy(e.RightThermistor)}";
			_adcSoc.Description = $"{Left}: {strfy(e.LeftAdcSOC)}, {Right}: {strfy(e.RightAdcSOC)}";
			_adcVoltage.Description = $"{Left}: {strfy(e.LeftAdcVCell)}, {Right}: {strfy(e.RightAdcVCell)}";
			_adcCurrent.Description = $"{Left}: {strfy(e.LeftAdcCurrent)}, {Right}: {strfy(e.RightAdcCurrent)}";
			_hall.Description = $"{Left}: {strfy(e.LeftHall)}, {Right}: {strfy(e.RightHall)}";
			_accel.Description = $"{Left}: {strfy(e.AllLeftAccelerator)}, {Right}: {strfy(e.AllRightAccelerator)}";

			_result.Text = e.AllChecks ? Loc.Resolve("selftest_pass_long") : Loc.Resolve("selftest_fail_long");

			_pageHeader.LoadingSpinnerVisible = false;
		}

		private string strfy(bool b)
		{
			return b ? Loc.Resolve("selftest_pass") : Loc.Resolve("selftest_fail");
		}
		
		public override async void OnPageShown()
		{
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SELF_TEST);
			_pageHeader.LoadingSpinnerVisible = true;
			
			_hwVer.Description = Waiting;
			_swVer.Description = Waiting;
			_touchFwVer.Description = Waiting;
			_btAddr.Description = Waiting;
			_proximity.Description = Waiting;
			_thermo.Description = Waiting;
			_adcSoc.Description = Waiting;
			_adcVoltage.Description = Waiting;
			_adcCurrent.Description = Waiting;
			_hall.Description = Waiting;
			_accel.Description = Waiting;

			_result.Text = Waiting;
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}
	}
}
