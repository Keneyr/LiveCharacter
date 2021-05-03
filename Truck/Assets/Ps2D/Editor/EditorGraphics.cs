using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace Ps2D
{

    /// <summary>
    /// The graphics to power the editor.
    /// </summary>
    [System.Serializable]
    public class EditorGraphics
    {
        /// <summary>
        /// The path to the graphics.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Whether or not the graphics have been loaded.
        /// </summary>
        public bool isLoaded { get; set; }

        /// <summary>
        /// The title graphic for the editor window.
        /// </summary>
        public Texture2D editorTitle { get; set; }

        /// <summary>
        /// A folder icon.
        /// </summary>
        public Texture2D spriteLayerFolder { get; set; }

        /// <summary>
        /// A dead leaf.
        /// </summary>
        public Texture2D spriteLayerDeadLeaf { get; set; }

        /// <summary>
        /// A sprite icon.
        /// </summary>
        public Texture2D spriteLayerSprite { get; set; }

        /// <summary>
        /// A visible icon.
        /// </summary>
        public Texture2D spriteLayerVisible { get; set; }

        /// <summary>
        /// An invisible icon.
        /// </summary>
        public Texture2D spriteLayerInvisible { get; set; }

        /// <summary>
        /// A checkmark icon.
        /// </summary>
        public Texture2D checkmark { get; set; }

        /// <summary>
        /// A lightning bolt.
        /// </summary>
        public Texture2D usain { get; set; }

        /// <summary>
        /// Choose a map.
        /// </summary>
        public Texture2D chooseMap { get; set; }

        /// <summary>
        /// A folder icon.
        /// </summary>
        public Texture2D refresh { get; set; }

        /// <summary>
        /// The quintessential cog.
        /// </summary>
        public Texture2D cog { get; set; }

        /// <summary>
        /// Load the graphics.
        /// </summary>
        public void Load()
        {
            if (isLoaded) return;

            // TODO: Write some guessing code when the user changes the folder
            path = "Assets/Ps2D/Editor/Graphics";

            // load each of the textures
            editorTitle = LoadTexture("ps2d-inspector-title");
            spriteLayerSprite = LoadTexture("ps2d-sprite-layer-sprite");
            spriteLayerFolder = LoadTexture("ps2d-sprite-layer-folder");
            spriteLayerDeadLeaf = LoadTexture("ps2d-sprite-layer-deadleaf");
            spriteLayerVisible = LoadTexture("ps2d-sprite-layer-visible");
            spriteLayerInvisible = LoadTexture("ps2d-sprite-layer-invisible");
            refresh = LoadTexture("ps2d-refresh");
            usain = LoadTexture("ps2d-usain");
            checkmark = LoadTexture("ps2d-checkmark");
            chooseMap = LoadTexture("ps2d-choose-map");
            cog = LoadTexture("ps2d-cog");

            // hey, we've been loaded!
            isLoaded = true;
        }

        /// <summary>
        /// Load a texture if you can.
        /// </summary>
        /// <param name="partialPath">The name of the file with out the directory and extension.</param>
        /// <returns>A Texture2D or nullsauce.</returns>
        Texture2D LoadTexture(string partialPath)
        {
            if (partialPath == null) return null;
            if (path == null) return null;

            string full = string.Format("{0}/{1}.psd", path, partialPath);

            try
            {
                return AssetDatabase.LoadAssetAtPath(full, typeof(Texture2D)) as Texture2D;
            }
            catch
            {
                return null;
            }
        }


    }
}

