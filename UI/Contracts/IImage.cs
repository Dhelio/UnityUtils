using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.UI.Contracts {

    public interface IImage : IGameObjectSource {
        public Texture2D Image { get; set; }
        public void SetImage(Texture2D image);
        public Texture2D GetImage();
    }

}