using UnityEngine;

namespace Castrimaris.Core {
    public interface IDelayedInitializer {
        public void InitializeGameObject(GameObject gameObject);
    }
}
