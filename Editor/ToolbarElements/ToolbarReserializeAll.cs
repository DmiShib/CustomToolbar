namespace CustomToolbar.Editor.ToolbarElements
{
    using Utils;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarReserializeAll : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/ReserializeAll";

        public static ToolbarReserializeAll Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Reserialize All Assets";
        protected override string Tooltip => "Forces a re-serialization of all assets in the project. Useful after a Unity upgrade or to fix serialization errors.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            SetEnabled(state == PlayModeStateChange.EnteredEditMode);
        }

        protected override MainToolbarElement CreateElement()
        {
            Texture2D icon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;

            var button = new MainToolbarButton(new MainToolbarContent(icon, Tooltip), () =>
            {
                Debug.Log("Starting to force reserialize all assets...");
                SerializeAssetsUtils.ForceReserializeAllAssets();
            });

            button.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
            return button;
        }
    }
}