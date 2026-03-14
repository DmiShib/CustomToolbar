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


    internal sealed class ToolbarSceneSelectionModern : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/FavoriteScenes";

        private List<string> _favoritePaths = new();

        public static ToolbarSceneSelectionModern Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Scene Selection";
        protected override string Tooltip => "Quick access to favorites, build scenes, and all project scenes";
        protected override float Width => 150f;


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
            var icon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
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

            // We collect a dictionary of scenes from Build Settings in advance for checking and sorting
            var buildDict = EditorBuildSettings.scenes
                .Select((s, i) => new { s.path, index = i })
                .Where(x => !string.IsNullOrEmpty(x.path))
                .ToDictionary(x => x.path, x => x.index);

            // ==========================================
            // Block 1: MANAGEMENT OF ELECTED
            // ==========================================
            if (isCurrentSaved)
            {
                if (isFavorite)
                    menu.AddItem(new GUIContent("－ Remove current from Favorites"), false, () => ToggleFavorite(currentPath, false));
                else
                    menu.AddItem(new GUIContent("＋ Add current to Favorites"), false, () => ToggleFavorite(currentPath, true));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Cannot add unsaved scene"));
            }

            menu.AddSeparator("");

            // ==========================================
            // BLOCK 2: LIST OF FAVORITE SCENES
            // ==========================================
            if (_favoritePaths.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No favorite scenes yet"));
            }
            else
            {
                // Prepare data for sorting: if it is in the build - sort by index, otherwise to the end of the list
                var favoritesData = _favoritePaths
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => new
                    {
                        Path = p,
                        Name = System.IO.Path.GetFileNameWithoutExtension(p),
                        HasBuildIndex = buildDict.TryGetValue(p, out var index),
                        BuildIndex = buildDict.GetValueOrDefault(p, int.MaxValue)
                    })
                    .OrderBy(x => x.BuildIndex)
                    .ThenBy(x => x.Name);

                foreach (var fav in favoritesData)
                {
                    var isOpened = fav.Path == currentPath;
                    string label = fav.HasBuildIndex ? $"❤︎ {fav.Name}   [{fav.BuildIndex}]" : $"❤︎ {fav.Name}";
                    menu.AddItem(new GUIContent(label), isOpened, () => OpenScene(fav.Path));
                }
            }

            menu.AddSeparator("");

            // ==========================================
            // DATA PREPARATION FOR ALL SCENES
            // ==========================================
            var allSceneGuids = AssetDatabase.FindAssets("t:scene", new[] { "Assets" });
            var buildScenes = new List<(string path, int buildIndex)>();
            var otherScenes = new List<string>();
            var ignoredScenes = new HashSet<string> { "Basic", "Standard" };

            foreach (var guid in allSceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (ignoredScenes.Contains(sceneName) || !path.StartsWith("Assets/")) continue;

                if (buildDict.TryGetValue(path, out var bIndex))
                    buildScenes.Add((path, bIndex));
                else
                    otherScenes.Add(path);
            }

            buildScenes.Sort((a, b) => a.buildIndex.CompareTo(b.buildIndex));
            otherScenes.Sort();

            // ==========================================
            // BOX 3: BUILD SETTINGS SCENES (In a separate folder)
            // ==========================================
            if (buildScenes.Count > 0)
            {
                foreach (var (path, buildIndex) in buildScenes)
                {
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                    var isOpened = path == currentPath;
                    menu.AddItem(new GUIContent($"Build Scenes/{sceneName}   [{buildIndex}]"), isOpened, () => OpenScene(path));
                }
            }

            menu.AddSeparator("");

            // ==========================================
            //BLOCK 4: ALL OTHER SCENES IN THE PROJECT
            // ==========================================
            foreach (var path in otherScenes)
            {
                // Replace slashes so that GenericMenu automatically creates beautiful subfolders
                var menuPath = path.Replace("Assets/", "").Replace(".unity", "");
                var isOpened = path == currentPath;

                menu.AddItem(new GUIContent(menuPath), isOpened, () => OpenScene(path));
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