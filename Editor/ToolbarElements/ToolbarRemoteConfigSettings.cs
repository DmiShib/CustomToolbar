namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor.Toolbars;


    internal sealed class ToolbarRemoteConfigSettings : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/RemoteConfig";

        public static ToolbarRemoteConfigSettings Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Remote Config Settings";
        protected override string Tooltip => "Select Remote Config Load mode";
        protected override float Width => 50f;


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var settings = Modules.Shared.Editor.RemoteConfig.Settings.RemoteConfigEditorSettings.Instance;

            return CreateEnumDropdown(settings.Load, selected =>
            {
                settings.SetRemoteConfigLoad(selected);
            });
        }
    }
}