using System.Reflection;
using System;

namespace Castrimaris.Core.Extensions {
    /// <summary>
    /// Extension methods for Attributes
    /// </summary>
    public static class AttributesExtensions {

        /// <summary>
        /// Returns the value of the enum as a string. The enum fields must be tagged with <see cref="StringValueAttribute"/>.
        /// </summary>
        public static string AsString(this Enum value) {
            Type type = value.GetType();

            FieldInfo fieldInfo = type.GetField(value.ToString());

            StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            return attributes.Length > 0 ? attributes[0].Value : null;
        }
    }
}