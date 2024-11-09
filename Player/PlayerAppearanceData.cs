using Castrimaris.Core.Extensions;
using Castrimaris.Player.Contracts;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Data container for player's <see cref="SkinnedMeshRenderer"/>s
    /// </summary>
    public class PlayerAppearanceData : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private AppearanceCategories category;
        [SerializeField] private string value;

        public AppearanceCategories Category { get => category; set => category = value; }
        public string CategoryName => category.AsString();
        public string Value { get => value; set => this.value = value; }

        private void Reset() {
            //Just for utility, check if the name of this gameObject is more or less the same as any of the category, so we assign it right away
            var categories = Enum.GetValues(typeof(AppearanceCategories));
            int shortest = int.MaxValue;
            var result = AppearanceCategories.Jacket;
            foreach (var category in categories) {
                var tmp = (AppearanceCategories)category;
                var distance = tmp.AsString().LevenshteinDistance(this.gameObject.name);
                if (distance < shortest) {
                    shortest = distance;
                    result = tmp;
                }
            }
            this.category = result;
            value = this.name;
        }
    }

}