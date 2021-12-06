using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridMapCreator : EditorWindow
{
    #region Private Variables

    private StyleManager styleManager;

    private List<List<Node>> nodes;
    private List<List<PartsScript>> parts;

    private GUIStyle currentStyle;
    private GUIStyle empty;
    private Vector2 nodePos;
    private Vector2 offset;
    private Vector2 drag;
    private Rect MenuBar;
    private Rect MenuBar2;
    private GameObject mapGO;

    private bool isErasing;
    private int buttonWidth = 80;
    private int mapWidth = 16;
    private int mapHeight = 9;
    private int boundsX = 315;
    private int boundsY = 175;

    #endregion

    #region SetUp

    /// <summary>
    /// Setting up access to the tool in GUI Window
    /// </summary>
    [MenuItem("Window/Level Editor Tool")]
    private static void OpenWindow()
    {
        GridMapCreator window = GetWindow<GridMapCreator>();
        window.titleContent = new GUIContent("Level Editor Tool");        
    }

    private void OnEnable()
    {
        SetupStyles();
        SetUpNodesAndParts();
        SetUpMap();
    }

    /// <summary>
    /// Sets up the map game object that will hold all child objects once instantiated. 
    /// </summary>
    private void SetUpMap()
    {
        try
        {
            mapGO = GameObject.FindGameObjectWithTag("Map");
            RestoreTheMap(mapGO);
        }
        catch (Exception e)
        {
            throw e;
        }
        if (mapGO == null)
        {
            mapGO = new GameObject("Map");
            mapGO.tag = "Map";
        }
    }

    /// <summary>
    /// This function is used to show the instantiated game objects on the map, and corresponds with the map on GUI
    /// </summary>
    /// <param name="mapGO">The game object of the map, the parent that holds the child objects once instantiated</param>
    private void RestoreTheMap(GameObject mapGO)
    {
        if (mapGO.transform.childCount > 0)
        {
            for (int i = 0; i < mapGO.transform.childCount; i++)
            {
                //Gets the row index of the child object @ index
                int row = mapGO.transform.GetChild(i).GetComponent<PartsScript>().Row;
                //Gets the column index of the child object @ index
                int col = mapGO.transform.GetChild(i).GetComponent<PartsScript>().Column;
                //Gets the style from the child object @ index and sets it to mapStyle 
                GUIStyle mapStyle = mapGO.transform.GetChild(i).GetComponent<PartsScript>().Style;
                //Sets currentNode to the row and column just obtained from child @ index  
                var currentNode = nodes[row][col];
                //Set the current nodes style 
                currentNode.SetStyle(mapStyle);
                //Get the parts script of the child parts    
                var currentPartsComponent = parts[row][col] = mapGO.transform.GetChild(i).GetComponent<PartsScript>();
                //Set the game object @ index to the part
                currentPartsComponent.Part = mapGO.transform.GetChild(i).gameObject;
                //Set the game object @ index to the name
                currentPartsComponent.partName = mapGO.transform.GetChild(i).name;
                //Set the game object @ index to the column
                currentPartsComponent.Column = col;
                //Set the game object @ index to the row
                currentPartsComponent.Row = row;
            }
        }
    }

    /// <summary>
    /// Setting up the styles to correspond with the buttons and the icons set in the inspector 
    /// </summary>
    private void SetupStyles()
    {
        // Sets up the buttons 
        try
        {
            styleManager = GameObject.FindGameObjectWithTag("StyleManager").GetComponent<StyleManager>();
            for (int i = 0; i < styleManager.buttonStyles.Length; i++)
            {
                styleManager.buttonStyles[i].NodeStyle = new GUIStyle();
                styleManager.buttonStyles[i].NodeStyle.normal.background = styleManager.buttonStyles[i].Icon;
            }
        }
        catch (Exception e)
        {
            throw e;
        }
        empty = styleManager.buttonStyles[0].NodeStyle;
        currentStyle = styleManager.buttonStyles[1].NodeStyle;
    }

    /// <summary>
    /// Setting up the nodes and parts using 2D lists 
    /// </summary>
    private void SetUpNodesAndParts()
    {
        nodes = new List<List<Node>>();
        parts = new List<List<PartsScript>>();
        for (int i = 0; i < 20; i++)
        {
            nodes.Add(new List<Node>());
            parts.Add(new List<PartsScript>());
            for (int j = 0; j < 10; j++)
            {
                nodePos.Set(i * 20, j * 20);
                nodes[i].Add(new Node(nodePos, 20, 20, empty));
                parts[i].Add(null);
            }
        }
    }

    #endregion

    #region GUI

    /// <summary>
    /// Calls the specified functions when GUI opened 
    /// </summary>
    private void OnGUI()
    {
        DrawGrid();
        DrawNodes();
        DrawObjMenuBar();
        DrawResetMenuBar();
        processNodes(Event.current);
        ProcessGrid(Event.current);
        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Drawing the Menu Bar on the Tool Window that allows users to select a tile to paint onto the map.
    /// </summary>
    private void DrawObjMenuBar()
    {
        MenuBar = new Rect(0, 0, position.width, 20);
        GUILayout.BeginArea(MenuBar,EditorStyles.toolbar);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < styleManager.buttonStyles.Length; i++)
        {
            if (GUILayout.Toggle((currentStyle == styleManager.buttonStyles[i].NodeStyle), new GUIContent(styleManager.buttonStyles[i].ButtonTex), EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
            {
                currentStyle = styleManager.buttonStyles[i].NodeStyle;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Currently only set up to create a reset button + button functionality  
    /// </summary>
    private void DrawResetMenuBar()
    {
        MenuBar2 = new Rect(0, 21.5f, position.width, 20);
        GUILayout.BeginArea(MenuBar2, EditorStyles.toolbar);
        GUILayout.BeginHorizontal(); 

        if (GUILayout.Button(new GUIContent("Reset"), EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
        {            
            Debug.Log("Clicked");

            int childs = mapGO.transform.childCount;
            Debug.Log(childs);

            for (int i = childs - 1; i >= 0; i--)
            {
                DestroyImmediate(mapGO.transform.GetChild(i).gameObject);
            }
            GUI.changed = true;
            this.Close();            
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    #endregion

    #region Map/Nodes

    /// <summary>
    /// Assesses where the mouse is positioned so that anything outside of the map will register as moving the grid/map around, and anything inside the map will register as painting or erasing nodes. 
    /// </summary>
    /// <param name="e">Mouse click event - on Mouse down/ mousedrag </param>
    private void processNodes(Event e)
    {        
        int Row = (int)((e.mousePosition.x - offset.x)/ 20);
        int Column = (int)((e.mousePosition.y - offset.y) / 20);       
        
        if ((e.mousePosition.x - offset.x) < 0 || (e.mousePosition.x - offset.x) > boundsX || (e.mousePosition.y - offset.y) < 0 || (e.mousePosition.y - offset.y) > boundsY)
        { }
        else
        {
            if (e.type == EventType.MouseDown)
            {
                if (nodes[Row][Column].nodeStyle.normal.background.name == "Empty")
                {
                    isErasing = false;
                }
                else
                {
                    isErasing = true;
                }
                PaintNodes(Row, Column);
            }
            if (e.type == EventType.MouseDrag)
            {
                PaintNodes(Row, Column);
                e.Use();
            }
        }               
    }

    /// <summary>
    /// Paints the node to the toggled buttons style and instantiates child objects of our Map Game Object that correspond with painted style. 
    /// </summary>
    /// <param name="Row">Takes in an integer on the x axis of the grid</param>
    /// <param name="Column">Takes in an integer on the y axis of the grid</param>`
    private void PaintNodes(int Row, int Column)
    {

        if (isErasing)
        {
            if (parts[Row][Column] != null)
            {
                nodes[Row][Column].SetStyle(empty);
                DestroyImmediate(parts[Row][Column].gameObject);
                GUI.changed = true;
            }
            parts[Row][Column] = null;
        }
        else
        {
            if (parts[Row][Column] == null)
            {
                nodes[Row][Column].SetStyle(currentStyle);
                GameObject gameObject = Instantiate(Resources.Load("MapParts/" + currentStyle.normal.background.name)) as GameObject;
                gameObject.name = currentStyle.normal.background.name;
                gameObject.transform.position = new Vector3(Column * 10, 0, Row * 10) + Vector3.forward * 5 + Vector3.right * 5;
                gameObject.transform.parent = mapGO.transform;
                parts[Row][Column] = gameObject.GetComponent<PartsScript>();
                parts[Row][Column].Part = gameObject;
                parts[Row][Column].name = gameObject.name;
                parts[Row][Column].Row = Row;
                parts[Row][Column].Column = Column;
                parts[Row][Column].Style = currentStyle;

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Drawing the nodes of the map on the GUI grid
    /// </summary>
    private void DrawNodes()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                nodes[i][j].Draw();
            }
        }       
    }

    #endregion

    #region Background Grid

    /// <summary>
    /// Create drag functionality so background grid is movable 
    /// </summary>
    /// <param name="e">The unity gui event that determines if mouse drag is being called or not</param>
    private void ProcessGrid(Event e)
    {
        drag = Vector2.zero;
        switch (e.type)
        {            
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnMouseDrag(e.delta);
                }
                break;            
            default:
                break;
        }
    }

    /// <summary>
    /// Called on mouse drag event 
    /// </summary>
    /// <param name="delta"> holds the amount the mouse has moved</param>
    private void OnMouseDrag(Vector2 delta)
    {
        drag = delta;

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                nodes[i][j].Drag(delta);
            }
        }        
        GUI.changed = true;
    }

    /// <summary>
    /// Draws lines along the x and along the y axis to create the background grid. 
    /// </summary>
    private void DrawGrid()
    {
        int widthDivider = Mathf.CeilToInt(position.width / 20);
        int heightDivider = Mathf.CeilToInt(position.height / 20);

        Handles.BeginGUI();
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        offset += drag;
        Vector3 newOffset = new Vector3(offset.x % 20, offset.y % 20, 0);

        for (int i = 0; i < widthDivider; i++)
        {
            Handles.DrawLine(new Vector3(20 * i, -20, 0) + newOffset, new Vector3(20 * i, position.height, 0) + newOffset);
        }

        for (int i = 0; i < heightDivider; i++)
        {
            Handles.DrawLine(new Vector3(-20, 20 * i, 0) + newOffset, new Vector3(position.width, 20 * i, 0) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    #endregion
}
