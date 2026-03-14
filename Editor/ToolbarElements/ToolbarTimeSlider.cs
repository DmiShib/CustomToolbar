namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarTimeSlider : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/TimeScale";
        private const float MIN_TIME_SCALE = 0f;
        private const float MAX_TIME_SCALE = 10f;
        private const string PREFS_KEY = "CustomToolbar.ToolbarTimeSlider.Value";

        public static ToolbarTimeSlider Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Timescale Slider";
        protected override string Tooltip => "Controls Time.timeScale to slow down or speed up the game.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        public override void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.EnteredEditMode)
            {
                Time.timeScale = 1.0f;
                EditorPrefs.SetFloat(PREFS_KEY, 1.0f);
                RefreshUI();
            }
            SetEnabled(state == PlayModeStateChange.EnteredPlayMode);
        }

        protected override MainToolbarElement CreateElement()
        {
            var currentTimeScale = EditorPrefs.GetFloat(PREFS_KEY, 1.0f);
            Time.timeScale = currentTimeScale;

            var slider = new MainToolbarSlider(new MainToolbarContent("Time", Tooltip), currentTimeScale, MIN_TIME_SCALE, MAX_TIME_SCALE,
                val =>
                {
                    Time.timeScale = val;
                    EditorPrefs.SetFloat(PREFS_KEY, val);
                });

            slider.populateContextMenu = menu =>
            {
                menu.AppendAction("Reset to 1.0", _ =>
                {
                    Time.timeScale = 1.0f;
                    EditorPrefs.SetFloat(PREFS_KEY, 1.0f);
                    RefreshUI();
                });
            };

            return slider;
        }
    }
}