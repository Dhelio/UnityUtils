using Castrimaris.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// General interface for teleporters
    /// </summary>
    public interface ITeleporter : IGameObjectSource {

        /// <summary>
        /// Teleports the <see cref="GameObject"/>.
        /// </summary>
        /// <param name="position">The position where to teleport.</param>
        /// <param name="rotation">The rotation where to teleport.</param>
        public void Teleport(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Teleports the <see cref="GameObject"/>
        /// </summary>
        /// <param name="position">The position where to teleport.</param>
        public void Teleport(Transform position);

        /// <summary>
        /// Teleports the <see cref="GameObject"/> even if it shouldn't be able to teleport.
        /// </summary>
        /// <param name="position">The position where to teleport.</param>
        public void ForceTeleport(Transform position);

        /// <summary>
        /// Teleports the <see cref="GameObject"/> even if it shouldn't be able to teleport.
        /// </summary>
        /// <param name="position">The position where to teleport.</param>
        /// <param name="rotation">The rotation where to teleport.</param>
        public void ForceTeleport(Vector3 position, Quaternion rotation);
    }

}
