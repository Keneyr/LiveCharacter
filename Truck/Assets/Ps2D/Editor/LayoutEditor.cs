using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#if PS2D_TK2D
using tk2dEditor;
using tk2dRuntime;
#endif

namespace Ps2D
{

    /// <summary>
    /// The main editor.
    /// </summary>
    public class LayoutEditor : EditorWindow
#if PS2D_TK2D
        , tk2dRuntime.ISpriteCollectionForceBuild
#endif
    {

        /// <summary>
        /// The layout we're editing.
        /// </summary>
        public Layout layout;

#if PS2D_TK2D
        /// <summary>
        /// The sprite collection to use.
        /// </summary>
        private tk2dSpriteCollectionData _spriteCollectionData;

        /// <summary>
        /// Should we force a refresh of the tk2dcollection?
        /// </summary>
        private bool _forceRefreshOfTk2dCollection = false;
#endif

        /// <summary>
        /// Fires when the editor comes to life.
        /// </summary>
        void OnEnable()
        {
            if (layout == null)
                layout = ScriptableObject.CreateInstance<Layout>();

            // reset our map list
            MapWatcher.Refresh();

            // fires when the map list change.
            MapWatcher.mapListChanged += HandleMapChanged;

            LoadDocument();
        }

        /// <summary>
        /// Fires when Unity tells this editor it is time to die.
        /// </summary>
        void OnDisable()
        {
            MapWatcher.mapListChanged -= HandleMapChanged;
        }

        /// <summary>
        /// Fires when the map list changes.
        /// </summary>
        void HandleMapChanged()
        {
            // sanity
            //if (_layout == null || _layout.document == null || _layout.selectedMapAssetPath == null) return;

            // did our map file just go away?
            if (layout.selectedMapAssetPath != null)
            {
                if (!MapWatcher.availableMaps.Contains(layout.selectedMapAssetPath))
                {
                    ResetEditor();
                }
                else
                {
                    // lets reload, we could have changed
                    // TODO: an optimization could be to know what if the selected map has changed.
                    LoadDocument();
                }
            }

#if PS2D_TK2D
            _forceRefreshOfTk2dCollection = true;
#else
#endif

            this.Repaint();
        }


        /// <summary>
        /// Reset the whole editor.
        /// </summary>
        void ResetEditor()
        {
            // reset everything
            if (layout != null)
            {
                layout.selectedMapAssetPath = null;
                layout.layoutDocumentAsset = null;
                layout.document = null;
                layout.ResetLayout();
            }
        }



        /// <summary>
        /// Grab or create the window.
        /// </summary>
        [MenuItem(Backpack.MenuItem)]
        public static void ShowWindow()
        {
            LayoutEditor window = (LayoutEditor)EditorWindow.GetWindow(typeof(LayoutEditor));
            window.title = Backpack.Name;
        }

        /// <summary>
        /// Fires when Unity decides to do inspector things.
        /// </summary>
        public void OnGUI()
        {

            // start a scrollable window
            layout.scrollPositionForOptions = EditorGUILayout.BeginScrollView(
                layout.scrollPositionForOptions,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
                );

            MakeHeader();

            if (MapWatcher.availableMaps.Count == 0)
            {
                MakeNoMaps();
                MakeGettingStarted();
            }
            else
            {
                GUILayout.Space(10);
                MakeMapPopup();
                if (layout == null || layout.document == null || layout.layoutDocumentAsset == null)
                {
                    MakeChooseMap();
                }
                else
                {
                    // show the image source popup
                    MakeImageSourcePopup();

                    // what did they select?
                    switch (layout.imageSource)
                    {
                        case TextureSource.None:
                            EditorGUILayout.HelpBox("Texture could not be auto-detected.  Please choose your textures.", MessageType.Error, true);
                            break;
                        case TextureSource.AssetFolder:
                            MakeImageAssetsFolderField();
                            break;
                        case TextureSource.Spritesheet:
                            MakeSpritesheetField();
                            break;
#if PS2D_TK2D
                        case TextureSource.Tk2dSpriteCollection:
                            MakeTk2dSpriteCollectionField();
                            break;
#endif
                        default:
                            break;
                    }

                    // do we have a valid iamge source?
                    if (layout.hasValidImageSource())
                    {
                        MakeAssembleButton();
                        MakeSettings();
                    }

                }
            }


            EditorGUILayout.EndScrollView();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(layout);
            }

        }

