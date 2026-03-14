namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using UnityEngine.UIElements;
    using Modules.Shared.Helpers;


    public abstract class BaseDropdownElement : BaseToolbarElement
    {
        protected override void OnApplyCustomStyle(VisualElement rootOverlay)
        {
            var textElement = rootOverlay.Q<TextElement>("EditorToolbarButtonText");

            if (textElement != null)
            {
                textElement.style.width = Width;
                textElement.style.minWidth = Width;

                textElement.style.overflow = Overflow.Hidden;
                textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
                textElement.style.textOverflow = TextOverflow.Ellipsis;
                textElement.style.whiteSpace = WhiteSpace.NoWrap;
            }
        }


        protected MainToolbarDropdown CreateEnumDropdown<TEnum>(TEnum currentValue, Action<TEnum> onSelected) where TEnum : struct, Enum
        {
            var content = new MainToolbarContent(Enum<TEnum>.GetName(currentValue), Tooltip);

            return new MainToolbarDropdown(content, rect =>
            {
                var menu = new GenericMenu();

                foreach (var type in Enum<TEnum>.GetValues())
                {
                    menu.AddItem(new GUIContent(Enum<TEnum>.GetName(type)), Enum<TEnum>.EqualityComparer.Equals(currentValue, type), () =>
                    {
                        onSelected?.Invoke(type);
                        UpdateContentText(Enum<TEnum>.GetName(type));
                    });
                }

                menu.DropDown(rect);
            });
        }
    }
}