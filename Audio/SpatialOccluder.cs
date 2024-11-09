using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Audio {

    /// <summary>
    /// Simple implementation of an occluder geometry for reducing sounds coming from a <see cref="SpatialEmitter"/> towards a <see cref="SpatialListener"/>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SpatialOccluder : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField, Range(0f, 100f)] private float occlusionPercentage = 100f;

        public float OcclusionPercentage => occlusionPercentage;

    }
}