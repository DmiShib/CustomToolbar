namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using Modules.Shared.Helpers;
    using System.Collections.Generic;


    internal sealed class ToolbarEnterPlayMode : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/EnterPlayModeOptions";
        public const string DEFAULT = "Default";

        private List<(string name, EnterPlayModeOptions? value)> _availableOptions;

        public static ToolbarEnterPlayMode Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Play Mode Options";
        protected override string Tooltip => "Configure 'Enter Play Mode' settings for faster iteration (Domain/Scene Reload).";
        protected override float Width => 125f;


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnInit()
        {
            _availableOptions = new()
            {
                (DEFAULT, null)
            };

            foreach (var option in Enum<EnterPlayModeOptions>.GetValues())
            {
                var name = Enum<EnterPlayModeOptions>.GetName(option);

                if (option == EnterPlayModeOptions.None || name.Equals("DisableSceneBackupUnlessDirty", StringComparison.OrdinalIgnoreCase))
                    continue;

                _availableOptions.Add((name, option));
            }
        }

        protected override MainToolbarElement CreateElement()
        {
            return new MainToolbarDropdown(new MainToolbarContent(GetCurrentOptionName(), Tooltip), BuildMenu);
        }

        private string GetCurrentOptionName()
        {
            if (!EditorSettings.enterPlayModeOptionsEnabled)
                return DEFAULT;

            var current = EditorSettings.enterPlayModeOptions;
            foreach (var option in _availableOptions)
            {
                if (option.value == current)
                    return option.name;
            }

            return DEFAULT;
        }

        private void BuildMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var currentName = GetCurrentOptionName();

            foreach (var option in _availableOptions)
            {
                menu.AddItem(new GUIContent(option.name), currentName == option.name, () =>
                {
                    if (option.value == null)
                    {
                        EditorSettings.enterPlayModeOptionsEnabled = false;
                    }
                    else
                    {
                        EditorSettings.enterPlayModeOptionsEnabled = true;
                        EditorSettings.enterPlayModeOptions = option.value.Value;
                    }

                    UpdateContentText(option.name);
                });
            }

            menu.DropDown(rect);
        }
    }
}