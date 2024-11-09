namespace Castrimaris.Core {
    /// <summary>
    /// General interface for data saving services
    /// </summary>
    public interface IPlayerPrefs {
        public int GetInt(string Key, int DefaultValue);
        public void SetInt(string Key, int Value);
        public float GetFloat(string Key, float DefaultValue);
        public void SetFloat(string Key, float Value);
        public string GetString(string Key, string DefaultValue);
        public void SetString(string Key, string Value);
        public void Save();
    }
}