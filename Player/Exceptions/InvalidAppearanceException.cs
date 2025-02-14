using System.Text;

using System;

namespace Castrimaris.Player.Exceptions {

    /// <summary>
    /// Exception thrown when something with the loading of player skins went wrong
    /// </summary>
    public class InvalidAppearanceException : SystemException {

        /// <summary>
        /// Exception thrown when something with the loading of player skins went wrong
        /// </summary>
        public InvalidAppearanceException(string appearanceCategory, string appearanceValue) : base(Format(appearanceCategory, appearanceValue)) { }

        private static string Format(string appearanceCategory, string appearanceValue) => $"Invalid appearance for category {appearanceCategory} with  value {appearanceValue}!";
    }
}