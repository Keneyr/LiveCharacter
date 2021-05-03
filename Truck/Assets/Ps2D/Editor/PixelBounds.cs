using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{

    /// <summary>
    /// Pixel-based boundaries from a 0,0 top-left origin.
    /// </summary>
    [System.Serializable]
    public class PixelBounds
    {
        /// <summary>
        /// Pixels from the left.
        /// </summary>
        public int x { get; set; }

        /// <summary>
        /// Pixels from the top.
        /// </summary>
        public int y { get; set; }

        /// <summary>
        /// Pixels wide.
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// Pixels high.
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// The string representation.
        /// </summary>
        /// <returns>A string version.</returns>
        public override string ToString()
        {
            return string.Format("x={0:d} y={1:d} w={2:d} h={3:d}", x, y, width, height);
        }

        /// <summary>
        /// The center of the graphic.
        /// </summary>
        /// <returns>Ye olde Vector2.</returns>
        public Vector2 GetCenter()
        {
            return new Vector2(halfWidth + x, halfHeight + y);
        }

        /// <summary>
        /// Half the width.
        /// </summary>
        public float halfWidth
        {
            get
            {
                return width * 0.5f;
            }
        }

        /// <summary>
        /// Half the height.
        /// </summary>
        public float halfHeight
        {
            get
            {
                return height * 0.5f;
            }
        }

    }

}
