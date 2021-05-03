using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{

    /// <summary>
    /// Try to guess the image source format.
    /// </summary>
    public class FormatGuesser
    {

        /// <summary>
        /// The layout to try to guess from.
        /// </summary>
        public Layout layout { get; set; }

#if PS2D_TK2D
        public tk2dSpriteCollectionData spriteCollectionData;
#endif


        /// <summary>
        /// Attempt to auto detect the format.
        /// </summary>
        /// <returns></returns>
        public TextureSource Guess()
        {
            if (layout == null) return TextureSource.None;

#if PS2D_TK2D
            if (AttemptTk2dCollection()) return TextureSource.Tk2dSpriteCollection;
#endif
            if (AttemptSpritesheet()) return TextureSource.Spritesheet;
            if (AttemptAssetFolder()) return TextureSource.AssetFolder;

            return TextureSource.None;
        }

        /// <summary>
        /// Is this an asset folder?
        /// </summary>
        /// <returns>Yes or no.</returns>
        bool AttemptAssetFolder()
        {
            string assetPathToDocument = AssetDatabase.GetAssetPath(layout.layoutDocumentAsset);
            string documentFolder = Path.GetDirectoryName(assetPathToDocument);
            string documentName = Path.GetFileNameWithoutExtension(assetPathToDocument).Replace(Backpack.MapExtension, "");

            List<string> guesses = new List<string>();

            // just the name of the file (as a folder)
            guesses.Add(documentFolder + Path.DirectorySeparatorChar.ToString() + documentName);

            // try what Generator calls the folder.
            guesses.Add(documentFolder + Path.DirectorySeparatorChar.ToString() + documentName + "-assets");

            // go through our guesses
            foreach (string guess in guesses)
            {
                // if the directory exists?
                if (Directory.Exists(guess))
                {
                    // try loading the asset
                    Object guessAsset = AssetDatabase.LoadAssetAtPath(guess, typeof(Object));

                    // did that work?
                    if (guessAsset != null)
                    {
                        // we found our folder! (we hope)
                        layout.imageSource = TextureSource.AssetFolder;
                        layout.imageSourceAssetFolder = guessAsset;
                        return true;
                    }
                }
            }


            return false;
        }

        /// <summary>
        /// Is this a spritesheet?
        /// </summary>
        /// <returns>yes or no.</returns>
        bool AttemptSpritesheet()
        {
            string assetPathToDocument = AssetDatabase.GetAssetPath(layout.layoutDocumentAsset);
            string documentFolder = Path.GetDirectoryName(assetPathToDocument);
            string documentName = Path.GetFileNameWithoutExtension(assetPathToDocument).Replace(Backpack.MapExtension, "");
            List<string> guesses = new List<string>();

            // check for the same named file in the same directory
            guesses.Add(documentFolder + "/" + documentName + ".png");
            guesses.Add(documentFolder + "/" + documentName + ".psd");
            guesses.Add(documentFolder + "/" + documentName + ".jpg");

            // with each guess.
            foreach (string guess in guesses)
            {
                object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(guess);
                if (assetsAtPath == null)
                {
                    continue;
                }

                // we should have at least 2 sprites to be a spritesheet.
                // we exclude 2 or less, because unity counts the texture as 1 and the sprite as 1.
                if (assetsAtPath.Length <= 2)
                {
                    continue;
                }

                layout.imageSource = TextureSource.Spritesheet;
                layout.spritesheetTexture = AssetDatabase.LoadAssetAtPath(guess, typeof(Texture)) as Texture;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is this a 2dToolkit collection?
        /// </summary>
        /// <returns>Yes or no</returns>
        bool AttemptTk2dCollection()
        {
#if PS2D_TK2D
            tk2dSpriteCollectionIndex[] collectionIndexes = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
            if (collectionIndexes == null || collectionIndexes.Length == 0) return false;

            List<tk2dSpriteCollectionIndex> collections = new List<tk2dSpriteCollectionIndex>(collectionIndexes);

            // look for identical names
            string basename = layout.GetFriendlyDocumentName();
            tk2dSpriteCollectionIndex foundIndex = collections.Find(each => each.name.Equals(basename, System.StringComparison.CurrentCultureIgnoreCase));
            if (foundIndex == null) return false;

            // found it ... load it
            GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(foundIndex.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
            if (go == null) return false;

            // grab the data from it
            tk2dSpriteCollectionData spriteCollectionData = go.GetComponent<tk2dSpriteCollectionData>();
            if (spriteCollectionData == null) return false;

            // all good.
            layout.imageSource = TextureSource.Tk2dSpriteCollection;
            this.spriteCollectionData = spriteCollectionData;

            return true;
#else
            return false;
#endif
        }


    }

}
