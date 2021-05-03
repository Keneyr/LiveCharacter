using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{

    /// <summary>
    /// Watch for map files and changes.
    /// </summary>
    public class MapWatcher : AssetPostprocessor
    {
        /// <summary>
        /// Fires when the map list has changed.
        /// </summary>
        public static event System.Action mapListChanged;

        /// <summary>
        /// The list of available maps.
        /// </summary>
        public static List<string> availableMaps = new List<string>();

        /// <summary>
        /// Unity fires this when Assets have changed in the editor.
        ///
        /// Use this opportunity to maintain a list of map files available.
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            int removed = RemoveThese(deletedAssets, movedFromAssetPaths);
            int added = AddThese(importedAssets, movedAssets);
            SortMaps();

            // fire the event the maps changed and anyone cares
            if (removed + added > 0 && mapListChanged != null)
            {
                mapListChanged();
            }
        }

        /// <summary>
        /// Sort the maps.
        /// </summary>
        static void SortMaps()
        {
            availableMaps.Sort(delegate(string a, string b) { return a.CompareTo(b); });
        }

        /// <summary>
        /// Remove these assets from our list.
        /// </summary>
        /// <param name="deletedAssets">The fallen</param>
        /// <param name="movedFromAssetPaths">The displaced</param>
        /// <returns>The number of changed maps.</returns>
        static int RemoveThese(string[] deletedAssets, string[] movedFromAssetPaths)
        {
            int changeCount = 0;

            // sanity
            if (deletedAssets == null && movedFromAssetPaths == null) return changeCount;

            // create a list to hold everything
            List<string> removables = new List<string>();

            // add the lists
            if (deletedAssets != null) removables.AddRange(deletedAssets);
            if (movedFromAssetPaths != null) removables.AddRange(movedFromAssetPaths);

            // clean up each file
            foreach (string s in removables)
            {
                if (IsMapFile(s))
                {
                    availableMaps.Remove(s);
                    changeCount++;
                }
            }

            return changeCount;
        }

        /// <summary>
        /// Add these to our list
        /// </summary>
        /// <param name="importedAssets">The fresh</param>
        /// <param name="movedAssets">The relocated</param>
        /// <returns>The number of changed maps</returns>
        static int AddThese(string[] importedAssets, string[] movedAssets)
        {
            int changeCount = 0;

            // sanity
            if (importedAssets == null && movedAssets == null) return changeCount;

            // create a list to hold everything
            List<string> addables = new List<string>();

            // add the lists
            if (importedAssets != null) addables.AddRange(importedAssets);
            if (movedAssets != null) addables.AddRange(movedAssets);

            // add our items
            foreach (string s in addables)
            {

                if (IsMapFile(s))
                {
                    if (availableMaps.IndexOf(s) < 0)
                    {
                        availableMaps.Add(s);
                    }
                    changeCount++;
                }
            }

            return changeCount;
        }

        /// <summary>
        /// Is this asset path a map file?
        /// </summary>
        /// <param name="assetPath">The asset path.</param>
        /// <returns>Yup or Nope</returns>
        static bool IsMapFile(string assetPath)
        {
            if (assetPath == null) return false;
            return assetPath.EndsWith(Backpack.MapExtension + ".json");
        }

        /// <summary>
        /// Manually refresh our list of maps.  Useful for start up to get the list of maps.
        /// </summary>
        public static void Refresh()
        {
            string[] everyPath = AssetDatabase.GetAllAssetPaths();
            availableMaps.Clear();
            foreach (string path in everyPath)
            {
                if (IsMapFile(path))
                {
                    availableMaps.Add(path);
                }
            }
            SortMaps();
        }

    }

}
