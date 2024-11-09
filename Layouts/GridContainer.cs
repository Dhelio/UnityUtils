using System;
using UnityEngine;

namespace Castrimaris.Layouts {

    [Serializable]
    public class GridContainer {

        [SerializeField] private Bounds _bounds;

        public Vector3 Center {
            get {
                return _bounds.center;
            }
            set {
                _bounds.center = value;
            }
        }

        public Vector3 Extents {
            get {
                return _bounds.extents;
            }

            set {
                _bounds.extents = value;
            }
        }

        public float X {
            get { return _bounds.extents.x; }
        }

        public float Y {
            get { return _bounds.extents.y; }
        }

        public float Z {
            get { return _bounds.extents.z; }
        }

        public Vector3 Size {
            get {
                return _bounds.extents * 2;
            }
        }

        public Vector3 Origin {
            get {
                return _bounds.center + _bounds.extents;
            }
        }

        public Vector3 Min {
            get {
                return _bounds.min;
            }
        }

        public Vector3 Max {
            get {
                return _bounds.max;
            }
        }

    }

}