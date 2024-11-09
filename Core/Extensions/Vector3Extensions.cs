using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for <see cref="Vector3"/>
    /// </summary>
    public static class Vector3Extensions {

        public static Vector3 Remap(this Vector3 target, AxisTypes X = AxisTypes.X, AxisTypes Y = AxisTypes.Y, AxisTypes Z = AxisTypes.Z) {
            var tmp = target;
            switch (X) {
                case AxisTypes.NegX:
                case AxisTypes.X:
                    target.x = tmp.x;
                    break;
                case AxisTypes.NegY:
                case AxisTypes.Y:
                    target.x = tmp.y;
                    break;
                case AxisTypes.NegZ:
                case AxisTypes.Z:
                    target.x = tmp.z;
                    break;
                default:
                    break;
            }

            switch (Y) {
                case AxisTypes.NegX:
                case AxisTypes.X:
                    target.y = tmp.x;
                    break;
                case AxisTypes.NegY:
                case AxisTypes.Y:
                    target.y = tmp.y;
                    break;
                case AxisTypes.NegZ:
                case AxisTypes.Z:
                    target.y = tmp.z;
                    break;
                default:
                    break;
            }

            switch (Z) {
                case AxisTypes.NegX:
                case AxisTypes.X:
                    target.z = tmp.x;
                    break;
                case AxisTypes.NegY:
                case AxisTypes.Y:
                    target.z = tmp.y;
                    break;
                case AxisTypes.NegZ:
                case AxisTypes.Z:
                    target.z = tmp.z;
                    break;
                default:
                    break;
            }

            return target;
        }

    }
}