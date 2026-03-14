namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor.Toolbars;
    using Modules.Shared.Helpers;
    using Modules.Shared.Environment;


    internal sealed class ToolbarEnvironmentSettings : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/Environment";

        public static ToolbarEnvironmentSettings Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Environment Settings";
        protected override float Width => 35f;


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var settings = HelperEditor.GetScriptableObject<EnvironmentSettings>();

            return CreateEnumDropdown(settings.Environment, selected =>
            {
                settings.SetEnvironment(selected, true);
            });
        }
    }
}