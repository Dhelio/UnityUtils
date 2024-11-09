using UnityEngine;

namespace Castrimaris.Core.Extensions {

    public static class SkinnedMeshRendererExtensions {

        /// <summary>
        /// Uses another <see cref="SkinnedMeshRenderer"/> to remap bones in this <see cref="SkinnedMeshRenderer"/>
        /// </summary>
        /// <param name="target">The <see cref="SkinnedMeshRenderer"/> whose bones to change</param>
        /// <param name="source">The <see cref="SkinnedMeshRenderer"/> whose bones to copy</param>
        public static void Remap(this SkinnedMeshRenderer target, SkinnedMeshRenderer source) {
            var rootBone = source.rootBone;
            var newBones = new Transform[target.bones.Length];
            for (int i = 0; i < target.bones.Length; i++) {
                foreach (var newBone in rootBone.GetComponentsInChildren<Transform>()) {
                    if (newBone.name == target.bones[i].name) {
                        newBones[i] = newBone;
                        continue;
                    }
                }
            }
            target.bones = newBones;
            target.rootBone = rootBone;
        }
    }
}