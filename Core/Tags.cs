using Castrimaris.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Utility component used to add multiple Tags to a <see cref="GameObject"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class Tags : MonoBehaviour, IEquatable<Tags>, IEquatable<string> {

        [Header("Parameters")]
        [SerializeField] private SerializableHashSet<string> values = new SerializableHashSet<string>() { "" };

        /// <summary>
        /// The tags of this object as array. Same as <see cref="Tags.GetTags(out string[])"/>.
        /// </summary>
        public string[] Array { get { return values.ToArray(); } }
        /// <summary>
        /// The tags of this object as list. Same as <see cref="Tags.GetTags(out List{string})"/>
        /// </summary>
        public List<string> List { get { return values.ToList(); } }

        /// <summary>
        /// Retrieves the tags array. Same as calling <see cref="Tags.Array"/>.
        /// </summary>
        /// <param name="Tags">The array of tags.</param>
        public void GetTags(out string[] Tags) {
            Tags = values.ToArray();
        }

        /// <summary>
        /// Retrieves the tags list. Same as calling <see cref="Tags.List"/>.
        /// </summary>
        /// <param name="Tags">The list of tags</param>
        public void GetTags(out List<string> Tags) {
            Tags = values.ToList();
        }

        /// <summary>
        /// Checks if a tag is contained in this object
        /// </summary>
        /// <param name="Tag">The tags to check</param>
        /// <returns>True if all tags are contained, False otherwise</returns>
        public bool Has(params string[] Tag) {
            foreach (string tag in Tag) {
                if (!values.Contains(tag))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if any of the given tags is in this object
        /// </summary>
        /// <param name="Tag">The tags to check.</param>
        /// <returns>True if any tag is contained, false otherwise</returns>
        public bool HasAny(params string[] Tag) {
            foreach (string tag in Tag) {
                if (values.Contains(tag))
                    return true;
            }
            return false;
        }

        public bool HasAny(List<string> tags) {
            foreach (string tag in tags) {
                if (this.values.Contains(tag))
                    return true;
            }
            return false;
        }

        public bool HasAny(Tags tags) {
            foreach (var tag in tags.values) {
                if (this.values.Contains(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the indicated tags are contained in this object's tags.
        /// </summary>
        /// <param name="Tags">A list of string tags</param>
        /// <returns>True if all tags are contained, False otherwise</returns>
        public bool Has(List<string> Tags) {
            return Has(values.ToArray());
        }

        /// <summary>
        /// Checks if the indicated tags are contained in this object's tags.
        /// </summary>
        /// <param name="Tags">A <see cref="Tags"/> component which contains the tags to check</param>
        /// <returns>True if all tags are contained, False otherwise</returns>
        public bool Has(Tags Tags) {
            return Has(Tags.Array);
        }

        #region Operators
        public bool Equals(string other) => Has(other);
        public bool Equals(Tags other) => Has(other);
        public static bool operator ==(Tags lhs, Tags rhs) => lhs.Equals(rhs);
        public static bool operator !=(Tags lhs, Tags rhs) => !lhs.Equals(rhs);
        #endregion
    }



}
