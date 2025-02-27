namespace Castrimaris.Player.Contracts {

    /// <summary>
    /// Contract for handling player appearance loading and saving.
    /// </summary>
    public interface IPlayerAppearance {
        public void Load();
        public void Save();
        public void Set(AppearanceCategories category, string value);
        public void Set(string category, string value);
    }
}