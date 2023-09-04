using System.Reflection;
using System;

namespace CastrimarisStudios.Extensions {
  
    public static class AttributesExtensions {

        /// <summary>
        /// Returns the value of the enum as a string. The enum fields must be tagged with StringValue attribute.
        /// </summary>
        public static string AsString(this Enum Value) {
            Type type = Value.GetType();

            FieldInfo fieldInfo = type.GetField(Value.ToString());

            StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            return attributes.Length > 0 ? attributes[0].Value : null;
        }

    }

}
