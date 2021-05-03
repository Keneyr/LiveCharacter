using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ps2D
{

    /// <summary>
    /// Loads a document from a .json file created by the Photoshop plugin.
    /// </summary>
    public class Loader
    {
        /// <summary>
        /// The document's dictionary before parsing.
        /// </summary>
        private Dictionary<string, object> _documentDictionary;

        /// <summary>
        /// The input string that contains the json.
        /// </summary>
        public string input { get; set; }

        /// <summary>
        /// Was there an error parsing?
        /// </summary>
        public bool errorParsing { get; set; }

        /// <summary>
        /// The error message (if any).
        /// </summary>
        public string errorMessage { get; set; }

        /// <summary>
        /// The document we're loading.
        /// </summary>
        public LayerMap document { get; set; }

        /// <summary>
        /// The indentation level of the layers.
        /// </summary>
        private int _indentLevel = 0;

        /// <summary>
        /// Load the document.
        /// </summary>
        public bool Load()
        {
            errorParsing = false;

            VerifyInput();
            if (errorParsing) return false;

            ParseDocumentIntoDictionary();
            if (errorParsing) return false;

            ReadDocumentProperties();
            if (errorParsing) return false;

            // fill up the layers in order of hierarchy
            document.allLayers = new List<Layer>();
            document.allLayers.Add(document.root);

            CollectTheseLayers(document.root, document.allLayers, "0");

            return true;
        }

        /// <summary>
        /// Is the input sane?
        /// </summary>
        void VerifyInput()
        {
            if (input == null)
            {
                errorParsing = true;
                errorMessage = "Input file not assigned.";
            }
            else if (input == null || input == "")
            {
                errorParsing = true;
                errorMessage = "Input file is empty.";
            }
        }

        /// <summary>
        /// Recursively fill up the list of layers in order.
        /// </summary>
        /// <param name="layer">The parent layer.</param>
        /// <param name="masterList">The list to put them into</param>
        void CollectTheseLayers(Layer layer, List<Layer> masterList, string hierarchyString)
        {
            // safety first
            if (layer == null || layer.layers == null) return;

            // with each kid
            for ( int i = 0; i < layer.layers.Count; i++)
            {
                Layer kid = layer.layers[i];

                // add them to the list
                masterList.Add(kid);

                // update the layer order
                kid.order = masterList.Count;

                // update the hierarchy string lol
                kid.hierarchyString = hierarchyString + "." + kid.photoshopLayerId.ToString();

                // and ask their children to do the same
                CollectTheseLayers(kid, masterList, kid.hierarchyString);
            }
        }

        /// <summary>
        /// Parse the document into
        /// </summary>
        void ParseDocumentIntoDictionary()
        {
            try
            {
                _documentDictionary = Ps2D.MiniJSON.Json.Deserialize(input) as Dictionary<string, object>;
                document = new LayerMap();

                // create a "root" element that looks like a photoshop layer.
                document.root = new Layer();
                document.root.photoshopLayerId = -1;
                document.root.photoshopLayerName = "root";
                document.root.indentLevel = -1;
                document.root.isVisible = true;
                document.root.bounds = new PixelBounds();
                document.root.hierarchyString = "";
                document.root.order = -1;
            }
            catch
            {
                errorParsing = true;
                errorMessage = "Invalid file format.";
            }
        }

        /// <summary>
        /// Read in the document properties.
        /// </summary>
        void ReadDocumentProperties()
        {
            // get the version
            document.version = _documentDictionary["pluginVersion"] as String;

            // get the dimensions
            document.bounds = ReadBoundsFromDictionary("bounds", _documentDictionary);

            // fill it with awesome.
            document.root.layers = ReadLayersFromDictionary("sprites", _documentDictionary);
        }

        /// <summary>
        /// Read a layer from dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary containing the goods.</param>
        /// <returns></returns>
        Layer ReadLayerFromDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null) return null;

            Layer result = new Layer();
            // read the properties
            result.photoshopLayerId = ReadIntFromDictionary("id", dictionary, 0);
            result.photoshopLayerName = ReadStringFromDictionary("name", dictionary, null);
            result.bounds = ReadBoundsFromDictionary("bounds", dictionary);
            result.isVisible = ReadBoolFromDictionary("visible", dictionary, true);
            result.indentLevel = _indentLevel;

            // read the children layers
            result.layers = ReadLayersFromDictionary("sprites", dictionary);

            return result;
        }

        /// <summary>
        /// Reads a list of layers from a dictionary entry.
        /// </summary>
        /// <param name="key">The key to fish from</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>A list of layers or not.</returns>
        List<Layer> ReadLayersFromDictionary(string key, Dictionary<string, object> dictionary)
        {
            // sanity
            if (key == null || dictionary == null) return null;

            // no key?
            if (!dictionary.ContainsKey(key)) return null;

            // empty value?
            if (dictionary[key] == null) return null;

            try
            {

                // grab children dictionaries
                List<object> spritesDictionaries = (List<object>)dictionary[key];

                _indentLevel++;
                List<Layer> layers = new List<Layer>();
                foreach (object o in spritesDictionaries)
                {
                    Dictionary<string, object> spriteDictionary = o as Dictionary<string, object>;
                    Layer layer = ReadLayerFromDictionary(spriteDictionary);
                    layers.Add(layer);
                }
                _indentLevel--;

                return layers;
            }
            catch
            {
                // don't care (actually i do, but this code doesn't)
            }
            return null;
        }

        /// <summary>
        /// Reads some bounds from a dictionary.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="dictionary">The dictionary to read from.</param>
        /// <returns>Bounds or bust.</returns>
        static PixelBounds ReadBoundsFromDictionary(string key, Dictionary<string, object> dictionary)
        {
            // sanity
            if (dictionary == null || key == null) return null;

            // missing key
            if (!dictionary.ContainsKey(key)) return null;

            // grab the sub dictionary with the bounds info
            PixelBounds bounds = null;
            try
            {
                Dictionary<string, object> subDictionary = dictionary[key] as Dictionary<string, object>;
                bounds = new PixelBounds();
                bounds.x = ReadIntFromDictionary("left", subDictionary, 0);
                bounds.y = ReadIntFromDictionary("top", subDictionary, 0);
                bounds.width = ReadIntFromDictionary("right", subDictionary, 0) - bounds.x;
                bounds.height = ReadIntFromDictionary("bottom", subDictionary, 0) - bounds.y;
            }
            catch
            {
                // shrug
            }
            return bounds;
        }

        /// <summary>
        /// Reads an integer key from a dictionary with a default value.
        /// </summary>
        /// <param name="key">The key containing the integer.</param>
        /// <param name="dictionary">The dictionary to pull from.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>The integer</returns>
        static int ReadIntFromDictionary(string key, Dictionary<string, object> dictionary, int defaultValue)
        {
            int result = defaultValue;
            try
            {
                result = Convert.ToInt32(dictionary[key]);
            }
            catch
            {
                // oh well.
            }
            return result;
        }

        /// <summary>
        /// Reads a string key from a dictionary with a default value.
        /// </summary>
        /// <param name="key">The key containing the string.</param>
        /// <param name="dictionary">The dictionary to look in.</param>
        /// <param name="defaultValue">The default value to use if we can't find it.</param>
        /// <returns>A string hopefully.</returns>
        static string ReadStringFromDictionary(string key, Dictionary<string, object> dictionary, string defaultValue)
        {
            string result = defaultValue;
            try
            {
                result = Convert.ToString(dictionary[key]);
            }
            catch
            {
                // damn.
            }
            return result;
        }

        /// <summary>
        /// Reads a bool key from a dictionary with a default value.
        /// </summary>
        /// <param name="key">The key in the dictionary.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>true or false.</returns>
        static bool ReadBoolFromDictionary(string key, Dictionary<string, object> dictionary, bool defaultValue)
        {
            bool result = defaultValue;
            try
            {
                result = Convert.ToBoolean(dictionary[key]);
            }
            catch
            {
                // the end.
            }
            return result;
        }


    }

}
