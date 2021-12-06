using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    #region Private Variables

    private Rect rect;
    private GUIStyle style;

    #endregion
    public GUIStyle nodeStyle
    {
        get { return style;  }
        set { style = value; }
    }

    public Node(Vector2 position, float width, float height, GUIStyle defaultStyle)
    {
        rect = new Rect(position.x, position.y, width, height);
        nodeStyle = defaultStyle;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        GUI.Box(rect, "", nodeStyle);
    }
    public void SetStyle(GUIStyle NodeStyle)
    {
        nodeStyle = NodeStyle;
    }
}
