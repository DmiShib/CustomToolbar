namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor.Toolbars;
    using Modules.Shared.Helpers;
    using Modules.Shared.PlatformServices.Settings.ServiceProvider;


    internal sealed class ToolbarStoreType : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/StoreType";

        public static ToolbarStoreType Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Store Type";
        protected override float Width => 90f;


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var settings = HelperEditor.GetScriptableObject<PlatformSettings>();
            return CreateEnumDropdown(settings.StoreType, selected => settings.SetStore(selected, true));
        }
    }
}