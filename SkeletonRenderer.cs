using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Castrimaris.Metaverse.Editor {
    public class SkeletonRenderer : BoneRenderer {

        

        [ContextMenu("Get Children as references")]
        private void GetChildrenAsReferences() {
            //transforms = GetComponentsInChildren<Transform>(); //TODO: this gives error
        }
    }
}