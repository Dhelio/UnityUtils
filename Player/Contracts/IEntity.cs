using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Generic interface for entities (e.g. players, creatures, NPCs, etc.)
    /// </summary>
    public interface IEntity : IGameObjectSource {

        ulong Id { get; }

        void Lock(); //TODO maybe remove?
        void Unlock(); //TODO maybe remove?
        
    }

}
 