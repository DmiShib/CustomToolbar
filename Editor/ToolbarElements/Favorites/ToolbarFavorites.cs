namespace CustomToolbar.Editor.ToolbarElements.Favorites
{
    using Window;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarFavorites : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/Favorites";

        public static ToolbarFavorites Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Favorites";
        protected override string Tooltip => "Quick access to favorite assets and folders.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var icon = EditorGUIUtility.IconContent("d_Favorite Icon").image as Texture2D;
            var content = new MainToolbarContent(icon, Tooltip);
            return new MainToolbarButton(content, FavoritesWindow.ShowWindow);
        }
    }
}