using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace Ps2D
{
    public enum SpriteLayerStyle
    {
        None        = 0,
        UnitySprite = 1,
        Tk2dSprite  = 2
    }

    /// <summary>
    /// A pre-importing layer.
    /// </summary>
    // [System.Serializable]
    public class UnitySpriteLayer : ScriptableObject
    {
        /// <summary>
        /// The Ps2D layer.
        /// </summary>
        public Layer layer;

        /// <summary>
        /// The sprite.
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// The bounds of the sprite.
        /// </summary>
        public Bounds bounds;

        /// <summary>
        /// The type of sprite this is.
        /// </summary>
        public SpriteLayerStyle spriteLayerStyle = SpriteLayerStyle.None;

#if PS2D_TK2D
        /// <summary>
        /// The 2D Toolkit sprite definition.
        /// </summary>
        public string tk2dSpriteName;

        /// <summary>
        /// The 2D Toolkit sprite collection.
        /// </summary>
        //public tk2dSpriteCollectionData spriteCollectionData
        //{
        //    get { return _spriteCollectionData; }
        //    set { _spriteCollectionData = value; }
        //}
#endif
        /// <summary>
        /// The sorting order to use.
        /// </summary>
        public int sortingOrder;

        /// <summary>
        /// The sorting layer name to use.
        /// </summary>
        public string sortingLayerName;

        /// <summary>
        /// Is this layer visible?
        /// </summary>
        public bool isVisible;

        /// <summary>
        /// The z order to use.
        /// </summary>
        public float z;

        /// <summary>
        /// Attach a Unity sprite.
        /// </summary>
        /// <param name="targetGameObject">The game object to attach to</param>
        public void AttachUnitySprite(GameObject targetGameObject)
        {
            // sanity.
            if (targetGameObject == null || sprite == null) return;

            // find or create the sprite renderer
            SpriteRenderer spriteRenderer = targetGameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = (SpriteRenderer)targetGameObject.AddComponent<SpriteRenderer>();
            }

            // set it
            spriteRenderer.sprite = sprite;

            // update sorting order and layer name
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.sortingLayerName = sortingLayerName;

            // whether it is visible.
            spriteRenderer.enabled = isVisible;
        }

#if PS2D_TK2D
        /// <summary>
        /// Let's attach a tk2d sprite.
        /// </summary>
        /// <param name="targetGameObject">The game object to attach to.</param>
        public void AttachTk2dSprite(GameObject targetGameObject, tk2dSpriteCollectionData spriteCollectionData)
        {
            // sanity.
            if (targetGameObject == null || tk2dSpriteName == null || spriteCollectionData == null) return;

            // triple check the sprite definitions... this is madness... this is SPARTA!
            List<tk2dSpriteDefinition> spriteDefinitions = new List<tk2dSpriteDefinition>(spriteCollectionData.spriteDefinitions);
            tk2dSpriteDefinition spriteDef = spriteDefinitions.Find(each => each.name == tk2dSpriteName);

            if (spriteDef == null || spriteDef.name == "")
            {
                // this condition should never happen because we're finding the definition by name...
                // if we do trip it up, then, at least don't die a horrible death.
                return;
            }

            // lookup the index
            int spriteIndex = spriteDefinitions.IndexOf(spriteDef);

            // create a tk2dsprite
            tk2dSprite spriteComponent = targetGameObject.AddComponent<tk2dSprite>() as tk2dSprite;

            // set the sprite based on the index because it's safer that way... :(
            spriteComponent.SetSprite(spriteCollectionData, spriteIndex);

            // update the sorting layer
            spriteComponent.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
            spriteComponent.SortingOrder = sortingOrder;
            spriteComponent.Build();

            // set visibility
            spriteComponent.GetComponent<Renderer>().enabled = isVisible;
        }
#endif

        /// <summary>
        /// Get the pixel position of the sprite.
        /// </summary>
        /// <returns>A Vector3.</returns>
        public Vector3 GetPixelPosition(float coordinatesScale, PixelBounds fromBounds)
        {
            if (!hasSprite)
            {
                return Vector3.zero;
            }

            // figure out the placement
            Vector2 layerCenter = layer.bounds.GetCenter();
            float x             = (layerCenter.x - fromBounds.x) * coordinatesScale;
            float y             = (fromBounds.height - layerCenter.y + fromBounds.y) * coordinatesScale;
            float z             = this.z;

            // adjust for pivots (literally a day to figure this out... i fail so hard)
            Vector3 spriteCenter  = bounds.center;
            Vector3 spriteExtents = bounds.extents;
            float pivotX          = spriteCenter.x / -spriteExtents.x * (layer.bounds.halfWidth * coordinatesScale);
            float pivotY          = spriteCenter.y / -spriteExtents.y * (layer.bounds.halfHeight * coordinatesScale);
            Vector2 pivotOffset   = new Vector2(pivotX, pivotY);

            x += pivotOffset.x;
            y += pivotOffset.y;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Do we have a sprite?
        /// </summary>
        public bool hasSprite
        {
            get
            {
#if PS2D_TK2D
                if (spriteLayerStyle == SpriteLayerStyle.UnitySprite && sprite != null) return true;
                if (spriteLayerStyle == SpriteLayerStyle.Tk2dSprite && tk2dSpriteName != null) return true;
                return false;
#else
                return sprite != null;
#endif
            }
        }

        /// <summary>
        /// Do we have child layers?
        /// </summary>
        public bool hasChildren
        {
            get
            {
                return layer != null && layer.layers != null && layer.layers.Count > 0;
            }
        }

        /// <summary>
        /// When Unity enables this object.
        /// </summary>
        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// Reset the sprites associated with this sprite layer.
        /// </summary>
        public void ResetSprites()
        {
            spriteLayerStyle = SpriteLayerStyle.None;
            sprite           = null;
#if PS2D_TK2D
            tk2dSpriteName   = null;
#endif
        }

    }
}
