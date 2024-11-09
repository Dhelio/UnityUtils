using UnityEngine;

namespace Castrimaris.IO.Contracts {

    public interface IController {

        public Vector2 Movement { get; }
        public Vector2 Direction { get; }

    }
}