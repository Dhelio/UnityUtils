using Castrimaris.Core;

namespace Castrimaris.Player.Contracts {

    /// <summary>
    /// Categories for the various customizable parts of the player avatar
    /// </summary>
    public enum AppearanceCategories {
        [StringValue(nameof(Beard))] Beard = 0,
        [StringValue(nameof(Eyebrows))] Eyebrows,
        [StringValue(nameof(Eyelashes))] Eyelashes,
        [StringValue(nameof(Eyes))] Eyes,
        [StringValue(nameof(Hair))] Hair,
        [StringValue(nameof(Hands))] Hands,
        [StringValue(nameof(Head))] Head,
        [StringValue(nameof(Iris))] Iris,
        [StringValue(nameof(Jacket))] Jacket,
        [StringValue(nameof(Mouth))] Mouth,
        [StringValue(nameof(Nose))] Nose,
        [StringValue(nameof(Pants))] Pants,
        [StringValue(nameof(Shirt))] Shirt,
        [StringValue(nameof(Shoes))] Shoes
    }

}