using Castrimaris.Core;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// Generic interface for components that implement joystick functionalities for touchscreens.
    /// </summary>
    public interface ITouchJoystick : IGameObjectSource {

        public float Horizontal { get; }
        public float Vertical { get; }

    }

}