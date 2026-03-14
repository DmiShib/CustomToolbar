namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarClearPlayerPrefs : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/ClearPlayerPrefs";

        public static ToolbarClearPlayerPrefs Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Clear PlayerPrefs";
        protected override string Tooltip => "Deletes all keys and values from PlayerPrefs. This cannot be undone.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            Texture2D icon = EditorGUIUtility.IconContent("d_TreeEditor.Trash").image as Texture2D;
            return new MainToolbarButton(new MainToolbarContent(icon, Tooltip), OnClicked);
        }

        private void OnClicked()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs",
                    "Are you sure you want to delete all PlayerPrefs? This action cannot be undone.",
                    "Yes, delete them", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("[CustomToolbar] PlayerPrefs cleared successfully.");
            }
        }
    }
}