        /// <summary>
        /// No maps have been created.  Fresh project or fresh install.
        /// </summary>
        private void MakeNoMaps()
        {
            string noMaps = "No Ps2D Layer Maps found in project.";
            EditorGUILayout.HelpBox(noMaps, MessageType.Info);
            return;
        }

        /// <summary>
        /// Getting started section.
        /// </summary>
        private void MakeGettingStarted()
        {

            string title = "Getting Started";
            string nextStep = "The next step is to create some Ps2D Layer Maps from Photoshop.\n\nOnce you bring them into your Assets folder, you will get a chance to choose them in this window.";
            string readme = "Read the readme file for details and awesome tips!";

            GUIStyle paragraphStyle = new GUIStyle(EditorStyles.wordWrappedLabel);

            GUIStyle vertStyle = new GUIStyle(EditorStyles.label);
            GUILayout.BeginVertical(vertStyle);

            GUIStyle headingStyle = new GUIStyle(EditorStyles.boldLabel);

            GUILayout.Label(title, headingStyle);
            GUILayout.Label(nextStep, paragraphStyle);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.BeginVertical(vertStyle);
            GUILayout.Label(readme, paragraphStyle);

            GUILayout.EndVertical();

        }


        /// <summary>
        /// Create the "Assemble From Layer" section.
        /// </summary>
        private void MakeAssembleFromLayer()
        {
            bool hasLayers = layout != null && layout.document != null && layout.document.allLayers != null;

            string folderLabel = "Assemble From Layer";
            if (hasLayers)
            {
                folderLabel = string.Format("{0} ({1:d})", folderLabel, layout.document.allLayers.Count);
            }

            layout.foldoutFiltering = EditorGUILayout.Foldout(layout.foldoutFiltering, folderLabel);
            if (layout.foldoutFiltering)
            {
                EditorGUI.indentLevel++;

                if (layout != null && layout.document != null && layout.document.allLayers != null)
                {
                    foreach (Layer layer in layout.document.allLayers)
                    {
                        MakeAssembleFromLayerNode(layer);
                    }

                }

                EditorGUI.indentLevel--;
            }
        }


