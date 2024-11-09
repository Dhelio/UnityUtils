using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// Attribute that exposes data for specific interfaces tied to GameObjects
    /// </summary>
    public class RequireInterfaceAttribute : PropertyAttribute {

        private readonly System.Type interfaceType;

        public System.Type Type { get { return interfaceType; } }

        public RequireInterfaceAttribute(System.Type InterfaceType)
        {
            this.interfaceType = InterfaceType;
        }

    }

}