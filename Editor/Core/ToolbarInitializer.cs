namespace CustomToolbar.Editor.Core
{
    using UnityEditor;


    [InitializeOnLoad]
    public static class ToolbarInitializer
    {
        static ToolbarInitializer()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
        }

        private static void HandlePlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Utils.SceneAssetsUtils.RestoreSceneAfterPlay();
            }
        }
    }
}