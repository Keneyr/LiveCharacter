using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{
    /// <summary>
    /// Creates the sprites.
    /// </summary>
    public class SpriteCreator
    {

        /// <summary>
        /// The Layout.
        /// </summary>
        public Layout layout { get; set; }

#if PS2D_TK2D
        /// <summary>
        /// The 2D Toolkit Sprite Collection to draw from.
        /// </summary>
        public tk2dSpriteCollectionData spriteCollectionData;
#endif

        /// <summary>
        /// Creates the sprites.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="startingLayer">The starting layer.</param>
        public static void CreateSprites(Layout layout, Layer startingLayer)
        {
            SpriteCreator creator = new SpriteCreator();
            creator.layout        = layout;
            creator.CreateSprites(startingLayer);
        }

#if PS2D_TK2D
        /// <summary>
        /// Create the sprites.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="startingLayer">The starting layer.</param>
        /// <param name="spriteCollectionData">The sprite collection data.</param>
        public static void CreateSprites(Layout layout, Layer startingLayer, tk2dSpriteCollectionData spriteCollectionData)
        {
            SpriteCreator creator        = new SpriteCreator();
            creator.layout               = layout;
            creator.spriteCollectionData = spriteCollectionData;
            creator.CreateSprites(startingLayer);
        }
#endif

        /// <summary>
        /// Create the sprites.
        /// </summary>
        public void CreateSprites(Layer startingLayer)
        {
            // line up the ducks
            layout.UpdateSpriteLayerProperties();

            // create the root object to attach all these sprites
            string assetPathToDocument = AssetDatabase.GetAssetPath(layout.layoutDocumentAsset);
            string rootName            = Path.GetFileNameWithoutExtension(assetPathToDocument).Replace(Backpack.MapExtension, "");

            // do we need to trim the sprite bounds?
            if (layout.trimToSprites)
            {
                // calculate the new bounds and cache it.
                layout.trimmedSpriteBounds = layout.GetTrimmedSpriteBounds();
            }

            // are we starting from a specific node?
            if (startingLayer != null)
            {
                rootName = layout.GetBestName(startingLayer);
            }

            GameObject root;

            // recreate the nested photoshop layers?
            if (layout.preservePhotoshopLayers)
            {
                Layer fromHere = startingLayer;
                if (fromHere == null)
                {
                    fromHere = layout.document.root;
                }
                root = CreateGameObjectHierarchyForLayer(fromHere, null);
                if (startingLayer == null && root != null)
                {
                    root.name = rootName;
                }
            }
            else
            {
                List<UnitySpriteLayer> spriteLayers = layout.GetSelectedSpriteLayers();

                // if we're just doing 1 sprite, make that sprite become the root
                if (spriteLayers.Count == 1)
                {
                    root = CreateGameObjectForSpriteLayer(spriteLayers[0], null);
                }
                else
                {
                    root = new GameObject(rootName);
                    // nah, just use flat sprites
                    foreach (UnitySpriteLayer spriteLayer in spriteLayers)
                    {
                        CreateGameObjectForSpriteLayer(spriteLayer, root.transform);
                    }
                }

            }


            // if we didn't create anything, jet.
            if (root == null) return;

            // should we apply a collider to the root?
            ColliderCreator.Create(layout, root);

            // scale it if the user wants
            root.transform.localScale = layout.scale;

            Undo.RegisterCreatedObjectUndo(root, Backpack.Name + " GameObject");

            // if we have a selection, tuck it under that
            if (Selection.activeTransform)
            {
                root.transform.parent = Selection.activeTransform;
            }


            // notify the user
            EditorGUIUtility.PingObject(root);
        }


        /// <summary>
        /// Create the nested layers.
        /// </summary>
        /// <param name="layer">The layer to start from.</param>
        /// <param name="parentTransform">Where to attach.</param>
        /// <returns>The game object</returns>
        public GameObject CreateGameObjectHierarchyForLayer(Layer layer, Transform parentTransform)
        {
            UnitySpriteLayer spriteLayer = layout.FindSpriteLayer(layer);

            // sanity
            if (spriteLayer == null) return null;

            // don't worry about this layer ... not a sprite, no kids.
            if (!spriteLayer.hasSprite && !spriteLayer.hasChildren) return null;

            GameObject layerGameObject;
            if (spriteLayer.hasSprite)
            {
                layerGameObject = CreateGameObjectForSpriteLayer(spriteLayer, parentTransform);
            }
            else
            {
                string layerName = layout.GetBestName(layer);
                layerGameObject = new GameObject(layerName);
                layerGameObject.transform.parent = parentTransform;
            }

            if (layer.layers != null)
            {
                foreach (Layer childLayer in layer.layers)
                {
                    CreateGameObjectHierarchyForLayer(childLayer, layerGameObject.transform);
                }
            }

            return layerGameObject;
        }


        /// <summary>
        /// Create a GameObject for this sprite layer, attached to this parent.
        /// </summary>
        /// <param name="parentTransform">The parent to attach to</param>
        /// <param name="spriteLayer">The sprite layer.</param>
        public GameObject CreateGameObjectForSpriteLayer(UnitySpriteLayer spriteLayer, Transform parentTransform)
        {

            // spawn up the game object
            GameObject goSprite;
            if (layout.replicator != null)
            {
                goSprite = GameObject.Instantiate(layout.replicator, Vector3.zero, Quaternion.identity) as GameObject;
            }
            else
            {
                goSprite = new GameObject();
            }
            goSprite.name = spriteLayer.name;

            // attach the appropriate sprite
            // do we have a sprite to set?
            if (spriteLayer.hasSprite)
            {
#if PS2D_TK2D
                if (spriteLayer.sprite != null)
                {
                    spriteLayer.AttachUnitySprite(goSprite);
                }
                else if (spriteLayer.tk2dSpriteName != null)
                {
                    spriteLayer.AttachTk2dSprite(goSprite, spriteCollectionData);
                }
#else
                spriteLayer.AttachUnitySprite(goSprite);
#endif
            }


            // where should this sprite be?
            PixelBounds bounds = layout.trimToSprites ? layout.trimmedSpriteBounds : layout.document.bounds;

            Vector3 position = spriteLayer.GetPixelPosition(layout.coordinatesScale, bounds);

            float x = position.x;
            float y = position.y;
            float z = position.z;

            // layout the sprite based on the anchor
            Vector2 offset = new Vector2(bounds.width, bounds.height);

            switch (layout.anchor)
            {
                case TextAnchor.LowerCenter:
                    offset.x = offset.x * 0.5f;
                    offset.y = offset.y * 0f;
                    break;
                case TextAnchor.LowerLeft:
                    offset.x = offset.x * 0f;
                    offset.y = offset.y * 0f;
                    break;
                case TextAnchor.LowerRight:
                    offset.x = offset.x * 1f;
                    offset.y = offset.y * 0f;
                    break;
                case TextAnchor.MiddleCenter:
                    offset.x = offset.x * 0.5f;
                    offset.y = offset.y * 0.5f;
                    break;
                case TextAnchor.MiddleLeft:
                    offset.x = offset.x * 0f;
                    offset.y = offset.y * 0.5f;
                    break;
                case TextAnchor.MiddleRight:
                    offset.x = offset.x * 1f;
                    offset.y = offset.y * 0.5f;
                    break;
                case TextAnchor.UpperCenter:
                    offset.x = offset.x * 0.5f;
                    offset.y = offset.y * 1f;
                    break;
                case TextAnchor.UpperLeft:
                    offset.x = offset.x * 0f;
                    offset.y = offset.y * 1f;
                    break;
                case TextAnchor.UpperRight:
                    offset.x = offset.x * 1f;
                    offset.y = offset.y * 1f;
                    break;
                default:
                    break;
            }

            // let's move it!
            x -= offset.x * layout.coordinatesScale;
            y -= offset.y * layout.coordinatesScale;

            // the pixels to units ratio
            x *= 1f / layout.pixelsToUnits;
            y *= 1f / layout.pixelsToUnits;

            // should we create an extra parent for the sprite?
            if (layout.addExtraParentToSprite)
            {
                GameObject goSpriteParent         = new GameObject(goSprite.name);
                goSpriteParent.transform.position = new Vector3(x, y, z);
                goSpriteParent.transform.parent   = parentTransform;
                goSprite.transform.parent         = goSpriteParent.transform;
                goSprite.transform.localPosition  = Vector3.zero;
            }
            else
            {
                goSprite.transform.position = new Vector3(x, y, z);
                goSprite.transform.parent = parentTransform;
            }

            return goSprite;
        }



    }


}
