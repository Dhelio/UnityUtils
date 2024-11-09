using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// An implementation of the <see cref="Dictionary{TKey, TValue}"/> struct that is serializable by Unity's Inspector.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [Serializable]
        public struct KeyValue {
            public TKey key;
            public TValue value;

            public static implicit operator KeyValue(KeyValuePair<TKey, TValue> pair) {
                return new KeyValue() {
                    key = pair.Key,
                    value = pair.Value
                };
            }
        }

        public List<KeyValue> keyValues = new();
        public void OnBeforeSerialize() {

            if (keyValues.Count > Count)
                AddNewValue();

            else if (keyValues.Count < Count)
                UpdateSerializedValues();

        }

        void UpdateSerializedValues() {
            keyValues.Clear();
            foreach (var pair in this)
                keyValues.Add(pair);
        }

        void AddNewValue() {
            var current = keyValues[^1];
            TryAdd(current.key, current.value);
        }
        public void OnAfterDeserialize() {
            Clear();

            for (var i = 0; keyValues != null && i < keyValues.Count; i++) {
                var current = keyValues[i];
                TryAdd(current.key, current.value);
            }
        }
    }

}