        /// <summary>
        /// Create a node for this layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        private void MakeAssembleFromLayerNode(Layer layer)
        {
            const int pixelsPerIndent = 15;

            // sanity
            if (layer == null) return;

            // grab the sprite layer.
            UnitySpriteLayer spriteLayer = layout.FindSpriteLayer(layer);

            if (spriteLayer == null)
            {

                return;
            }

            // don't even bother if we aren't a sprite and don't have kids
            bool deadLeaf = !spriteLayer.hasSprite && !spriteLayer.hasChildren;

            GUILayout.BeginVertical();

            GUIStyle horizontalStyle = new GUIStyle();
            horizontalStyle.padding = new RectOffset(20, 0, 0, 0);
            GUILayout.BeginHorizontal(horizontalStyle);

            GUIStyle iconStyle = new GUIStyle();
            iconStyle.padding = new RectOffset(0, 0, 2, 2);
            iconStyle.margin = new RectOffset(0, 0, 0, 0);
            iconStyle.border = new RectOffset(0, 0, 0, 0);
            iconStyle.stretchWidth = false;


            // indent column
            GUILayout.Space(System.Math.Max(0, layer.indentLevel) * pixelsPerIndent);


            // type column
            GUIContent typeContent = new GUIContent();
            if (spriteLayer.hasSprite)
            {
                if (layer.isVisible)
                {
                    typeContent.image = layout.editorGraphics.spriteLayerSprite;
                    typeContent.tooltip = "This is a sprite!";
                }
                else
                {
                    typeContent.image = layout.editorGraphics.spriteLayerInvisible;
                    typeContent.tooltip = "A sprite with a disabled renderer.";
                }

            }
            else
            {
                if (deadLeaf)
                {
                    typeContent.image = layout.editorGraphics.spriteLayerDeadLeaf;
                    typeContent.tooltip = "This dead-end layer will not be created.";
                }
                else
                {
                    typeContent.image = layout.editorGraphics.spriteLayerFolder;
                    typeContent.tooltip = "This is a group.";
                }

            }
            GUILayout.Label(typeContent, iconStyle);

            // name column
            GUIContent nameContent = new GUIContent();
            nameContent.text = layout.GetBestName(layer);
            GUIStyle nameStyle = new GUIStyle(EditorStyles.label);
            nameStyle.stretchWidth = true;
            GUILayout.Label(nameContent, nameStyle);

            // assemble column
            if (!deadLeaf)
            {
                GUIContent assembleButtonContent = new GUIContent();
                assembleButtonContent.image = layout.editorGraphics.usain;
                if (spriteLayer.hasChildren)
                {
                    assembleButtonContent.tooltip = "Assemble this layer and children.";
                }
                else
                {
                    assembleButtonContent.tooltip = "Assemble this single sprite.";
                }


                GUIStyle assembleButtonStyle = new GUIStyle(EditorStyles.miniButton);
                assembleButtonStyle.stretchWidth = false;
                if (GUILayout.Button(assembleButtonContent, assembleButtonStyle))
                {
                    layout.startingLayer = layer;
#if PS2D_TK2D
                    SpriteCreator.CreateSprites(layout, layer, _spriteCollectionData);
#else
                    SpriteCreator.CreateSprites(layout, layer);
#endif
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Make the settings.
        /// </summary>
        private void MakeSettings()
        {
            CreateSubtitle("Settings");

            GUIStyle optionsStyle = new GUIStyle();

            optionsStyle.padding = new RectOffset(4, 8, 0, 8);
            optionsStyle.margin = new RectOffset(4, 4, 0, 8);
            GUILayout.BeginVertical(optionsStyle);


            layout.foldoutPosition = EditorGUILayout.Foldout(layout.foldoutPosition, "Position");
            if (layout.foldoutPosition)
            {
                EditorGUI.indentLevel++;
                MakeAnchorField();
                MakeTrimToSpritesField();
                EditorGUI.indentLevel--;
            }

            layout.foldoutScale = EditorGUILayout.Foldout(layout.foldoutScale, "Scale");
            if (layout.foldoutScale)
            {
                EditorGUI.indentLevel++;
                MakePixelsToUnits();
                MakeCoordinatesScalingField();
                MakeScalingField();
                EditorGUI.indentLevel--;
            }

            layout.foldoutPrefabs = EditorGUILayout.Foldout(layout.foldoutPrefabs, "Prefabs");
            if (layout.foldoutPrefabs)
            {
                EditorGUI.indentLevel++;
                MakeReplicatorField();
                EditorGUI.indentLevel--;
            }

            layout.foldoutDepth = EditorGUILayout.Foldout(layout.foldoutDepth, "Depth");
            if (layout.foldoutDepth)
            {
                EditorGUI.indentLevel++;
                MakeSortingLayerNameField();
                MakeSortingOrderField();
                MakeSortingOrderStepField();
                MakeZOffsetField();
                EditorGUI.indentLevel--;
            }

            layout.foldoutPhysics = EditorGUILayout.Foldout(layout.foldoutPhysics, "Physics");
            if (layout.foldoutPhysics)
            {
                EditorGUI.indentLevel++;
                MakeRootColliderField();
                MakeRootColliderIsTrigger();
                EditorGUI.indentLevel--;
            }

            layout.foldoutNesting = EditorGUILayout.Foldout(layout.foldoutNesting, "Nesting");
            if (layout.foldoutNesting)
            {
                EditorGUI.indentLevel++;
                MakePhotoshopLayoutOption();
                MakeSpriteParentOption();
                EditorGUI.indentLevel--;
            }

            MakeAssembleFromLayer();


            GUILayout.EndVertical();

        }

        /// <summary>
        /// Make the Choose Map graphic.
        /// </summary>
        void MakeChooseMap()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUIContent contentChoose = new GUIContent(layout.editorGraphics.chooseMap);
            GUIStyle styleChoose = new GUIStyle();
            styleChoose.padding = new RectOffset(Mathf.RoundToInt(EditorGUIUtility.labelWidth - 100), 0, 0, 0);
            styleChoose.stretchWidth = false;
            GUILayout.Label(contentChoose, styleChoose);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Make a nice header for the window.
        /// </summary>
        void MakeHeader()
        {
            // background
            Color bgColor = new Color(0, 174f / 255f, 239f / 255f);
            Color lilDarker = Color.Lerp(bgColor, Color.black, 0.25f);
            EditorGUI.DrawRect(new Rect(0, 0, 10000, 28), bgColor);
            EditorGUI.DrawRect(new Rect(0, 28, 10000, 1), lilDarker);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            // a logo
            GUIContent contentLogo = new GUIContent(layout.editorGraphics.editorTitle);
            GUIStyle contentStyle = new GUIStyle();
            contentStyle.padding = new RectOffset(0, 0, 0, 0);
            contentStyle.stretchWidth = false;
            GUILayout.Label(contentLogo, contentStyle);

            // Version
            GUIStyle styleVersion = new GUIStyle(EditorStyles.label);
            styleVersion.alignment = TextAnchor.MiddleRight;
            styleVersion.stretchWidth = true;
            styleVersion.fixedHeight = 28;
            GUIContent contentVersion = new GUIContent(Backpack.Version);
            GUILayout.Label(contentVersion, styleVersion);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(1);

        }

        /// <summary>
        /// The giant assemble button.
        /// </summary>
        void MakeAssembleButton()
        {
            GUILayout.Space(10);

            GUIContent buttonContent = new GUIContent("Assemble!");
            buttonContent.image = layout.editorGraphics.usain;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));

            buttonStyle.padding = new RectOffset(10, 10, 10, 10);

            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.ExpandWidth(true)))
            {
                layout.startingLayer = null;
#if PS2D_TK2D
                SpriteCreator.CreateSprites(layout, null, _spriteCollectionData);
#else
                SpriteCreator.CreateSprites(layout, null);
#endif
            }

