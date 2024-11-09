using Castrimaris.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// An implementation of the <see cref="HashSet{T}"/> struct that is serializable by Unity's Inspector.
    /// </summary>
    [System.Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver {

#if UNITY_EDITOR
        [Header("Debug")]
        [Tooltip("Editor-Only debug fields. Since the Serialization is implemented through a List, we're doing some weird custom checks on list vs hashset counts to be able to insert multiple values in Editor.")]
        [ReadOnly, SerializeField] private int listCount = -1;
        [Tooltip("Editor-Only debug fields. Since the Serialization is implemented through a List, we're doing some weird custom checks on list vs hashset counts to be able to insert multiple values in Editor.")]
        [ReadOnly, SerializeField] private int hashCount = -1;
#endif

        [Header("Values")]
        [SerializeField] private List<T> Values = new List<T>();

        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            hashCount = Count;
            listCount = Values.Count;
#endif

            //If there aren't the same count of elements between the list and the hashset, just update the list.
            if (Count < Values.Count - 1) {
                Values.Clear();
                Values.AddRange(this);
            }
        }

        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            hashCount = Count;
            listCount = Values.Count;
#endif

            //Save the values in the HashSet
            Clear();
            foreach (var element in Values) {
                Add(element);
            }

            
        }
    }
}