using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Simple extension of the PlayerPrefs class to use with the IPlayerPrefs interface.
    /// Stores data only locally.
    /// </summary>
    public class LocalPlayerPrefs : PlayerPrefs, IPlayerPrefs {

        /// <inheritdoc cref="PlayerPrefs.GetInt(string, int)"/>
        int IPlayerPrefs.GetInt(string Key, int DefaultValue) => GetInt(Key, DefaultValue);
        /// <inheritdoc cref="PlayerPrefs.SetInt(string, int)"/>
        void IPlayerPrefs.SetInt(string Key, int Value) => SetInt(Key, Value);
        /// <inheritdoc cref="PlayerPrefs.GetFloat(string, float)"/>
        float IPlayerPrefs.GetFloat(string Key, float DefaultValue) => GetFloat(Key, DefaultValue);
        /// <inheritdoc cref="PlayerPrefs.SetFloat(string, float)"/>
        void IPlayerPrefs.SetFloat(string Key, float Value) => SetFloat(Key, Value);
        /// <inheritdoc cref="PlayerPrefs.GetString(string, string)"/>
        string IPlayerPrefs.GetString(string Key, string DefaultValue) => GetString(Key, DefaultValue);
        /// <inheritdoc cref="PlayerPrefs.SetString(string, string)"/>
        void IPlayerPrefs.SetString(string Key, string Value) => SetString(Key, Value);
        /// <inheritdoc cref="PlayerPrefs.Save()"/>
        void IPlayerPrefs.Save() => Save();

    }

}