            GUILayout.Space(10);
        }

        /// <summary>
        /// Make the popup for choosing the map.
        /// </summary>
        void MakeMapPopup()
        {
            // grab the friendly names of the maps
            List<string> availableMaps = MapWatcher.availableMaps;
            List<string> mapNames = availableMaps.ConvertAll<string>(each => Path.GetFileNameWithoutExtension(each).Replace(Backpack.MapExtension, ""));

            // create a friendly list for users
            // TODO:  Popups don't support duplicates, so might want to deal with this with nested lists or something
            List<string> chooseables = new List<string>();
            chooseables.Add("-");
            chooseables.AddRange(mapNames);
            List<GUIContent> guiChooseables = chooseables.ConvertAll<GUIContent>(each => new GUIContent(each));

            // remember the old choice
            string previousChoice = layout.selectedMapAssetPath;

            // what has the user selected?
            int selectedIndex = availableMaps.IndexOf(layout.selectedMapAssetPath);

            GUIContent labelContent = new GUIContent();
            labelContent.text = string.Format("{0} Map", Backpack.Name);
            labelContent.tooltip = "Choose a map file that was generated by the Photoshop plugin.";

            // show the popup
            selectedIndex++;
            selectedIndex = EditorGUILayout.Popup(labelContent, selectedIndex, guiChooseables.ToArray());
            selectedIndex--;

            // did the user get something good?
            if (selectedIndex >= 0)
            {
                layout.selectedMapAssetPath = availableMaps[selectedIndex];
            }
            else
            {
                layout.selectedMapAssetPath = null;
            }

            bool isNull = layout.selectedMapAssetPath == null;
            bool isDifferent = layout.selectedMapAssetPath != previousChoice;
            bool isNotLoaded = layout.selectedMapAssetPath != null && layout.document == null;

            if (isNull)
            {
                layout.layoutDocumentAsset = null;
                layout.ResetLayout();
            }
            else if (isDifferent || isNotLoaded)
            {
                layout.layoutDocumentAsset = (TextAsset)AssetDatabase.LoadMainAssetAtPath(layout.selectedMapAssetPath);
                LoadDocument();
            }
        }

        /// <summary>
        /// A helper to create the step title.
        /// </summary>
        /// <param name="title">The title text.</param>
        void CreateSubtitle(string title)
        {
            GUIContent label = new GUIContent(title);
            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(6, 0, 0, 0);
            style.fontStyle = FontStyle.Bold;
            GUILayout.Label(label, style);
        }

        /// <summary>
        /// The field for setting the image source.
        /// </summary>
        void MakeImageSourcePopup()
        {
            if (layout == null || layout.layoutDocumentAsset == null) return;

            GUIContent labelContent = new GUIContent();
            labelContent.text = "Texture Type";
            labelContent.tooltip = "Choose the type of textures we should use.";

            layout.imageSource = (TextureSource)EditorGUILayout.EnumPopup(labelContent, layout.imageSource);
            layout.UpdateImageSource();
        }

        /// <summary>
        /// Allows users to pick the asset folder containing their images.
        /// </summary>
        void MakeImageAssetsFolderField()
        {
            if (layout == null ||
                layout.document == null ||
                layout.imageSource != TextureSource.AssetFolder
                ) return;

            var previousValue = layout.imageSourceAssetFolder;

            GUIContent contentLabel = new GUIContent();
            contentLabel.text = "Asset Folder";
            contentLabel.tooltip = "The asset folder which contains the textures.";
            layout.imageSourceAssetFolder = EditorGUILayout.ObjectField(contentLabel, layout.imageSourceAssetFolder, typeof(Object), false);

            // did we change?
            if (layout.imageSourceAssetFolder != null && previousValue != layout.imageSourceAssetFolder)
            {
                SpriteAssigner.AssignSpritesFromFolder(layout);
            }
        }

        /// <summary>
        /// A field for the replicator.
        /// </summary>
        void MakeReplicatorField()
        {
            GUIContent contentLabel = new GUIContent();
            contentLabel.text = "Sprite Prefab";
            contentLabel.tooltip = "The prefab to use when creating each new sprite.";
            layout.replicator = (GameObject)EditorGUILayout.ObjectField(contentLabel, layout.replicator, typeof(GameObject), false);
        }

        /// <summary>
        /// A field for choosing the sprite layer.
        /// </summary>
        void MakeSortingLayerNameField()
        {
            if (layout == null ||
                layout.document == null
                ) return;

            // grab the sorting layer information from unity
            string[] sortingLayers = SortingLayers.GetNames();
            List<string> sortingLayersList = new List<string>(sortingLayers);
            GUIContent[] sortingLayersContent = sortingLayersList.ConvertAll<GUIContent>(each => new GUIContent(each)).ToArray();
            int indexOfDefaultSortingLayer = SortingLayers.IndexOfDefaultSortingLayer();

            // which one do we use?
            int indexOfSelectedSortingLayerName = sortingLayersList.IndexOf(layout.sortingLayerName);

            // nothing?  let's find something sane
            if (indexOfDefaultSortingLayer < 0)
            {
                indexOfSelectedSortingLayerName = indexOfDefaultSortingLayer;
            }

            GUIContent contentLabel = new GUIContent();
            contentLabel.text = "Sorting Layer";
            contentLabel.tooltip = "The sorting layer to be applied to each sprite.\n\nThe default is Default.";

            // setup the field
            indexOfSelectedSortingLayerName = EditorGUILayout.Popup(contentLabel, indexOfSelectedSortingLayerName, sortingLayersContent);

            // reset to something sane if we need to
            if (indexOfSelectedSortingLayerName < 0)
            {
                indexOfSelectedSortingLayerName = indexOfDefaultSortingLayer;
            }

            // update our layout with the selection
            layout.sortingLayerName = sortingLayers[indexOfSelectedSortingLayerName];
        }

        /// <summary>
        /// A field for changing the sorting order
        /// </summary>
        void MakeSortingOrderField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Sorting Order";
            labelContent.tooltip = "The sorting order to assign new sprites.\n\nThe default is 0.";
            layout.sortingOrder = EditorGUILayout.IntField(labelContent, layout.sortingOrder);
        }

        /// <summary>
        /// A field for changing the sorting order
        /// </summary>
        void MakeSortingOrderStepField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Counter";
            labelContent.tooltip = "When enabled, each sprite created will add 1 to the sorting order.\n\nThe default is on.";

            bool isSteppingOn = layout.sortingOrderStep == 1;
            EditorGUI.indentLevel++;
            isSteppingOn = EditorGUILayout.Toggle(labelContent, isSteppingOn);
            EditorGUI.indentLevel--;
            layout.sortingOrderStep = isSteppingOn ? 1 : 0;
        }

        /// <summary>
        /// A field for changing the z offset.
        /// </summary>
        void MakeZOffsetField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Z Offset";
            labelContent.tooltip = "Add a little bit of Z depth to each imported sprite.\n\nThe default is 0.";
            layout.zOffset = EditorGUILayout.FloatField(labelContent, layout.zOffset);
        }

        /// <summary>
        /// A field for changing the photoshop scale.
        /// </summary>
        void MakeCoordinatesScalingField()
        {
            EditorGUILayout.BeginHorizontal();

            GUIContent labelContent = new GUIContent();
            labelContent.text = "Photoshop Ratio";
            labelContent.tooltip = "How much bigger is the PSD than the imported graphics?\n\nIf scale your graphics to 50% before using in Unity, you should select 2x.\n\nThe default is 1x.";
            EditorGUILayout.PrefixLabel(labelContent);

            GUIContent content1x = new GUIContent("1x");
            GUIContent content2x = new GUIContent("2x");
            GUIContent content4x = new GUIContent("4x");
            GUIContent[] options = { content1x, content2x, content4x };

            int idx = 0;
            if (layout.coordinatesScale == 0.5f) idx = 1;
            if (layout.coordinatesScale == 0.25f) idx = 2;

            idx = GUILayout.SelectionGrid(idx, options, options.Length, GUILayout.ExpandWidth(false));

            if (idx == 0) layout.coordinatesScale = 1f;
            if (idx == 1) layout.coordinatesScale = 0.5f;
            if (idx == 2) layout.coordinatesScale = 0.25f;

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// A field for changing the scale of the sprites.
        /// </summary>
        void MakeScalingField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Size Multiplier";
            labelContent.tooltip = "How much to scale the new game object (uses local scale).\n\nThe default is 1 (original size).";
            float x = layout.scale.x;
            x = EditorGUILayout.FloatField(labelContent, x);
            layout.scale = new Vector3(x, x, x);

        }

        /// <summary>
        /// A field for changing the pixels to units
        /// </summary>
        void MakePixelsToUnits()
        {
            if (layout == null ||
                layout.document == null
                ) return;

            GUIContent contentLabel = new GUIContent();
            contentLabel.text = "Pixels To Units";
            contentLabel.tooltip = "The pixels to units conversion ratio that your imported images use.\n\nThe default is 100.";

            layout.pixelsToUnits = EditorGUILayout.IntField(contentLabel, layout.pixelsToUnits);
        }

        /// <summary>
        /// Choose the spritesheet.
        /// </summary>
        void MakeSpritesheetField()
        {
            if (layout == null ||
                layout.document == null ||
                layout.imageSource != TextureSource.Spritesheet
                ) return;

            var previousValue = layout.spritesheetTexture;
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Spritesheet";
            labelContent.tooltip = "The place where your spritesheet lives.";
            layout.spritesheetTexture = (Texture)EditorGUILayout.ObjectField(labelContent, layout.spritesheetTexture, typeof(Texture), false, GUILayout.ExpandWidth(false));

            // did we change?
            if (layout.spritesheetTexture != null && previousValue != layout.spritesheetTexture)
            {
                SpriteAssigner.AssignSpritesFromSpritesheet(layout);
            }
        }

        /// <summary>
        /// Choose a 2D Toolkit Sprite Collection field.
        /// </summary>
        void MakeTk2dSpriteCollectionField()
        {
#if PS2D_TK2D
            if (layout == null ||
                layout.document == null ||
                layout.imageSource != TextureSource.Tk2dSpriteCollection
                ) return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            GUIContent labelContent = new GUIContent();
            labelContent.text = "Sprite Collection";
            labelContent.tooltip = "The 2D Toolkit sprite collection to use.";
            EditorGUILayout.PrefixLabel(labelContent);

            // discover the state of the sprite collection build keys
            bool freshBuildKey = layout.currentSpriteCollectionBuildKey == 0;
            bool differentBuildKey = _spriteCollectionData != null && _spriteCollectionData.buildKey != layout.currentSpriteCollectionBuildKey;

            // should we reset the tk2d sprite collection cache?
            if (freshBuildKey || differentBuildKey)
            {
                _forceRefreshOfTk2dCollection = true;
                ReloadCollection();
            }

            // show the list
            EditorGUI.BeginChangeCheck();
            var previousSpriteCollection = _spriteCollectionData;
            _spriteCollectionData = tk2dSpriteGuiUtility.SpriteCollectionList(previousSpriteCollection);
            bool changed = EditorGUI.EndChangeCheck();

            // did we change to something valid?
            if (changed && _spriteCollectionData != null)
            {
                // let's just reload the collection... this too WAY too long to figure out
                // and I'm still convinced I'm doing it wrong.  It seems to work though.
                // then again, so does communism until you get it into the hands of the people.
                ReloadCollection();
            }


            // open in tk2d sprite collection editor
            GUIContent openInEditorContent = new GUIContent();
            openInEditorContent.image = layout.editorGraphics.cog;
            openInEditorContent.tooltip = "Opens the 2D Toolkit sprite collection window.";
            GUIStyle openInEditorStyle = new GUIStyle(EditorStyles.miniButton);
            if (GUILayout.Button(openInEditorContent, openInEditorStyle))
            {
                // do we have a valid collection data?
                if (_spriteCollectionData != null)
                {
                    // find and load the collection asset
                    string collectionPath = AssetDatabase.GUIDToAssetPath(_spriteCollectionData.spriteCollectionGUID);
                    tk2dSpriteCollection spriteCollectionReload = AssetDatabase.LoadAssetAtPath(collectionPath, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;

                    // got it?
                    if (spriteCollectionReload != null)
                    {
                        // find and show the editor
                        tk2dSpriteCollectionEditorPopup v = EditorWindow.GetWindow(typeof(tk2dSpriteCollectionEditorPopup), false, "Sprite Collection Editor") as tk2dSpriteCollectionEditorPopup;
                        v.SetGeneratorAndSelectedSprite(spriteCollectionReload, _spriteCollectionData.FirstValidDefinitionIndex);
                        v.Show();
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // should we reassign the sprites in our list?
            if ((_forceRefreshOfTk2dCollection || changed) && _spriteCollectionData != null)
            {
                // do we have a different tk2d build key?
                differentBuildKey = _spriteCollectionData.buildKey != layout.currentSpriteCollectionBuildKey;
                layout.currentSpriteCollectionBuildKey = _spriteCollectionData.buildKey;
                if (differentBuildKey || _forceRefreshOfTk2dCollection)
                {
                    // let's reload our sprite collection
                    SpriteAssigner.AssignSpritesFromTk2dCollection(layout, _spriteCollectionData);
                }
            }
            _forceRefreshOfTk2dCollection = false;

#endif
        }

        /// <summary>
        /// A field to choose if we want extra sprite parenting.
        /// </summary>
        void MakeSpriteParentOption()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Extra Sprite Parent";
            labelContent.tooltip = "Creates an extra GameObject parent for each sprite.\n\nThe default is off.";

            layout.addExtraParentToSprite = EditorGUILayout.Toggle(labelContent, layout.addExtraParentToSprite);
        }

        /// <summary>
        /// A field to choose if we want the photoshop layers instead of flat sprites.
        /// </summary>
        void MakePhotoshopLayoutOption()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Preserve PS Layers";
            labelContent.tooltip = "Keeps the layer hierarchy from your Photoshop file.\n\nThe default is off.";
            layout.preservePhotoshopLayers = EditorGUILayout.Toggle(labelContent, layout.preservePhotoshopLayers);
        }

        /// <summary>
        /// A field where to place the anchor.
        /// </summary>
        void MakeAnchorField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Anchor";
            labelContent.tooltip = "How to position the sprites.\n\nTry using Lower Center for characters that stand on the ground.\n\nThe default is Middle Center.";
            layout.anchor = (TextAnchor)EditorGUILayout.EnumPopup(labelContent, layout.anchor);
        }

        /// <summary>
        /// A field to choose trimming to sprites.
        /// </summary>
        void MakeTrimToSpritesField()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Trim To Sprites";
            labelContent.tooltip = "Remove any whitespace between the edges of the sprites imported and the document size.\n\nThe default is on.";
            layout.trimToSprites = EditorGUILayout.Toggle(labelContent, layout.trimToSprites);
        }

        /// <summary>
        /// A field to choose the collider type for the root.
        /// </summary>
        void MakeRootColliderField()
        {
            EditorGUILayout.BeginHorizontal();

            GUIContent labelContent = new GUIContent();
            labelContent.text = "Root Box Collider";
            labelContent.tooltip = "Add a collider to the root item which will be the size of the sprites placed inside.\n\nThe default is none.";
            EditorGUILayout.PrefixLabel(labelContent);

            GUIContent content1x = new GUIContent("None");
            GUIContent content2x = new GUIContent("2D");
            GUIContent content4x = new GUIContent("3D");
            GUIContent[] options = { content1x, content2x, content4x };

            int idx = 0;
            if (layout.rootCollider == ColliderStyle.BoxCollider2D) idx = 1;
            if (layout.rootCollider == ColliderStyle.BoxCollider) idx = 2;

            idx = GUILayout.SelectionGrid(idx, options, options.Length, GUILayout.ExpandWidth(false));

            if (idx == 0) layout.rootCollider = ColliderStyle.None;
            if (idx == 1) layout.rootCollider = ColliderStyle.BoxCollider2D;
            if (idx == 2) layout.rootCollider = ColliderStyle.BoxCollider;

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// A field to choose a trigger or not.
        /// </summary>
        void MakeRootColliderIsTrigger()
        {
            GUIContent labelContent = new GUIContent();
            labelContent.text = "Is Trigger";
            labelContent.tooltip = "Is this collider a trigger?";
            layout.rootColliderTrigger = EditorGUILayout.Toggle(labelContent, layout.rootColliderTrigger);
        }

        /// <summary>
        /// Load the document.
        /// </summary>
        void LoadDocument()
        {
            if (layout.layoutDocumentAsset == null) return;

            // load the map
            layout.Load();

            // update the root friendly name
            if (layout.document != null & layout.document.allLayers != null)
            {
                layout.document.allLayers[0].photoshopLayerName = layout.GetFriendlyDocumentName();
            }

            // try to auto-detect the texture source
            FormatGuesser guesser = new FormatGuesser();
            guesser.layout = layout;
            switch (guesser.Guess())
            {
                case TextureSource.AssetFolder:
                    SpriteAssigner.AssignSpritesFromFolder(layout);
                    break;

                case TextureSource.Spritesheet:
                    SpriteAssigner.AssignSpritesFromSpritesheet(layout);
                    break;
#if PS2D_TK2D
                case TextureSource.Tk2dSpriteCollection:
                    _spriteCollectionData = guesser.spriteCollectionData;
                    SpriteAssigner.AssignSpritesFromTk2dCollection(layout, _spriteCollectionData);
                    break;
#endif
                default:
                    break;
            }


        }


#if PS2D_TK2D

        /// <summary>
        /// Reload the collection that changed.
        /// </summary>
        public void ReloadCollection()
        {
            // safety
            if (_spriteCollectionData == null) return;

            // grab the sprite collection path
            string collectionPath = AssetDatabase.GUIDToAssetPath(_spriteCollectionData.spriteCollectionGUID);

            // reload the collection
            tk2dSpriteCollection spriteCollectionReload = AssetDatabase.LoadAssetAtPath(collectionPath, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
            if (spriteCollectionReload != null)
            {
                // reassign the collection instance
                _spriteCollectionData = spriteCollectionReload.spriteCollection.inst;
            }
        }


        /// <summary>
        /// 2D Toolkit wants to know if we're using this sprite collection.
        /// </summary>
        /// <param name="spriteCollection">This one right here.</param>
        /// <returns>Yes or no.</returns>
        public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
        {
            return _spriteCollectionData == spriteCollection;
        }

        /// <summary>
        /// 2D Toolkit is letting us know we should probably update ourself because the sprite
        /// collection we are using was just foot sweeped.
        /// </summary>
        public void ForceBuild()
        {
            // reload the current collection because it has changed.
            ReloadCollection();
            this.Repaint();
        }
#endif

    }

}
