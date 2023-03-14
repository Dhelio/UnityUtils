using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CastrimarisStudios.Editor.Extensions
{
    public static class EditorExtensions
    {

        public static VisualElement AddButton(this VisualElement localVisualElement, string buttonTitle, Action buttonAction, Action buttonScheduledAction = null
            )
        {
            Button button = new Button();
            button.text = buttonTitle;
            button.clicked += buttonAction;
            if (buttonScheduledAction != null)
            {
                button.schedule.Execute(buttonScheduledAction);
            }
            localVisualElement.Add(button);
            return localVisualElement;
        }

        public static VisualElement AddMultilineField(this VisualElement visualElement, string label)
        {
            TextField textField = new TextField(label, int.MaxValue, true, false, ' ');
            visualElement.Add(textField);
            return visualElement;
        }

        public static VisualElement AddFieldWithButton(this VisualElement visualElement, string fieldLabel, string buttonName, Action buttonAction, Action buttonScheduledAction = null)
        {
            TextField textField = new TextField(fieldLabel, int.MaxValue, false, false, ' ');
            textField.AddButton(buttonName, buttonAction, buttonScheduledAction);
            visualElement.Add(textField);
            return visualElement;
        }
    }
}
