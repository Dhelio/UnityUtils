using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.UI.Contracts {

    /// <summary>
    /// Contract for images that also have a text component
    /// </summary>
    public interface ISubtitledImage : IGameObjectSource, IText, IImage {
    }

}