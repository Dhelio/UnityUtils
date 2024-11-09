namespace Castrimaris.Attributes {

    //Disabling types used by the ConditionalField property

    /// <summary>
    /// What kind of disabling effect should be applied to this property.
    /// </summary>
    public enum DisablingTypes {
        /// <summary>
        /// The field can be seen, but not modified.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// The field is hidden from the Editor.
        /// </summary>
        Hidden
    }

}