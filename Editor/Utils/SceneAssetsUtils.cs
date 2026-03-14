namespace CustomToolbar.Editor.Utils
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.SceneManagement;


    internal static class SceneAssetsUtils
    {
        private const string LAST_SCENE_SETUP_STATE_KEY = "CustomToolbar.LastSceneSetup";

        [Serializable]
        private class SerializableSceneSetup
        {
            public string path;
            public bool isLoaded;
            public bool isActive;

            public static SerializableSceneSetup FromSceneSetup(SceneSetup setup) => new()
            {
                path = setup.path,
                isLoaded = setup.isLoaded,
                isActive = setup.isActive
            };
        }

        [Serializable]
        private class SceneSetupWrapper
        {
            public SerializableSceneSetup[] setups;
        }

        public static void StartPlayModeFromFirstScene()
        {
            if (EditorApplication.isPlaying) return;

            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogWarning("[CustomToolbar] Cannot start from first scene: No scenes in Build Settings.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var currentSceneSetup = EditorSceneManager.GetSceneManagerSetup();
                var wrapper = new SceneSetupWrapper
                {
                    setups = currentSceneSetup.Select(SerializableSceneSetup.FromSceneSetup).ToArray()
                };

                SessionState.SetString(LAST_SCENE_SETUP_STATE_KEY, JsonUtility.ToJson(wrapper));

                string firstScenePath = EditorBuildSettings.scenes[0].path;
                EditorSceneManager.OpenScene(firstScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        public static void RestoreSceneAfterPlay()
        {
            string jsonSetup = SessionState.GetString(LAST_SCENE_SETUP_STATE_KEY, string.Empty);
            if (string.IsNullOrEmpty(jsonSetup)) return;

            var wrapper = JsonUtility.FromJson<SceneSetupWrapper>(jsonSetup);
            if (wrapper?.setups is { Length: > 0 })
            {
                foreach (var setup in wrapper.setups)
                {
                    if (!File.Exists(setup.path))
                    {
                        Debug.LogWarning($"[CustomToolbar] Could not restore scene setup. File not found: {setup.path}");
                        SessionState.EraseString(LAST_SCENE_SETUP_STATE_KEY);
                        return;
                    }
                }

                var sceneSetupsToRestore = wrapper.setups.Select(s => new SceneSetup
                {
                    path = s.path,
                    isLoaded = s.isLoaded,
                    isActive = s.isActive
                }).ToArray();

                EditorSceneManager.RestoreSceneManagerSetup(sceneSetupsToRestore);
            }

            SessionState.EraseString(LAST_SCENE_SETUP_STATE_KEY);
        }
    }
}