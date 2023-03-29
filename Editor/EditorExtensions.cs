using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CastrimarisStudios.Editor.Extensions
{
    public static class EditorExtensions
    {
        /// <summary>
        /// Adds a button to this VisualElement, with a configurable behaviour
        /// </summary>
        public static VisualElement AddButton(this VisualElement localVisualElement, string buttonTitle, Action buttonBehaviour, Action buttonScheduledAction = null
            )
        {
            Button button = new Button();
            button.text = buttonTitle;
            button.clicked += buttonBehaviour;
            if (buttonScheduledAction != null)
            {
                button.schedule.Execute(buttonScheduledAction);
            }
            localVisualElement.Add(button);
            return localVisualElement;
        }

        /// <summary>
        /// Adds a multilinefield to this VisualElement
        /// </summary>
        public static VisualElement AddMultilineField(this VisualElement visualElement, string label)
        {
            TextField textField = new TextField(label, int.MaxValue, true, false, ' ');
            visualElement.Add(textField);
            return visualElement;
        }

        /// <summary>
        /// Adds a field followed by a button to this VisualElement, with a configurable behaviour
        /// </summary>
        public static VisualElement AddFieldWithButton(this VisualElement visualElement, string fieldLabel, string buttonName, Action buttonBehaviour, Action buttonScheduledAction = null)
        {
            TextField textField = new TextField(fieldLabel, int.MaxValue, false, false, ' ');
            textField.AddButton(buttonName, buttonBehaviour, buttonScheduledAction);
            visualElement.Add(textField);
            return visualElement;
        }
    }
}
