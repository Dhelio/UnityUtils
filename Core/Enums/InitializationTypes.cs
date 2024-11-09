namespace Castrimaris.Attributes {

    /// <summary>
    /// Helper enum to decide when some service or class should be initialized.
    /// </summary>
    public enum InitializationTypes {

        /// <summary>
        /// Initialization of this service/class should happen on Awake method.
        /// </summary>
        OnAwake = 0,

        /// <summary>
        /// Initialization of this service/class should happen on Start method.
        /// </summary>
        OnStart = 1,

        /// <summary>
        /// Initialization of this service/class should happen with an external method call or delayed call.
        /// </summary>
        OnDemand = 2
    }

}