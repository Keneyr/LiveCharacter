// Inspired by:
// http://answers.unity3d.com/questions/604703/sortlayer-renderer-extension.html

using UnityEngine;
using UnityEditorInternal;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


namespace Ps2D
{
    /// <summary>
    /// Unity sorting layer utility.
    /// </summary>
    /// <remarks>
    /// UT hasn't made the sorting layers available as a public API just yet, so
    /// this provides a way to go fishing.
    /// </remarks>
    public static class SortingLayers 
    {

        /// <summary>
        /// Gets a list of sorting layer names.
        /// </summary>
        /// <returns>The sorting layer names.</returns>
        public static string[] GetNames()
        {
            // grab the type for the mysterious InternalEditorUtility
            Type internalEditorUtilityType = typeof(InternalEditorUtility);

            // snag the sortingLayers property info
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            
            // grab the value, and cast as a bunch of strings.
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        /// <summary>
        /// Gets the index of the default sorting layer.
        /// </summary>
        /// <remarks>
        /// This may not be multi-lingual.  No clue.
        /// </remarks>
        /// <returns>The index of the default sorting layer.</returns>
        public static int IndexOfDefaultSortingLayer()
        {
            string[] names = GetNames();
            List<String> namesList = new List<string>(names);
            return namesList.IndexOf("Default");
        }

    
    }

}
