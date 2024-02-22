using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using GalaxyBudsClient.Cli.Ipc;
using GalaxyBudsClient.Cli.Ipc.Objects;
using GalaxyBudsClient.Utils.Extensions;
using Newtonsoft.Json;
using Serilog;
using Tmds.DBus;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace GalaxyBudsClient.Cli;

public static class CliHandler
{
    [Verb("action", HelpText = "List and execute app actions")]
    
    public class ActionOptions {
        [Option( 'l', "list", Required = false, HelpText = "Describe available actions")]
        public bool ListActions { get; set; }
        [Option('e', "execute", Required = false, MetaValue = "action", HelpText = "Execute action by identifier")]
        public string? ExecuteAction { get; set; }
    }
    
    [Verb("app", HelpText = "Application options")]
    public class AppOptions {
        [Option( 'b', "show-battery-popup", Required = false, HelpText = "Show the battery popup")]
        public bool ShowBatteryPopup { get; set; }    
        [Option( 'a', "activate-window", Required = false, HelpText = "Bring the app window to the front")]
        public bool ActivateWindow { get; set; }
    }
    
    [Verb("device", HelpText = "Device options")]
    public class DeviceOptions {
        [Option( 'G', "get-all", Required = false, HelpText = "Get all available device properties")]
        public bool GetAllProperties { get; set; }    
        [Option( 'g', "get", Required = false, MetaValue = "name", HelpText = "Get a device property")]
        public string? GetProperty { get; set; } 
        [Option( 'j', "json", Required = false, HelpText = "Serialize output as JSON")]
        public bool UseJson { get; set; }    
    }

    
    public static void ProcessArguments(IEnumerable<string> args)
    {
        using var parser = new Parser(s =>
        {
            s.AutoHelp = true;
            s.AutoVersion = true;
            s.HelpWriter = Console.Out;
        });

        try
        {
            parser.ParseArguments<ActionOptions, AppOptions, DeviceOptions>(args)
                .MapResult(
                    (ActionOptions opts) => ProcessActionVerb(opts).WaitAndReturnResult(),
                    (AppOptions opts) => ProcessAppVerb(opts).WaitAndReturnResult(),
                    (DeviceOptions opts) => ProcessDeviceVerb(opts).WaitAndReturnResult(),
                    errs => false);
        }
        catch (AggregateException e)
        {
            if(e.InnerExceptions.Any(x => x is DBusException { ErrorName: "org.freedesktop.DBus.Error.ServiceUnknown" }))
                Console.Error.WriteLine("\nError: The Galaxy Buds Manager app is not running. The CLI can only be used while the app is running.");
            else
                Console.Error.WriteLine("\nError: " + e.Message);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("\nError: " + e.Message);
        }
        
        Console.Out.Flush();
        Console.Error.Flush();
    }
    
    private static async Task<Connection?> OpenConnection()
    {
        try
        {
            return await IpcService.OpenClientConnectionAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to open connection to IPC service");
            await Console.Error.WriteLineAsync("\nError: Failed to connect to the Galaxy Buds Manager app. The CLI can only be used while the app is running.");
            await Console.Error.FlushAsync();
            Environment.Exit(1);
            return null;
        }
    }
    
    private static async Task<bool> ProcessActionVerb(ActionOptions opts)
    {
        using var client = await OpenConnection();
        if (client is null)
            return false;
        
        var proxy = client.CreateProxy<IApplicationObject>(IpcService.ServiceName, ApplicationObject.Path);

        if (opts.ListActions)
        {
            var actions = await proxy.ListActionsAsync();
            foreach (var (id, description) in actions)
            {
                Console.WriteLine($"{id}: {description}");
            }
        }
        else if (opts.ExecuteAction != null)
        {
            await proxy.ExecuteActionAsync(opts.ExecuteAction);
        }
        else
        {
            return false;
        }

        return true;
    }

    private static async Task<bool> ProcessAppVerb(AppOptions opts)
    {
        using var client = await OpenConnection();
        if (client is null)
            return false;
        
        var proxy = client.CreateProxy<IApplicationObject>(IpcService.ServiceName, ApplicationObject.Path);

        if (opts.ActivateWindow)
            await proxy.ActivateAsync();
        else if (opts.ShowBatteryPopup)
            await proxy.ShowBatteryPopupAsync();
        else
            return false;
        return true;
    }
    
    private static async Task<bool> ProcessDeviceVerb(DeviceOptions opts)
    {
        using var client = await OpenConnection();
        if (client is null)
            return false;
        
        var proxy = client.CreateProxy<IDeviceObject>(IpcService.ServiceName, DeviceObject.Path);

        if (opts.GetAllProperties)
        {
            var props = await proxy.GetAllAsync();
            var dict = props.GetAll();
            if(opts.UseJson)
                Console.WriteLine(JsonConvert.SerializeObject(dict, Formatting.Indented));
            else
            {
                foreach (var kv in dict)
                {
                    Console.WriteLine($"{kv.Key} = {kv.Value}");
                }
            }
        }
        else if (opts.GetProperty != null)
        {
            var prop = await proxy.GetAsync(opts.GetProperty);
            Console.WriteLine(opts.UseJson ? JsonConvert.SerializeObject(prop, Formatting.Indented) : $"{prop}");
        }
        else
            return false;
        return true;
    }
}