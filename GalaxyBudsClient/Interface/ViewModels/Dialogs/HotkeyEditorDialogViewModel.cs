﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs
{
    public class HotkeyEditorDialogViewModel : ViewModelBase
    {
        public HotkeyEditorDialogViewModel() : this(null) {}

        public HotkeyEditorDialogViewModel(Hotkey? hotkey)
        {
            hotkey ??= Hotkey.Empty;
            
            ModifierCtrl = hotkey.Modifier.Contains(ModifierKeys.Control);
            ModifierAlt = hotkey.Modifier.Contains(ModifierKeys.Alt);
            ModifierShift = hotkey.Modifier.Contains(ModifierKeys.Shift);
            ModifierWin = hotkey.Modifier.Contains(ModifierKeys.Win);
            Key1 = hotkey.Keys.FirstOrDefault();
            Action = hotkey.Action;
            VerifyAndMakeHotkey();
            
            PropertyChanged += OnPropertyChanged;
        }
        
        [Reactive] public bool ModifierCtrl { set; get; }
        [Reactive] public bool ModifierAlt { set; get; }
        [Reactive] public bool ModifierShift { set; get; }
        [Reactive] public bool ModifierWin { set; get; }
        [Reactive] public Keys? Key1 { set; get; }
        [Reactive] public Event? Action { set; get; }
        [Reactive] public string HotkeyPreview { set; get; } = Loc.Resolve("hotkey_edit_invalid");
        
        public static IEnumerable<Event> ActionSource =>
            Enum.GetValues(typeof(Event))
                .Cast<Event>()
                .Where(EventDispatcher.CheckDeviceSupport);

        public Hotkey? Hotkey => VerifyAndMakeHotkey();
        
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ModifierShift):  
                case nameof(ModifierWin):
                case nameof(ModifierAlt):
                case nameof(ModifierCtrl):
                case nameof(Key1):
                case nameof(Action):
                    VerifyAndMakeHotkey();
                    break;
            }
        }

        private Hotkey? VerifyAndMakeHotkey()
        {
            if (Key1 is null or Keys.None)
            {
                HotkeyPreview = Loc.Resolve("hotkey_edit_invalid");
                return null;
            }
            if (Action is null or Event.None)
            {
                HotkeyPreview = Loc.Resolve("hotkey_edit_invalid_action");
                return null;
            }

            var modifier = new List<ModifierKeys>();
            var keys = new List<Keys> { Key1.Value };

            if (ModifierCtrl)
                modifier.Add(ModifierKeys.Control);
            if (ModifierAlt)
                modifier.Add(ModifierKeys.Alt);
            if (ModifierWin)
                modifier.Add(ModifierKeys.Win);
            if (ModifierShift)
                modifier.Add(ModifierKeys.Shift);
                    
            if (modifier.Count < 1)
            {
                HotkeyPreview = Loc.Resolve("hotkey_edit_invalid_mod");
                return null;
            }
            
            HotkeyPreview = keys.AsHotkeyString(modifier);
            return new Hotkey(modifier, keys, Action!.Value);
        }
    }
}
