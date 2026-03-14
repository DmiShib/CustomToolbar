namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using System.Collections.Generic;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;


    internal sealed class ToolbarSceneSelectionModern : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/FavoriteScenes";

        public static ToolbarSceneSelectionModern Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Favorite Scenes";
        protected override string Tooltip => "Quick access to favorite scenes";
        private List<string> _favoritePaths = new();


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnInit()
        {
            LoadFromPlayerPrefs();
            EditorSceneManager.sceneOpened -= HandleSceneOpened;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            SetEnabled(state == PlayModeStateChange.EnteredEditMode);
        }

        private void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RefreshButtonState();
        }

        protected override MainToolbarElement CreateElement()
        {
            return new MainToolbarDropdown(GetCurrentContent(), BuildAndShowMenu);
        }

        private void RefreshButtonState()
        {
            if (RootElement != null)
            {
                RootElement.content = GetCurrentContent();
                RefreshUI();
            }
        }

        private MainToolbarContent GetCurrentContent()
        {
            var icon = EditorGUIUtility.IconContent("d_Favorite Icon").image as Texture2D;
            var sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "Unsaved Scene";

            return new MainToolbarContent(sceneName, icon, Tooltip);
        }

        private void BuildAndShowMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var currentPath = SceneManager.GetActiveScene().path;
            var isCurrentSaved = !string.IsNullOrEmpty(currentPath);
            var isFavorite = isCurrentSaved && _favoritePaths.Contains(currentPath);

            if (isCurrentSaved)
            {
                if (isFavorite)
                    menu.AddItem(new GUIContent("➖ Remove current from Favorites"), false, () => ToggleFavorite(currentPath, false));
                else
                    menu.AddItem(new GUIContent("➕ Add current to Favorites"), false, () => ToggleFavorite(currentPath, true));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Cannot add unsaved scene"));
            }

            menu.AddSeparator("");

            if (_favoritePaths.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No favorite scenes yet"));
            }
            else
            {
                foreach (var path in _favoritePaths)
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                    var isOpened = path == currentPath;

                    menu.AddItem(new GUIContent(sceneName), isOpened, () => OpenScene(path));
                }
            }

            menu.DropDown(rect);
        }

        private void ToggleFavorite(string path, bool add)
        {
            if (add && !_favoritePaths.Contains(path)) _favoritePaths.Add(path);
            else if (!add) _favoritePaths.Remove(path);

            SaveToPlayerPrefs();
            RefreshButtonState();
        }

        private void OpenScene(string path)
        {
            if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(path);
        }

        private void SaveToPlayerPrefs()
        {
            var serialized = string.Join(";", _favoritePaths);
            EditorPrefs.SetString($"{Application.productName}_SceneSelectionToolbar.Scenes", serialized);
        }

        private void LoadFromPlayerPrefs()
        {
            var serialized = EditorPrefs.GetString($"{Application.productName}_SceneSelectionToolbar.Scenes", "");
            _favoritePaths = serialized.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}