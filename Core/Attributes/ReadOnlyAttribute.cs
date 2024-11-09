using System;
using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// Attribute used to make a field or property not interactable in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ReadOnlyAttribute : PropertyAttribute {}

}