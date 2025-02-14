using Castrimaris.Core.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.ScriptableObjects {

    public class DynamicLightmapsContainer : ScriptableObject {

        [Header("Parameters")]
        public SerializableDictionary<int, (int, Vector4)> Data = new SerializableDictionary<int, (int, Vector4)>();
    }

}
