namespace CustomToolbar.Editor.ToolbarElements
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;


    internal sealed class ToolbarLayerVisibility : BaseDropdownElement
    {
        public const string ID = "CustomToolbar/LayerVisibility";

        public static ToolbarLayerVisibility Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Layer Visibility";
        protected override string Tooltip => "Controls which layers are visible in the Scene View.";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            Texture2D icon = EditorGUIUtility.IconContent("d_SceneLayersToggle").image as Texture2D;
            var content = new MainToolbarContent("Layers", icon, Tooltip);

            return new MainToolbarDropdown(content, rect => BuildLayerMenu().DropDown(rect));
        }

        private static GenericMenu BuildLayerMenu()
        {
            var menu = new GenericMenu();
            int currentMask = Tools.visibleLayers;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            bool hasIsolateItems = false;

            for (int i = 0; i < 32; i++)
            {
                string layerName = i <= 7 ? LayerMask.LayerToName(i) :
                    (i < layersProp.arraySize ? layersProp.GetArrayElementAtIndex(i).stringValue : "");

                if (!string.IsNullOrEmpty(layerName))
                {
                    int layerIndex = i;
                    bool isVisible = (currentMask & (1 << i)) != 0;
                    menu.AddItem(new GUIContent($"{layerName} (Layer {i})"), isVisible, () => Tools.visibleLayers ^= (1 << layerIndex));
                    hasIsolateItems = true;
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No layers found"));
                return menu;
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Show All"), false, () => Tools.visibleLayers = ~0);
            menu.AddItem(new GUIContent("Hide All"), false, () => Tools.visibleLayers = 0);
            menu.AddItem(new GUIContent("Invert Selection"), false, () => Tools.visibleLayers = ~Tools.visibleLayers);
            menu.AddSeparator("");

            if (!hasIsolateItems)
            {
                menu.AddDisabledItem(new GUIContent("Isolate/No layers available"));
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    string layerName = i <= 7 ? LayerMask.LayerToName(i) :
                        (i < layersProp.arraySize ? layersProp.GetArrayElementAtIndex(i).stringValue : "");

                    if (!string.IsNullOrEmpty(layerName))
                    {
                        int layerIndex = i;
                        menu.AddItem(new GUIContent($"Isolate/{layerName} (Layer {i})"), false, () => Tools.visibleLayers = (1 << layerIndex));
                    }
                }
            }

            return menu;
        }
    }
}