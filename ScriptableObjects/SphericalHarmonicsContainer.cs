using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Castrimaris.ScriptableObjects {

    [CreateAssetMenu(fileName = "Spherical Harmonics", menuName = "Castrimaris/ScriptableObjects/Spherical Harmonics")]
    public class SphericalHarmonicsContainer : ScriptableObject {

        [Header("Parameters")]
        [SerializeField] private SphericalHarmonicsL2[] harmonics;

        public SphericalHarmonicsL2[] Harmonics { get { return harmonics; } set { harmonics = value; } }
    }

}
