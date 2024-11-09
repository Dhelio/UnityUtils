using UnityEngine;

namespace Castrimaris.Layouts {
    public class GridCell {
        private Vector3 _center;
        private Vector3 _extents;
        private int _cellOrder;

        public Vector3 Center {
            get { return _center; }
            set { _center = value; }
        }

        public Vector3 Extents {
            get { return _extents; }
            set { _extents = value; }
        }

        public int CellOrder {
            get { return _cellOrder; }
            set { _cellOrder = value; }
        }

        public GridCell() {
            _center = Vector3.negativeInfinity;
            _extents = Vector3.zero;
            _cellOrder = -1;
        }

        public GridCell(Vector3 Center, Vector3 Extents, int Order) {
            _center = Center;
            _extents = Extents;
            _cellOrder = Order;
        }
    }
}