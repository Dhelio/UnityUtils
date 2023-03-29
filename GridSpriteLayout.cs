using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DhlWorks.Logging;

namespace CastrimarisStudios.Sprites
{
    /// <summary>
    /// Takes all sprites renderers that are child of this GameObject and orders them in a grid.
    /// </summary>
    public class GridSpriteLayout : MonoBehaviour
    {
        #region Constants
        private const string TAG = nameof(GridSpriteLayout);
        #endregion

        #region Enums
        public enum OrderType { BY_COLUMN, BY_ROW, BEST_FIT }
        #endregion

        #region Private Variables
        private Vector3 topLeftPosition = Vector3.negativeInfinity;
        private Vector3 topRightPosition = Vector3.negativeInfinity;
        private Vector3 bottomLeftPosition = Vector3.negativeInfinity;
        private Vector3 bottomRightPosition = Vector3.negativeInfinity;

        private int lastCheckedChildCount = 0;

        [Header("Parameters")]
        [SerializeField] private OrderType orderType = OrderType.BY_COLUMN;
        [SerializeField] private int ConstraintCount = 3;
        [SerializeField] private Vector2 boundingBox = Vector2.one;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private Vector2 spacing = Vector2.zero;
        #endregion

        #region Public Variables
        #endregion

        #region Private Methods
        private void InitializeBounds()
        {
            try
            {
                SpriteRenderer localSpriteRenderer = GetComponent<SpriteRenderer>();

                if (localSpriteRenderer != null)
                {
                    topLeftPosition = localSpriteRenderer.bounds.min;
                    bottomRightPosition = localSpriteRenderer.bounds.max;
                    //TODO?
                }
                else
                {
                    SpriteMask localSpriteMask = GetComponent<SpriteMask>();
                    if (localSpriteMask != null)
                    {
                        Log.D(TAG, $"Local Sprite Mask bounds {localSpriteMask.bounds}");
                        topLeftPosition = localSpriteMask.bounds.min;
                        bottomRightPosition = localSpriteMask.bounds.max;
                        //TODO
                    }
                    else
                    {
                        topLeftPosition = this.gameObject.transform.position + (Vector3)offset;
                        topRightPosition = topLeftPosition + this.gameObject.transform.right * boundingBox.x;
                        bottomLeftPosition = topLeftPosition - this.gameObject.transform.up * boundingBox.y;
                        bottomRightPosition = topLeftPosition + this.gameObject.transform.right * boundingBox.x - this.gameObject.transform.up * boundingBox.y;
                    }
                }
            }
            catch (MissingComponentException mce)
            {
                //TODO
            }
        }

        private void OrderSprites()
        {
            switch (orderType)
            {
                case OrderType.BY_COLUMN:
                    OrderByColumn();
                    break;
                case OrderType.BY_ROW:
                    OrderByRow();
                    break;
                case OrderType.BEST_FIT:
                    OrderByBestFit();
                    break;
                default:
                    Log.E(TAG, "No such OrderType!");
                    break;
            }
        }

        private void OrderByColumn()
        {
            for (int row = 0, i = 0; row < this.gameObject.transform.childCount; row++)
            {
                for (int column = 0; column < ConstraintCount && i < this.gameObject.transform.childCount; column++, i++)
                {
                    GameObject spriteObj = this.gameObject.transform.GetChild(i).gameObject;
                    SpriteRenderer spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();

                    spriteObj.transform.position = topLeftPosition + new Vector3(
                        spriteRenderer.bounds.extents.x * (2 * column + 1) + (spacing.x * column),
                        -spriteRenderer.bounds.extents.y * row * 2 - (spacing.y * row),
                        spriteRenderer.bounds.extents.z
                        );

                }
            }
        }

        private void OrderByRow()
        {
            throw new System.NotImplementedException();
        }

        private void OrderByBestFit()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Public Methods
        public void Order()
        {
            InitializeBounds();
            OrderSprites();
        }
        #endregion

        #region Unity Overrides

        private void OnDrawGizmos()
        {
            InitializeBounds();

            Gizmos.color = Color.red;
            Gizmos.DrawLine(topLeftPosition, topRightPosition);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(topLeftPosition, bottomLeftPosition);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(topRightPosition, bottomRightPosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottomLeftPosition, bottomRightPosition);

            Order();
        }


        private void Update()
        {
            if (lastCheckedChildCount != this.gameObject.transform.childCount)
            {
                Order();
                lastCheckedChildCount = this.gameObject.transform.childCount;
            }
        }
        #endregion
    }
}