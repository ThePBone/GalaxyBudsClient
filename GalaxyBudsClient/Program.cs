﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GalaxyBudsClient.Cli;
using GalaxyBudsClient.Cli.Ipc;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Serilog;
using Serilog.Events;
#if !DEBUG
using Sentry;
#endif

namespace GalaxyBudsClient;

internal static class Program
{
    public static long StartedAt = 0;
    public static readonly string AvaresUrl = "avares://" + typeof(Program).Assembly.GetName().Name;
        
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        StartedAt = Stopwatch.GetTimestamp();
     
#if Windows
        ThePBone.Interop.Win32.WindowsUtils.AttachConsole();
#endif
        
        var config = new LoggerConfiguration()
            .WriteTo.Sentry(o =>
            {
                o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                o.MinimumEventLevel = LogEventLevel.Fatal;
            })
            .WriteTo.File(PlatformUtils.CombineDataPath("application.log"))
            .WriteTo.Console();

        config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ?
            config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
        // Divert program startup flow if the app was started with arguments (except /StartMinimized)
        var cliMode = args.Length > 0 && !args.Contains("/StartMinimized");
        if (cliMode)
        {
            // Disable excessive logging in CLI mode
            config = config.MinimumLevel.Warning();
        }
            
        Log.Logger = config.CreateLogger();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Trace.Listeners.Add(new ConsoleTraceListener());
            
        if (!Settings.Instance.DisableCrashReporting)
        {
            CrashReports.SetupCrashHandler();
        }
            
        if (cliMode)
        {
            CliHandler.ProcessArguments(args);
            return;
        }
        
        try
        {
            /* OSX: Graphics must be drawn on the main thread.
             * Awaiting this call would implicitly cause the next code to run as a async continuation task.
             *
             * In general: Don't await this call to shave off about 1000ms of startup time.
             * The IpcService will terminate the app in time if another instance is already running.
             */
            _ = Task.Run(IpcService.Setup);
                
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        }
        // ReSharper disable once RedundantCatchClause
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
        {
#if DEBUG
            throw;
#else
            SentrySdk.CaptureException(ex);
            Log.Error(ex, "Unhandled exception in main thread");
#endif
        }
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .With(new MacOSPlatformOptions
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/14577
                DisableSetProcessName = true
            })
            .WithInterFont()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();

}