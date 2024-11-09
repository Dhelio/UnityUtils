using System;

namespace Castrimaris.Attributes {

    /// <summary>
    /// Attribute used to expose a method or property in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ExposeInInspector : Attribute {}

}