#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using System.Collections;
using System.IO;

[CustomEditor(typeof(MapEditor))]
public class GridEditor : Editor {

    MapEditor m_grid;
    private int m_oldIndex = 0;

	private bool m_isMouse1Down = false;
	private bool m_isMouse2Down = false;

    // Used for making sure that the user dont remove a map by mistake
    private int m_deleteAll = 0;

    private Texture2D[] m_textures = new Texture2D[0];

    // Variables for the Tile Selector
    private Color m_standardColor;
    private Color m_standardBackgroundColor;
    private Color m_selectedColor;
    private Color m_selectedBackgroundColor;

    private GUILayoutOption[] m_standardLayoutOptions = new GUILayoutOption[2]
                    {
                       GUILayout.Width(64), GUILayout.Height(64)
                    };


    private string[] m_acceptableTags = new string[1]
                    {
                        "EnvironmentObject"
                    };


    void OnEnable()
    {
        // Finds the target for this custom editor
        m_grid = (MapEditor)target;

        m_grid.m_tilePrefab = m_grid.m_tileset.prefabs[m_oldIndex];

        // Loads all Textures from TileSet
        LoadTileTextures();

        // Sets all preset Colors for Tile Selection
        m_standardColor = new Color(0.7f, 0.7f, 0.7f, 1);
        m_standardBackgroundColor = new Color(1, 1, 1, 1);
        m_selectedColor = new Color(1, 1, 1, 1);
        m_selectedBackgroundColor = new Color(1, 0.92f, 0.016f, 1);
    }


