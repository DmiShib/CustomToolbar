namespace CustomToolbar.Editor.ToolbarElements
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using System.Collections.Generic;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;


    internal sealed class ToolbarSceneSelection : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/AllScenesSelection";

        public static ToolbarSceneSelection Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Scene Selection";
        protected override string Tooltip => "Select a scene from the 'Assets/' folder.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnInit()
        {
            EditorSceneManager.sceneOpened -= OnSceneChanged;
            EditorSceneManager.sceneOpened += OnSceneChanged;
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            SetEnabled(state == PlayModeStateChange.EnteredEditMode);
        }

        private void OnSceneChanged(Scene scene, OpenSceneMode mode)
        {
            RefreshButtonState();
        }

        protected override MainToolbarElement CreateElement()
        {
            var icon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
            var content = new MainToolbarContent(SceneManager.GetActiveScene().name, icon, Tooltip);

            return new MainToolbarDropdown(content, BuildAndShowMenu);
        }

        private void RefreshButtonState()
        {
            if (RootElement != null)
            {
                var icon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
                RootElement.content = new MainToolbarContent(SceneManager.GetActiveScene().name, icon, Tooltip);
                RefreshUI();
            }
        }

        private void BuildAndShowMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var allSceneGuids = AssetDatabase.FindAssets("t:scene", new[] { "Assets" });

            if (allSceneGuids.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes found in project"));
                menu.DropDown(rect);

                return;
            }

            var buildScenes = new List<(string path, int buildIndex)>();
            var otherScenes = new List<string>();
            var ignoredScenes = new HashSet<string> { "Basic", "Standard" };

            var buildDict = EditorBuildSettings.scenes
                .Select((s, i) => new { s.path, index = i })
                .Where(x => !string.IsNullOrEmpty(x.path))
                .ToDictionary(x => x.path, x => x.index);

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

            foreach (var (path, buildIndex) in buildScenes)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                menu.AddItem(new GUIContent($"{sceneName}   [{buildIndex}]"), false, () => OpenScene(path));
            }

            if (buildScenes.Count > 0 && otherScenes.Count > 0)
                menu.AddSeparator("");

            foreach (var path in otherScenes)
            {
                var menuPath = path.Replace("Assets/", "").Replace(".unity", "");
                menu.AddItem(new GUIContent(menuPath), false, () => OpenScene(path));
            }

            menu.DropDown(rect);
        }

        private static void OpenScene(string path)
        {
            if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(path);
        }
    }
}