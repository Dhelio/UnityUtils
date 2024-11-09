using System;

namespace Castrimaris.Core {

    /// <summary>
    /// Attribute used in enumerators to tie string values to each enum value.
    /// </summary>
    public class StringValueAttribute : Attribute {
        public string Value { get; protected set; }

        public StringValueAttribute(string Value) { this.Value = Value; }
    }

}
