using System;

namespace CastrimarisStudios.Editor
{
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; protected set; }

        public StringValueAttribute(string Value) { this.StringValue = Value; }
    }
}