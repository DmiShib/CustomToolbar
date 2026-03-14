namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using UnityEngine.SceneManagement;


    internal sealed class ToolbarReloadScene : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/ReloadScene";

        public static ToolbarReloadScene Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Reload Scene";
        protected override string Tooltip => "Reloads the currently active scene (only in Play Mode).";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            SetEnabled(state == PlayModeStateChange.EnteredPlayMode);
        }

        protected override MainToolbarElement CreateElement()
        {
            Texture2D icon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;

            var button = new MainToolbarButton(new MainToolbarContent(icon, Tooltip), () =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });

            button.enabled = EditorApplication.isPlaying;
            return button;
        }
    }
}