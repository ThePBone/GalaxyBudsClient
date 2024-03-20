﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class TouchpadGesturePage : AbstractPage
	{
		public override Pages PageType => Pages.TouchGesture;

		public readonly SwitchDetailListItem Single;
		public readonly SwitchDetailListItem Double;
		public readonly SwitchDetailListItem Triple;
		public readonly SwitchDetailListItem Hold;

		public TouchpadGesturePage()
		{   
			AvaloniaXamlLoader.Load(this);
			Single = this.GetControl<SwitchDetailListItem>("SingleTouch");
			Double = this.GetControl<SwitchDetailListItem>("DoubleTouch");
			Triple = this.GetControl<SwitchDetailListItem>("TripleTouch");
			Hold = this.GetControl<SwitchDetailListItem>("HoldTouch");

            SppMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
		}
		
        private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
	        Single.IsChecked = e.SingleTapOn;
	        Double.IsChecked = e.DoubleTapOn;
	        Triple.IsChecked = e.TripleTapOn;
	        Hold.IsChecked = e.TouchHoldOn;
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Touch);
		}
        
		private async void UpdateLockState(object? sender, bool e)
		{
			var locked = false;
			if (MainWindow.Instance.Pager.FindPage(Pages.Touch) is TouchpadPage page)
			{
				locked = page.TouchpadLocked;
			}

			await BluetoothImpl.Instance.SendAsync(LockTouchpadEncoder.Build(locked, Single.IsChecked,
				Double.IsChecked, Triple.IsChecked, Hold.IsChecked));
		}
	}
}
