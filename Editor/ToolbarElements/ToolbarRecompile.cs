namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using UnityEditor.Compilation;


    internal sealed class ToolbarRecompile : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/Recompile";

        public static ToolbarRecompile Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Recompile Scripts";
        protected override string Tooltip => "Request a manual script compilation.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnInit()
        {
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private void OnCompilationStarted(object obj)
        {
            SetEnabled(false);
        }

        private void OnCompilationFinished(object obj)
        {
            SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            SetEnabled(state == PlayModeStateChange.EnteredEditMode && !EditorApplication.isCompiling);
        }

        protected override MainToolbarElement CreateElement()
        {
            var icon = EditorGUIUtility.IconContent("d_debug").image as Texture2D;
            var content = new MainToolbarContent(icon, Tooltip);

            var button = new MainToolbarButton(content, CompilationPipeline.RequestScriptCompilation)
            {
                enabled = !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode
            };

            return button;
        }
    }
}