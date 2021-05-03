using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ps2D
{
    /// <summary>
    /// Where textures can live.
    /// </summary>
    public enum TextureSource
    {
        None = 0
      , AssetFolder = 1
      , Spritesheet = 2
#if PS2D_TK2D
      , Tk2dSpriteCollection = 3
#endif
    }

    /// <summary>
    /// The type of collider to use.
    /// </summary>
    public enum ColliderStyle
    {
        None = 0,
        BoxCollider2D,
        BoxCollider
    }

    /// <summary>
    /// A layout creates your sprites, yo!
    /// </summary>
    [System.Serializable]
    public class Layout : ScriptableObject
    {
        /// <summary>
        /// Should we show the depth options?
        /// </summary>
        public bool foldoutDepth = false;

        /// <summary>
        /// Should we show the nesting options?
        /// </summary>
        public bool foldoutNesting = false;

        /// <summary>
        /// Should we show the replication options?
        /// </summary>
        public bool foldoutPrefabs = false;

        /// <summary>
        /// Should we show the coordinates scale?
        /// </summary>
        public bool foldoutScale = false;

        /// <summary>
        /// Should we show the filtering.
        /// </summary>
        public bool foldoutFiltering = false;

        /// <summary>
        /// Should we show the collider options?
        /// </summary>
        public bool foldoutPhysics = false;

        /// <summary>
        /// Show we show the position options?
        /// </summary>
        public bool foldoutPosition = false;

        /// <summary>
        /// The text asset for the layout document.
        /// </summary>
        public TextAsset layoutDocumentAsset;

        /// <summary>
        /// The asset for the folder to read the loose images from.
        /// </summary>
        public Object imageSourceAssetFolder;

        /// <summary>
        /// The sprite sheet to use.
        /// </summary>
        public Texture spritesheetTexture;

        /// <summary>
        /// Where the image should come from.
        /// </summary>
        public TextureSource imageSource;

        /// <summary>
        /// The document.
        /// </summary>
        public LayerMap document;

#if PS2D_TK2D
        /// <summary>
        /// The sprite collection.
        /// </summary>
        //public tk2dSpriteCollectionData spriteCollectionData;

        /// <summary>
        /// The build key of the currently selection sprite collection.
        /// </summary>
        public int currentSpriteCollectionBuildKey = 0;

#endif
        /// <summary>
        /// The folder to load the assets from.
        /// </summary>
        public string assetFolder;

        /// <summary>
        /// The sorting layer name to use.
        /// </summary>
        public string sortingLayerName;

        /// <summary>
        /// The sorting order to start from.
        /// </summary>
        public int sortingOrder = 0;

        /// <summary>
        /// The amount to increase each sort order.
        /// </summary>
        public int sortingOrderStep = 1;

        /// <summary>
        /// The z offset for each sprite.
        /// </summary>
        public float zOffset = 0f;

        /// <summary>
        /// The pixels to units ratio.
        /// </summary>
        public int pixelsToUnits = 100;

        /// <summary>
        /// How to scale the coordinates from Photoshop.
        /// </summary>
        public float coordinatesScale = 1f;

        /// <summary>
        /// Should we apply a root collider?
        /// </summary>
        public ColliderStyle rootCollider = ColliderStyle.None;

        /// <summary>
        /// Is the root collider a trigger?
        /// </summary>
        public bool rootColliderTrigger = false;

        /// <summary>
        /// The amount to scale each sprite.
        /// </summary>
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Which layer to start importing from.
        /// </summary>
        public Layer startingLayer;

        /// <summary>
        /// Should we use Photoshop to create our layers?
        /// </summary>
        public bool preservePhotoshopLayers = false;

        /// <summary>
        /// Should we add an extra parent to the sprite?
        /// </summary>
        public bool addExtraParentToSprite = false;

        /// <summary>
        /// How should we position this?
        /// </summary>
        public TextAnchor anchor = TextAnchor.MiddleCenter;

        /// <summary>
        /// Trim away whitespace down to the sprite for aligning.
        /// </summary>
        public bool trimToSprites = true;

        /// <summary>
        /// The hierarchy labels used for display.
        /// </summary>
        public List<string> hierarchyLabels;

        /// <summary>
        /// The trimmed sprite bounds based on the user's starting sprite..
        /// </summary>
        public PixelBounds trimmedSpriteBounds;

        /// <summary>
        /// The map selected.
        /// </summary>
        public string selectedMapAssetPath;

        /// <summary>
        /// The scroll position for the options.
        /// </summary>
        public Vector2 scrollPositionForOptions = Vector2.zero;

        /// <summary>
        /// The game object to replicate.
        /// </summary>
        public GameObject replicator;

        /// <summary>
        /// The editor graphics.
        /// </summary>
        public EditorGraphics editorGraphics;

        /// <summary>
        /// The sprite layers.
        /// </summary>
        public List<UnitySpriteLayer> spriteLayers;

        /// <summary>
        /// Called by Unity when this object gets enabled.
        /// </summary>
        public void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;

            if (spriteLayers == null)
            {
                spriteLayers = new List<UnitySpriteLayer>();
            }

            if (hierarchyLabels == null)
            {
                hierarchyLabels = new List<string>();
            }

            // TODO: hard coded graphics?  really?
            // what if the user moves this?  How do other people do this?
            // I see a lot of people don't... but some people do... ask in the forums about this.
            if (editorGraphics == null)
            {
                editorGraphics = new EditorGraphics();
                editorGraphics.Load();
            }
        }


        /// <summary>
        /// Gets a friendly name for the document (minus the extension)
        /// </summary>
        /// <returns>The friendly name.</returns>
        public string GetFriendlyDocumentName()
        {
            if (layoutDocumentAsset == null) return null;
            string assetPathToDocument = AssetDatabase.GetAssetPath(layoutDocumentAsset);
            string documentName = Path.GetFileNameWithoutExtension(assetPathToDocument).Replace(Backpack.MapExtension, "");
            return documentName;
        }


        /// <summary>
        /// Get the sprite layer that holds this layer.
        /// </summary>
        /// <param name="layer">The layer</param>
        /// <returns>The sprite layer or null</returns>
        public UnitySpriteLayer FindSpriteLayer(Layer layer)
        {
            return spriteLayers.Find(each => each.layer != null && each.layer.photoshopLayerId == layer.photoshopLayerId);
        }

        /// <summary>
        /// Load the document.
        /// </summary>
        public void Load()
        {
            ResetLayout();

            if (layoutDocumentAsset == null) return;

            // load!
            Loader _loader = new Loader();
            _loader.input = layoutDocumentAsset.text;
            _loader.Load();

            // assign the document
            if (!_loader.errorParsing)
            {
                document = _loader.document;

                CreateSpriteLayers();
                ResetHierarchyLabels();
            }

        }

        /// <summary>
        /// Reset anything that may be loaded or cached.
        /// </summary>
        public void ResetLayout()
        {
            startingLayer = null;
            document = null;
            //if (spriteLayers != null)
            {
                spriteLayers.Clear();
            }
        }

        /// <summary>
        /// Create the sprite layers for each layer in the document.
        /// </summary>
        void CreateSpriteLayers()
        {
            // safety first
            if (document == null ||
                document.allLayers == null
                ) return;

            // with each photoshop layer
            foreach (Layer layer in document.allLayers)
            {
                // create a sprite layer
                UnitySpriteLayer spriteLayer = ScriptableObject.CreateInstance<UnitySpriteLayer>();

                // that holds these details
                spriteLayer.spriteLayerStyle = SpriteLayerStyle.None;
                spriteLayer.layer = layer;
                spriteLayer.name = layer.photoshopLayerName;
                spriteLayer.isVisible = layer.isVisible;

                // add to our list
                spriteLayers.Add(spriteLayer);

            }

            // put them in the correct order
            spriteLayers.Sort(delegate(UnitySpriteLayer a, UnitySpriteLayer b)
            {
                return a.layer.order.CompareTo(b.layer.order);
            });

        }

        /// <summary>
        /// Create the labels used for the hierarchy.
        /// </summary>
        public void ResetHierarchyLabels()
        {
            // wipe em out
            hierarchyLabels.Clear();

            // safety jet
            if (document == null || document.allLayers == null) return;

            foreach (Layer layer in document.allLayers)
            {

                StringBuilder label = new StringBuilder();

                // build some indents
                for (int i = 1; i < layer.indentLevel; i++)
                {
                    label.Append("   ");
                }
                if (layer.indentLevel > 1)
                {
                    label.Append("> ");
                }

                // add the best layer name
                label.Append(GetBestName(layer));

                // add this the label
                hierarchyLabels.Add(label.ToString());
            }

            // change the root node to something special
            hierarchyLabels[0] = "-- Everything --";

        }

        /// <summary>
        /// Get the best name for this layer.  It's either the name of the layer,
        /// or the name of the sprite if we've got one assigned.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>The best name.</returns>
        public string GetBestName(Layer layer)
        {
            // lookup the sprite layer
            UnitySpriteLayer spriteLayer = FindSpriteLayer(layer);

            // set the best name
            string bestName = (spriteLayer != null && spriteLayer.hasSprite) ? spriteLayer.name : layer.photoshopLayerName;

            // replace our POWER .. ower .. ower .. TOKEN .. token .. token
            bestName = bestName.Replace("#ps2d", "");

            return bestName.Trim();

        }


        /// <summary>
        /// Reset any references to existing sprites.
        /// </summary>
        public void ResetSpriteLayers()
        {
            foreach (UnitySpriteLayer spriteLayer in spriteLayers)
            {
                spriteLayer.sprite = null;
#if PS2D_TK2D
                spriteLayer.tk2dSpriteName = null;
#endif
            }
        }

        /// <summary>
        /// Refresh the sprite layer properties that come
        /// from the layout.
        /// </summary>
        public void UpdateSpriteLayerProperties()
        {
            UpdateSpriteLayerZOffsets();
            UpdateSpriteLayerSortingNames();
            UpdateSpriteLayerSortOrder();
            UpdateTk2dSpriteCollections();
        }

        /// <summary>
        /// Assign the sorting layer name to each of the sprite layers.
        /// </summary>
        void UpdateSpriteLayerSortingNames()
        {
            foreach (UnitySpriteLayer spriteLayer in spriteLayers)
            {
                spriteLayer.sortingLayerName = sortingLayerName;
            }
        }

        /// <summary>
        /// Update the zoffset for each of the sprite layers.
        /// </summary>
        void UpdateSpriteLayerZOffsets()
        {
            float currentZ = 0f;

            foreach (UnitySpriteLayer spriteLayer in GetSelectedSpriteLayers())
            {
                spriteLayer.z = currentZ;
                currentZ += zOffset;
            }
        }

        /// <summary>
        /// Update the Sprite Collection references.
        /// </summary>
        void UpdateTk2dSpriteCollections()
        {
#if PS2D_TK2D
            //if (imageSource != TextureSource.Tk2dSpriteCollection) return;
            //foreach (UnitySpriteLayer spriteLayer in GetSelectedSpriteLayers())
            //{
            //    spriteLayer.spriteCollectionData = spriteCollectionData;
            //}
#endif
        }

        /// <summary>
        /// The list of sprite layers with assigned sprites.
        /// </summary>
        /// <returns></returns>
        public List<UnitySpriteLayer> GetSelectedSpriteLayers()
        {
            var validSprites = spriteLayers.FindAll(each => each.hasSprite);
            if (startingLayer != null)
            {
                validSprites.RemoveAll(each => !each.layer.hierarchyString.StartsWith(startingLayer.hierarchyString));
            }
            return validSprites;
        }

        /// <summary>
        /// Assign the sorting order.
        /// </summary>
        void UpdateSpriteLayerSortOrder()
        {
            List<UnitySpriteLayer> spriteLayersWithSprites = GetSelectedSpriteLayers();

            int currentOrder = (spriteLayersWithSprites.Count - 1) * sortingOrderStep + sortingOrder;

            foreach (UnitySpriteLayer spriteLayer in spriteLayersWithSprites)
            {
                spriteLayer.sortingOrder = currentOrder;
                currentOrder = currentOrder - sortingOrderStep;
            }
        }

        /// <summary>
        /// The minimum bounds needed to hold the visible sprites.
        /// </summary>
        /// <returns>The bounds.</returns>
        public PixelBounds GetTrimmedSpriteBounds()
        {
            PixelBounds bounds = new PixelBounds();
            bounds.x = int.MaxValue;
            bounds.y = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;


            // grab the list of visible sprites
            List<UnitySpriteLayer> spriteLayers = GetSelectedSpriteLayers();
            foreach (UnitySpriteLayer spriteLayer in spriteLayers)
            {
                PixelBounds layerBounds = spriteLayer.layer.bounds;
                bounds.x = System.Math.Min(bounds.x, layerBounds.x);
                bounds.y = System.Math.Min(bounds.y, layerBounds.y);
                maxX = System.Math.Max(maxX, layerBounds.x + layerBounds.width);
                maxY = System.Math.Max(maxY, layerBounds.y + layerBounds.height);
            }

            bounds.width = maxX - bounds.x;
            bounds.height = maxY - bounds.y;

            return bounds;
        }


        /// <summary>
        /// Reset the values of the properties tied to the non-selected image source.
        /// </summary>
        public void UpdateImageSource()
        {
            switch (imageSource)
            {
                case TextureSource.None:
                    imageSourceAssetFolder = null;
                    spritesheetTexture = null;
#if PS2D_TK2D
                    //spriteCollectionData = null;
                    currentSpriteCollectionBuildKey = int.MinValue;
#endif
                    break;
                case TextureSource.AssetFolder:
                    spritesheetTexture = null;
#if PS2D_TK2D
                    //spriteCollectionData = null;
                    currentSpriteCollectionBuildKey = int.MinValue;
#endif
                    break;

                case TextureSource.Spritesheet:
                    imageSourceAssetFolder = null;
#if PS2D_TK2D
                    //spriteCollectionData = null;
                    currentSpriteCollectionBuildKey = int.MinValue;
#endif
                    break;

#if PS2D_TK2D
                case TextureSource.Tk2dSpriteCollection:
                    imageSourceAssetFolder = null;
                    spritesheetTexture = null;
                    break;
#endif

                default:
                    break;
            }

        }

        /// <summary>
        /// Is there a valid image source choosen?
        /// </summary>
        /// <returns>yes or no.</returns>
        public bool hasValidImageSource()
        {
            if (spriteLayers == null || spriteLayers.Count == 0) return false;
            switch (imageSource)
            {
                case TextureSource.None:
                    return false;

                case TextureSource.AssetFolder:
                    return imageSourceAssetFolder != null;

                case TextureSource.Spritesheet:
                    return spritesheetTexture != null;

#if PS2D_TK2D
                case TextureSource.Tk2dSpriteCollection:
                    return true;
                    //return spriteCollectionData != null;
#endif
                default:
                    return false;
            }

        }
    }

}
