namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using UnityEngine.UIElements;


    public abstract class BaseToolbarElement : IComparable<BaseToolbarElement>
    {
        public const string PACKAGE_ROOT_PATH = "Assets/Modules/Shared/Plugins/CustomPackages/CustomToolbar";

        public abstract string ElementId { get; }
        protected abstract string Name { get; }
        protected virtual string Tooltip => string.Empty;
        protected virtual float Width => -1f;

        protected MainToolbarElement RootElement { get; private set; }


        private void Init()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            OnInit();
        }

        public virtual void OnInit() { }
        public virtual void OnPlayModeStateChanged(PlayModeStateChange state) {}

        protected abstract MainToolbarElement CreateElement();

        public MainToolbarElement GetOrCreateElement()
        {
            Init();
            RootElement = CreateElement();
            TryApplyCustomStyle();
            return RootElement;
        }

        protected void RefreshUI()
        {
            MainToolbar.Refresh(ElementId);
            TryApplyCustomStyle();
        }

        protected void SetEnabled(bool isEnabled)
        {
            if (RootElement != null && RootElement.enabled != isEnabled)
            {
                RootElement.enabled = isEnabled;
                RefreshUI();
            }
        }

        protected void UpdateContentText(string newText)
        {
            if (RootElement != null)
            {
                RootElement.content = new MainToolbarContent(newText, Tooltip);
                RefreshUI();
            }
        }

        private void TryApplyCustomStyle()
        {
            if (Width <= 0)
                return;

            EditorApplication.delayCall += () =>
            {
                var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var wnd in windows)
                {
                    if (wnd.GetType().Name == "MainToolbarWindow")
                    {
                        var rootOverlay = wnd.rootVisualElement?.Q(ElementId);
                        if (rootOverlay != null)
                            OnApplyCustomStyle(rootOverlay);
                    }
                }
            };
        }

        protected virtual void OnApplyCustomStyle(VisualElement rootOverlay)
        {
            rootOverlay.style.width = Width;
            rootOverlay.style.minWidth = Width;
        }

        public int CompareTo(BaseToolbarElement other)
        {
            return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
        }
    }
}