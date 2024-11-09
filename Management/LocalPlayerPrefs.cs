using UnityEngine;

namespace Castrimaris.Management {

    /// <summary>
    /// Simple extension of the PlayerPrefs class to use with the IPlayerPrefs interface.
    /// </summary>
    public class LocalPlayerPrefs : PlayerPrefs, IPlayerPrefs {

        int IPlayerPrefs.GetInt(string Key, int DefaultValue) {
            return GetInt(Key, DefaultValue);
        }
        void IPlayerPrefs.SetInt(string Key, int Value) {
            SetInt(Key, Value);
        }
        float IPlayerPrefs.GetFloat(string Key, float DefaultValue) {
            return GetFloat(Key, DefaultValue);
        }
        void IPlayerPrefs.SetFloat(string Key, float Value) {
            SetFloat(Key, Value);
        }
        string IPlayerPrefs.GetString(string Key, string DefaultValue) {
            return GetString(Key, DefaultValue);
        }
        void IPlayerPrefs.SetString(string Key, string Value) {
            SetString(Key, Value);
        }

        void IPlayerPrefs.Save() {
            Save();
        } 

    }

}