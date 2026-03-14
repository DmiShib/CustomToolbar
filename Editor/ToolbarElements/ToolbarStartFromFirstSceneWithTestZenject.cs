namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEngine;
    using UnityEditor;
    using Zenject.Internal;
    using UnityEditor.Toolbars;
    using UnityEngine.SceneManagement;


    internal sealed class ToolbarStartFromFirstSceneWithTestZenject : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/StartFromFirstSceneZenject";

        public static ToolbarStartFromFirstSceneWithTestZenject Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Start from first scene with test zenject";
        protected override string Tooltip => "Save, validate scenes and start from build index 0";


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
            var iconPath = $"{PACKAGE_ROOT_PATH}/Editor/Icons/LookDevSingleZenject.png";
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            var button = new MainToolbarButton(new MainToolbarContent(icon, Tooltip), OnButtonClicked);

            button.enabled = !EditorApplication.isPlayingOrWillChangePlaymode &&
                             SceneUtility.GetBuildIndexByScenePath(SceneUtility.GetScenePathByBuildIndex(0)) != -1;
            return button;
        }

        private void OnButtonClicked()
        {
            var isValid = ZenUnityEditorUtil.SaveThenRunPreserveSceneSetup(() =>
            {
                var numValidated = ZenUnityEditorUtil.ValidateAllActiveScenes();
                Debug.Log($"Validated all '{numValidated}' active scenes successfully");
            });

            if (isValid)
                Utils.SceneAssetsUtils.StartPlayModeFromFirstScene();
        }
    }
}