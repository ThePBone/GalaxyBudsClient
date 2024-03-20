﻿using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GalaxyBudsClient.InterfaceOld.Elements;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class SpatialTestPage : AbstractPage
	{
		public override Pages PageType => Pages.SpatialTest;

		private readonly SpatialSensorManager _spatialSensorManager = new SpatialSensorManager();
		
		private readonly PageHeader _pageHeader;
		private readonly Image _arrow;
		private readonly TextBlock _details;

		public SpatialTestPage()
		{   
			AvaloniaXamlLoader.Load(this);
			
			_pageHeader = this.GetControl<PageHeader>("PageHeader");
			_arrow = this.GetControl<Image>("Arrow");
			_details = this.GetControl<TextBlock>("SpatialDetails");
			
			_spatialSensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
		}

		private void OnNewQuaternionReceived(object? sender, Quaternion e)
		{
			var rpy = e.ToRollPitchYaw();
			var degree = rpy[0].Remap(-3, 3, 0, 360);
			((RotateTransform) _arrow.RenderTransform!).Angle = degree;

			_details.Text = $"{Loc.Resolve("spatial_dump_quaternion")}\n" +
			                $"X={e.X}\nY={e.Y}\nZ={e.Z}\nW={e.W}\n\n" + 
			                $"{Loc.Resolve("spatial_dump_rpy")}\n" +
							$"Roll={rpy[0]}\nPitch={rpy[1]}\nYaw={rpy[2]}\n";
		}

		public override void OnPageShown()
		{
			_spatialSensorManager.Attach();
			
			_details.Text = Loc.Resolve("system_waiting_for_device");
		}
		
		public override void OnPageHidden()
		{
			_spatialSensorManager.Detach();
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}
	}
}