    [MenuItem("Assets/Create/TileSet")]
    static void CreateTileSet()
    {
        var asset = ScriptableObject.CreateInstance<TileSet>();
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
        {
            path = "Assets/";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(path), "");
        }
        else
        {
            path += "/";
        }

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "TileSet.asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        asset.hideFlags = HideFlags.DontSave;
    }

    public override void OnInspectorGUI()
    {
        InfoBox("Toggle if you need Help. All info will be ABOVE the given option.");
        if (m_grid.m_helpBoxes = EditorGUILayout.Toggle("Enable Help?", m_grid.m_helpBoxes))

            InfoBox("Toggles the visible Grid in scene-view, on/off.");
        // Toggle box to enable/disable the grid
        if (GUILayout.Button("Toggle Grid"))
        {
            m_grid.m_displayGrid = !m_grid.m_displayGrid;

            // Repaints the scene
            SceneView.RepaintAll();
        }

        InfoBox("Displays a window with options for the Grid. (pref. Width = 1, Height = 1) ");
        if (GUILayout.Button("Open Grid Window"))
        {
            GridWindow window = (GridWindow)EditorWindow.GetWindow(typeof(GridWindow));
            window.init();
        }

        EditorGUILayout.Space();

        InfoBox("Add diffrent Tilesets to this Map. Requires a TileSet from Assets. (Assets/Create/TileSet)");
        // Allows user to add tilesets to this Map.
        serializedObject.Update();
        SerializedProperty tps = serializedObject.FindProperty ("m_mapSets");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();


        EditorGUILayout.Space();

        InfoBox("Allows you to start drawing in the scene-view and change tiles or tilesets.");
        m_grid.m_displayTileSelector = EditorGUILayout.Toggle("Enable Map Editor: ", m_grid.m_displayTileSelector);

        if (m_grid.m_displayTileSelector)
        {
            EditorGUILayout.Space();

            InfoBox("Here you can select between your Tilemaps. If its not showing up, make sure you have added a tilemap.");
            TilemapSelector();

            InfoBox("Here you can select your Tiles. If its not showing up just re-select the object or a tilemap in the Inspector. (You might not have a tilemap added?)");
            TileSelector();

            EditorGUILayout.Space();
            InfoBox("If you have a tile selected that is of an acceptable object, you will be able to add some special options to it. (Will always snap in Y-axis)");
            ObjectOptions();

            EditorGUILayout.Space();

            InfoBox("Here you can select your layer. -5 is FRONT and 5 is BACK. Get used to it (Or I can change)");
            LayerSelection();

            EditorGUILayout.Space();
            
            InfoBox("Toggles the placement of one way platforms, on/off.");
            // Toggle box to enable/disable placement of one way platforms
            m_grid.m_oneWay = EditorGUILayout.Toggle("One Way Platform: ", m_grid.m_oneWay);

            EditorGUILayout.Space();
        }

        InfoBox("Allows you to clear the entire map, if you press, you will get a 'are you sure?' prompt. CAN'T be deleted by accident... hopefully");

        // Buttons for Clearing the entire map.
        if (m_deleteAll == 0 && GUILayout.Button("Reset Map"))
        {
            m_deleteAll++;
        }
        else if (m_deleteAll == 1)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                m_deleteAll++;
            }
            if (GUILayout.Button("No"))
            {
                m_deleteAll--;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("If you press 'Yes' your map will be deleted.", MessageType.Warning);
        }
        else if (m_deleteAll == 2)
        {
            ClearAll();
        }
        else 
        {
            m_deleteAll = 0;
        }
    }

    void InfoBox(string message) 
    {
        if (m_grid.m_helpBoxes)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }

    /// <summary>
    /// Clears ALL objects in the scene.
    /// </summary>
    void ClearAll()
    {
        // Clears untill both Child Transforms are empty.
        while(m_grid.transform.GetChild(0).childCount > 0)
        {
            DestroyImmediate(m_grid.transform.GetChild(0).GetChild(0).gameObject);
        }

        while(m_grid.transform.GetChild(1).childCount > 0)
        {
            DestroyImmediate(m_grid.transform.GetChild(1).GetChild(0).gameObject);
        }

        // Resets int for safety.
        m_deleteAll = 0;
    }

    /// <summary>
    /// Allows user to select a tilemap from the given Tile Sets.
    /// </summary>
    void TilemapSelector()
    {
        string[] names = new string[m_grid.m_mapSets.Length];
        int[] options = new int[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = m_grid.m_mapSets[i].m_name;
            options[i] = i;
        }

        EditorGUI.BeginChangeCheck();
        int newTileSet = EditorGUILayout.IntPopup("Select tileset: ", m_grid.m_currentSet, names, options);
        if (EditorGUI.EndChangeCheck())
        {
            m_grid.m_tileset = m_grid.m_mapSets[newTileSet].m_tileset;
            m_grid.m_tilePrefab = m_grid.m_tileset.prefabs[m_oldIndex];

            // Reloads textures from other tileset.
            LoadTileTextures();
            m_grid.m_currentLayerDepth = 0;
            m_grid.m_currentSet = newTileSet;

            Undo.RecordObject(target, "Grid tileSet changed");
        }
    }

    /// <summary>
    /// Displays the Tile Selection in the inspector.
    /// </summary>
    void TileSelector() 
    {
        GUILayout.Label("Tile Selector:");

        int rows = Mathf.CeilToInt(m_textures.Length / 4.0f);
        int maxCol = 4;
        bool thisRow = false;

        for (int row = 0; row < rows; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < maxCol; col++)
            {
                int i = (maxCol * row) + col;

                // Checks whether there are more textures or not.
                if (i >= m_textures.Length)
                {
                    break;
                }
                if (AssetPreview.GetAssetPreview(m_grid.m_tilePrefab.gameObject) == m_textures[i] && !thisRow)
                {
                    GUI.color = m_selectedColor;
                    GUI.backgroundColor = m_selectedBackgroundColor;

                    GUILayout.Button(m_textures[i], m_standardLayoutOptions);
                    thisRow = true;
                }
                else
                {
                    GUI.color = m_standardColor;
                    GUI.backgroundColor = m_standardBackgroundColor;

                    if (GUILayout.Button(m_textures[i], m_standardLayoutOptions))
                    {
                        m_grid.m_tilePrefab = m_grid.m_tileset.prefabs[i];
                    }
                }
                GUILayout.Space(-6);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(-5);
        }

        if (GUI.color != Color.white || GUI.backgroundColor != Color.white)
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }
    }

    /// <summary>
    /// Displays extra options for certain Objects.
    /// </summary>
    void ObjectOptions()
    {
        if (RandomOptionsAvalible())
        {
            EditorGUILayout.Space();

            m_grid.m_mouseOffsetX = EditorGUILayout.Toggle("Mouse Placement: ", m_grid.m_mouseOffsetX);

            if (!m_grid.m_mouseOffsetX)
            {
                m_grid.m_randomOffset = EditorGUILayout.BeginToggleGroup("Random Offset: ", m_grid.m_randomOffset);
                m_grid.m_offsetMax = EditorGUILayout.FloatField("Max Offset: ", m_grid.m_offsetMax);
                EditorGUILayout.EndToggleGroup();
            }

            m_grid.m_flipPrefab = EditorGUILayout.BeginToggleGroup(" Flip Object", m_grid.m_flipPrefab);
            if (!m_grid.m_flipPrefab) m_grid.m_flipRandom = false;
            m_grid.m_flipRandom = EditorGUILayout.ToggleLeft(" Randomly", m_grid.m_flipRandom);
            EditorGUILayout.EndToggleGroup();
        }
    }

    /// <summary>
    /// Displays a slider for Layer (Z-depth).
    /// </summary>
    void LayerSelection()
    {
        // Allows for the selection of Layer instantly.
        EditorGUI.BeginChangeCheck();
        int newLayer = EditorGUILayout.IntSlider("Current Layer", m_grid.m_currentLayerDepth, -5, 5);
        if (EditorGUI.EndChangeCheck())
        {
            m_grid.m_currentLayerDepth = newLayer;
            Undo.RecordObject(target, "Layer changed");
        }

    }

    /// <summary>
    /// Function for loading all tile textures.
    /// </summary>
    void LoadTileTextures()
    {
        m_textures = new Texture2D[m_grid.m_tileset.prefabs.Length];

        Debug.Log(m_textures.Length);

        for (int i = 0; i < m_grid.m_tileset.prefabs.Length; i++)
        {
            m_textures[i] = AssetPreview.GetAssetPreview(m_grid.m_tileset.prefabs[i].gameObject);
        }
    }

    /// <summary>
    /// Returns True if the current object is acceptable, false if it's not.
    /// </summary>
    /// <returns>Returns true/false, if the Object is avalible.</returns>
    bool RandomOptionsAvalible()
    {
        foreach (string acceptableTag in m_acceptableTags)
        {
            if (m_grid.m_tilePrefab.tag == acceptableTag)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the given mouse button to true/false. Used to check for continuous mouse inputs in Scene window.
    /// </summary>
    /// <param name="mouse">Button number from mouse.</param>
    /// <param name="check">Set to true/false.</param>
    void setMouse(int mouse, bool check)
    {
        if (m_grid.m_tilePrefab.tag == "Tile")
        {
            if(mouse == 1)
                m_isMouse1Down = check;
            else if (mouse == 2)
                m_isMouse2Down = check;
        }
    }

    void OnSceneGUI(){
        // Allows no KEYBOARD focus controls.
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;
        
        // Makes sure that it actually is a mouse input.
        if (e.isMouse && m_grid.m_displayTileSelector) 
        {
		    Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
            Vector3 mousePos = ray.origin;


		    if (e.type == EventType.MouseUp && (e.button == 0 || e.button == 1) && e.button != 2)
		    {
			    GUIUtility.hotControl = 0;
			    if(m_isMouse1Down)
				    setMouse(1, false);

			    if(m_isMouse2Down)
                    setMouse(2, false);
		    }

		    if ((e.type == EventType.MouseDown || m_isMouse1Down) && e.button == 0)
            {
                GUIUtility.hotControl = controlId;
                setMouse(2, false);
                setMouse(1, true);
                e.Use();

                GameObject gameObj;
                Transform prefab = m_grid.m_tilePrefab;

                if (prefab)
                {
                    Undo.IncrementCurrentGroup();
                    Vector3 align = new Vector3(Mathf.Floor(mousePos.x / m_grid.m_gridWidth) * m_grid.m_gridWidth + (m_grid.m_gridWidth / 2.0f),
                                                    Mathf.Floor(mousePos.y / m_grid.m_gridHeight) * m_grid.m_gridHeight + (m_grid.m_gridHeight / 2.0f), m_grid.m_currentLayerDepth);

                    if (getTileTransformFromPosition(align) != null) return;

                    gameObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
                    gameObj.transform.position = align;

                    // If one way plaftorm placement is enabled
                    if (m_grid.m_oneWay)
                        // Add the one way platform component to the instantiated object
                        gameObj.AddComponent<OneWayPlatform>();

                    if (RandomOptionsAvalible())
                    {
                        if (m_grid.m_mouseOffsetX)
                        {
                           align = new Vector3(mousePos.x,
                                                        Mathf.Floor(mousePos.y / m_grid.m_gridHeight) * m_grid.m_gridHeight + (m_grid.m_gridHeight / 2.0f), m_grid.m_currentLayerDepth);
                        }

                        gameObj.transform.position = align;

                        if(m_grid.m_randomOffset)
                        {
                            Vector3 offset = new Vector3(Random.Range(-m_grid.m_offsetMax, m_grid.m_offsetMax), 0, 0);

                            gameObj.transform.position += offset;
                        }

                        if (m_grid.m_flipRandom)
                        {
                            int value = Random.Range(0, 2);
                            if (value == 1)
                            {
                                gameObj.GetComponent<SpriteRenderer>().flipX = !gameObj.GetComponent<SpriteRenderer>().flipX;
                            }
                        }
                        else if (m_grid.m_flipPrefab)
                        {
                            gameObj.GetComponent<SpriteRenderer>().flipX = !gameObj.GetComponent<SpriteRenderer>().flipX;
                        }

                        gameObj.transform.parent = m_grid.transform.GetChild(0);
                    }
                    else
                    {
                        gameObj.transform.parent = m_grid.transform.GetChild(1);
                    }

                    Undo.RegisterCreatedObjectUndo(gameObj, "Create " + gameObj.name);
                }
            }
		    if ((e.type == EventType.MouseDown || m_isMouse1Down) && e.button == 2) 
		    {
                setMouse(1, false);
		    }

		    if ((e.type == EventType.MouseDown || m_isMouse2Down) && e.button == 1)
            {
                GUIUtility.hotControl = controlId;
                setMouse(1, false);
                setMouse(2, true);
                e.Use();

                Vector3 align = new Vector3(Mathf.Floor(mousePos.x / m_grid.m_gridWidth) * m_grid.m_gridWidth + (m_grid.m_gridWidth / 2.0f),
                                               Mathf.Floor(mousePos.y / m_grid.m_gridHeight) * m_grid.m_gridHeight + (m_grid.m_gridHeight / 2.0f), m_grid.m_currentLayerDepth);

                Transform transform = getTileTransformFromPosition(align);

                if (transform != null)
                {
                    DestroyImmediate(transform.gameObject);
                }
		    }
		    if ((e.type == EventType.MouseDown || m_isMouse2Down) && e.button == 2) 
		    {
                setMouse(2, false);
		    }
        }
    }

    /// <summary>
    /// Used to check whether there is a Tile at a given position.
    /// </summary>
    /// <param name="position">The given position to check.</param>
    /// <returns>If it finds a Tile it return a transform, else it return a null transform.</returns>
    Transform getTileTransformFromPosition(Vector3 position)
    {
        int i = 0;

        // .GetChild(1) is a specific child named "Tiles", which contains all Tiles
        while (i < m_grid.transform.GetChild(1).childCount)
        {
            if (m_grid.transform.GetChild(1).GetChild(i).tag == "Tile") 
            {
                Transform trans = m_grid.transform.GetChild(1).transform.GetChild(i);
                if (trans.position == position)
                {
                    return trans;
                }
            }

            i++;
        }

        return null;
    }
}
#endif