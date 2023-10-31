using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core {

    public class Tags : MonoBehaviour {
        [SerializeField] private string[] tags;

        public string[] Values { get { return tags; } }

        public void GetTags(out string[] Tags) {
            Tags = tags;
        }

        public void GetTags(out List<string> Tags) {
            Tags = tags.ToList();
        }

        public bool Has(string Tag) {
            return tags.Contains(Tag);
        }

        public bool Has(string[] Tags) {
            foreach (var Tag in Tags) {
                if (!Has(Tag))
                    return false;
            }
            return true;
        }

    }

}
