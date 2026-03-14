namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarFpsSlider : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/TargetFPS";
        private const int MIN_FPS_VALUE = 0;
        private const int MAX_FPS_VALUE = 600;
        private const string PREFS_KEY = "CustomToolbar.ToolbarFpsSlider.Value";

        public static ToolbarFpsSlider Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "FPS Slider";
        protected override string Tooltip => "Controls Application.targetFrameRate. Set to 0 for unlimited FPS.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var currentFps = EditorPrefs.GetInt(PREFS_KEY, MIN_FPS_VALUE);
            Application.targetFrameRate = (currentFps == MIN_FPS_VALUE) ? -1 : currentFps;

            var content = new MainToolbarContent("FPS", Tooltip);

            var slider = new MainToolbarSlider(content, currentFps, MIN_FPS_VALUE, MAX_FPS_VALUE,
                val =>
                {
                    var intVal = Mathf.RoundToInt(val);
                    Application.targetFrameRate = (intVal == MIN_FPS_VALUE) ? -1 : intVal;
                    EditorPrefs.SetInt(PREFS_KEY, intVal);
                });

            slider.populateContextMenu = menu =>
            {
                menu.AppendAction("Unlimited FPS (∞)", _ => SetFps(MIN_FPS_VALUE));
                menu.AppendSeparator();
                menu.AppendAction("Set to 30 FPS", _ => SetFps(30));
                menu.AppendAction("Set to 60 FPS", _ => SetFps(60));
                menu.AppendAction("Set to 120 FPS", _ => SetFps(120));
            };

            return slider;
        }

        private void SetFps(int fps)
        {
            Application.targetFrameRate = (fps == MIN_FPS_VALUE) ? -1 : fps;
            EditorPrefs.SetInt(PREFS_KEY, fps);
            RefreshUI();
        }
    }
}