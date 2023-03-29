using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CastrimarisStudios.Editor;

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

        /// <summary>
        /// Returns the value of the enum as a string
        /// </summary>
        public static string GetStringValue(this Enum Value)
        {
            Type t = Value.GetType();

            FieldInfo fi = t.GetField(Value.ToString());

            StringValueAttribute[] attributes = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            return attributes.Length > 0 ? attributes[0].StringValue : null;
        }
    }
}
