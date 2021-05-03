using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{

    /// <summary>
    /// Assigns the appropriate sprites to the layers discovered in the PSD.
    /// </summary>
    public class SpriteAssigner
    {
        /// <summary>
        /// Assign the sprites from the folder to existing sprite layers.
        /// </summary>
        public static void AssignSpritesFromFolder(Layout layout)
        {
            layout.ResetSpriteLayers();
            string folder = AssetDatabase.GetAssetPath(layout.imageSourceAssetFolder);

            List<string> assetPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
            assetPaths.RemoveAll(each => !each.StartsWith(folder));

            // with each document layer
            foreach (Layer layer in layout.document.allLayers)
            {
                UnitySpriteLayer spriteLayer = layout.FindSpriteLayer(layer);
                spriteLayer.ResetSprites();

                // with each matching asset path
                foreach (string assetPath in assetPaths)
                {
                    // extra the filename
                    string onlyFilename = Path.GetFileNameWithoutExtension(assetPath);

                    // and go through each guess at the layer's sprite name
                    foreach (string guess in layer.GetGuessesForSpriteName())
                    {
                        // match?
                        if (onlyFilename == guess)
                        {
                            spriteLayer.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite));
                            spriteLayer.name = onlyFilename;
                            spriteLayer.bounds = spriteLayer.sprite.bounds;
                            spriteLayer.spriteLayerStyle = SpriteLayerStyle.UnitySprite;
                            break;
                        }
                    }
                }
            }

            // reset the labels
            layout.ResetHierarchyLabels();

        }

        /// <summary>
        /// Assign the sprites from a spritesheet.
        /// </summary>
        public static void AssignSpritesFromSpritesheet(Layout layout)
        {
            layout.ResetSpriteLayers();
            string spritesheetPath = AssetDatabase.GetAssetPath(layout.spritesheetTexture);

            Object[] allSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritesheetPath);

            // with each layer in the document
            foreach (Layer layer in layout.document.allLayers)
            {
                UnitySpriteLayer spriteLayer = layout.FindSpriteLayer(layer);
                spriteLayer.ResetSprites();

                // with each sprite inside the spritesheet
                foreach (Sprite sprite in allSprites)
                {
                    // with each guess of the layer's name
                    foreach (string guess in layer.GetGuessesForSpriteName())
                    {
                        // did we match the sprite in the spritesheet?
                        if (sprite.name == guess)
                        {
                            spriteLayer.sprite = sprite;
                            spriteLayer.name = sprite.name;
                            spriteLayer.bounds = sprite.bounds;
                            spriteLayer.spriteLayerStyle = SpriteLayerStyle.UnitySprite;
                            break;
                        }
                    }
                }
            }
            layout.ResetHierarchyLabels();
        }

#if PS2D_TK2D
        /// <summary>
        /// Assign the sprites from a 2D Toolkit Collection.
        /// </summary>
        public static void AssignSpritesFromTk2dCollection(Layout layout, tk2dSpriteCollectionData spriteCollection)
        {

            layout.ResetSpriteLayers();

            // did we select anything?
            if (spriteCollection != null)
            {

                List<tk2dSpriteDefinition> spriteDefinitions = new List<tk2dSpriteDefinition>(spriteCollection.spriteDefinitions);
                // go through our layers
                foreach (Layer layer in layout.document.allLayers)
                {
                    UnitySpriteLayer spriteLayer = layout.FindSpriteLayer(layer);
                    spriteLayer.ResetSprites();

                    // go through the guesses for the current layer
                    foreach (string guess in layer.GetGuessesForSpriteName())
                    {
                        // find the sprite definition with this name...
                        // HEADS UP:  Use the sprite definitions list, because the GetSpriteDefinition() call
                        // uses a cache that's stale when you click Commit from the editor.
                        tk2dSpriteDefinition spriteDef = spriteDefinitions.Find(each => each.name == guess);

                        // did we get it? (null && empty names... gah, this hung me up for a few hours)
                        if (spriteDef != null && spriteDef.name != "")
                        {
                            // hook it up
                            spriteLayer.tk2dSpriteName = guess;
                            spriteLayer.spriteLayerStyle = SpriteLayerStyle.Tk2dSprite;
                            spriteLayer.bounds = spriteDef.GetUntrimmedBounds();
                            spriteLayer.name = guess;
                            break;
                        }
                    }
                }

            }

            layout.ResetHierarchyLabels();
        }
#endif


    }

}