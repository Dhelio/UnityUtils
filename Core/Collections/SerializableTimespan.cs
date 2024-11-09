using System;
using UnityEngine;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// An implementation of the <see cref="TimeSpan"/> struct that is serializable by Unity's Inspector.
    /// </summary>
    [Serializable]
    public class SerializableTimeSpan {
        [HideInInspector] public TimeSpan TimeSpan;

        // These fields are used to serialize the TimeSpan
        public int Days;
        public int Hours;
        public int Minutes;
        public int Seconds;

        // Synchronize TimeSpanValue with the serialized fields
        public void Synchronize() {
            TimeSpan = new TimeSpan(Days, Hours, Minutes, Seconds);
        }
    }

}
