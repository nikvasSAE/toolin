using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    #region Private Variables

    private int row, column;    
    private GameObject part;
    private GUIStyle style;

    #endregion

    public string partName = "Empty";

    public int Row
    {
        get { return row; }
        set { row = value; }
    }
    public int Column
    {
        get { return column; }
        set { column = value; }
    }
    public GameObject Part
    {
        get { return part; }
        set { part = value; }
    }    
    public GUIStyle Style
    {
        get { return style; }
        set { style = value; }
    }
}
