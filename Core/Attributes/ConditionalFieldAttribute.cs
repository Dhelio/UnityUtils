using Castrimaris.Core.Monitoring;
using System;
using UnityEngine;

namespace Castrimaris.Attributes {

    /// <summary>
    /// An attributes used to conditionally show properties and fields in the Inspector based on the values of other properties or fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ConditionalFieldAttribute : PropertyAttribute {

        /// <summary>
        /// The name of the property or field to check
        /// </summary>
        public string TargetPropertyName { get; private set; }
        /// <summary>
        /// The value of the property or field to check
        /// </summary>
        public object TargetPropertyValue { get; private set; }
        /// <summary>
        /// The type of hiding the property or field should apply if the <see cref="TargetPropertyValue"/> is not met.
        /// </summary>
        public DisablingTypes DisablingType { get; private set; }

        /// <summary>
        /// Shows this value in Editor ONLY if certain conditions are met.
        /// </summary>
        /// <param name="TargetPropertyName">The name of the property of this class to check.</param>
        /// <param name="TargetCompareValue">The value of the property of this class to check.</param>
        /// <param name="DisablingType">The type of property disabling to apply if the conditions are not met.</param>
        public ConditionalFieldAttribute(string TargetPropertyName, object TargetCompareValue, DisablingTypes DisablingType) {
            this.TargetPropertyName = TargetPropertyName;
            this.TargetPropertyValue = TargetCompareValue;
            this.DisablingType = DisablingType;
        }
    }
}
