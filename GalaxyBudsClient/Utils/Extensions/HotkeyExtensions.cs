using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Input;
using GalaxyBudsClient.Model.Hotkeys;

namespace GalaxyBudsClient.Utils.Extensions;

public static class HotkeyExtensions
{
    public static IEnumerable<ModifierKeys> FlagsToList(this ModifierKeys flags)
    {
        return flags.ToString()
            .Split([","], StringSplitOptions.RemoveEmptyEntries)
            .Select(
                str =>
                {
                    Enum.TryParse<ModifierKeys>(str , true , out var result);
                    return result;
                });
    }
        
    public static IEnumerable<Keys> FlagsToList(this Keys flags)
    {
        return flags.ToString()
            .Split([","], StringSplitOptions.RemoveEmptyEntries)
            .Select(
                str =>
                {
                    Enum.TryParse<Keys>(str , true , out var result);
                    return result;
                });
    }
        
    public static bool Compare(this Hotkey h1, Hotkey h2)
    {
        return h1.Keys.AsHotkeyString(h1.Modifier) == h2.Keys.AsHotkeyString(h2.Modifier) && h1.Action == h2.Action;
    }

    public static int IndexOf(this Hotkey[] c, Hotkey h)
    {
        for (var i = 0; i < c.Length; i++)
        {
            if (h.Compare(c[i]))
            {
                return i;
            }
        }

        return -1;
    }
        
    public static string AsAvaloniaHotkeyString(this IEnumerable<Key>? keys)
    {
        var first = true;
        var sb = new StringBuilder();

        if (keys == null)
            return "null";

        foreach (var key in keys)
        {
            if (!first)
            {
                sb.Append("+");
            }
            sb.Append(key);

            first = false;
        }

        return sb.ToString();
    }
        
    public static string AsHotkeyString(this IEnumerable<Keys>? keys, IEnumerable<ModifierKeys>? modifiers)
    {
        var first = true;
        var sb = new StringBuilder();

        if (keys == null && modifiers == null)
        {
            return "null";
        }
            
        foreach (var modifier in modifiers ?? Array.Empty<ModifierKeys>())
        {
            if (!first)
            {
                sb.Append("+");
            }
            sb.Append(modifier);

            first = false;
        }
            
        foreach (var key in keys ?? Array.Empty<Keys>())
        {
            if (!first)
            {
                sb.Append("+");
            }
            sb.Append(key);

            first = false;
        }

        return sb.ToString();
    }
}