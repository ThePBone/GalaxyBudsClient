﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using NetSparkleUpdater;
using Serilog;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class UpdatePage : AbstractPage
	{
		public override Pages PageType => Pages.UpdateAvailable;
		
		private readonly TextBlock _versionTitle;

		private event EventHandler<AppCastItem>? UpdateInstallerHandler; 
		private AppCastItem? _appcastItem;
		private Pages _previousPage = Pages.Home;
		
		public UpdatePage()
		{   
			AvaloniaXamlLoader.Load(this);
			_versionTitle = this.GetControl<TextBlock>("VersionTitle");
			
			Loc.LanguageUpdated += OnLanguageUpdated;
		}

		private void OnLanguageUpdated()
		{
			_versionTitle.Text = string.Format(Loc.Resolve("updater_newrelease"), _appcastItem?.Version ?? "x.x.x.x");
		}

		private void ViewChangelog_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
		}
		
		private void OpenWebsite(String url)
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		public void SetUpdate(List<AppCastItem> items, bool silent)
		{
			foreach(var item in items)
			{
                if (item.IsWindowsUpdate && PlatformUtils.IsWindows)
				{
					UpdateInstallerHandler = OnInstall_Windows;
					Select(item, silent);
					break;
				}
				
				if (item.IsLinuxUpdate && PlatformUtils.IsLinux)
				{
					UpdateInstallerHandler = OnInstall_Linux;
					Select(item, silent);
					break;
				}

				if (item.IsMacOSUpdate && PlatformUtils.IsOSX)
				{
					UpdateInstallerHandler = OnInstall_OSX;
					Select(item, silent);
					break;
				}
			}
		}

		private void OnInstall_Windows(object? sender, AppCastItem e)
        {
	        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
	        {
		        MainWindow.Instance.UpdateProgressPage.BeginUpdate(e);
	        }
	        else
	        {
		        Log.Warning("UpdatePage: Only x64 Windows builds have updater support");
		        OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
	        }
        }

		private void OnInstall_Linux(object? sender, AppCastItem e)
		{
			OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
		}

		private void OnInstall_OSX(object? sender, AppCastItem e)
		{
			OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
		}

		private void Select(AppCastItem item, bool silent)
		{
			_appcastItem = item;

			if (silent && Settings.Instance.UpdateSkippedVersion == item.Version)
			{
				Log.Information("UpdateManager: Ignoring new update {Version}; skipped by user", item.Version);
				return;
			}
			
			Dispatcher.UIThread.Post(() => _versionTitle.Text = string.Format(Loc.Resolve("updater_newrelease"), item.Version));
			
			_previousPage = MainWindow.Instance.Pager.CurrentPage;
			MainWindow.Instance.Pager.SwitchPage(Pages.UpdateAvailable);
		}

		private void Later_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(_previousPage);
		}

		private void Skip_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			Settings.Instance.UpdateSkippedVersion = _appcastItem?.Version ?? string.Empty;
			MainWindow.Instance.Pager.SwitchPage(_previousPage);
		}

		private void UpdateNow_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (_appcastItem == null)
			{
				new MessageBox()
				{
					Title = Loc.Resolve("error"),
					Description = Loc.Resolve("updater_appcast_null")
				}.ShowDialog(MainWindow.Instance);
				return;
			}
			
			UpdateInstallerHandler?.Invoke(this, _appcastItem);
		}
	}
}
