using UnityEngine;
using System.Collections;

[System.Serializable]
public struct MapSet
{
    public string m_name;
    public TileSet m_tileset;
}

public class MapEditor : MonoBehaviour {
    public bool m_displayGrid = true;
    public bool m_displayTileSelector = false;

    public float m_gridWidth = 1.0f;
    public float m_gridHeight = 1.0f;
    public Color m_gridColor = Color.white;

    public Transform m_tilePrefab;
    public TileSet m_tileset;

    public MapSet[] m_mapSets = new MapSet[1];
    public int m_currentSet = 0;
    public int m_currentLayerDepth = 0;

    public bool m_mouseOffsetX;
    public bool m_randomOffset;
    public float m_offsetMax;
    public bool m_flipPrefab;
    public bool m_flipRandom;

    public bool m_helpBoxes = true;

	// Is used to draw Gizmos in the Scene
	void  OnDrawGizmos() 
    {
        if (this.m_displayGrid)
        {
            Vector3 position = Camera.current.transform.position;
            Gizmos.color = this.m_gridColor;

            for (float y = position.y - 800.0f; y < position.y + 800.0f; y += this.m_gridHeight)
            {
                Gizmos.DrawLine(new Vector3(-1000000, Mathf.Floor(y / this.m_gridHeight) * this.m_gridHeight, 0),
                                new Vector3(1000000, Mathf.Floor(y / this.m_gridHeight) * this.m_gridHeight, 0));
            }

            for (float x = position.x - 1200.0f; x < position.x + 1200; x += this.m_gridWidth)
            {
                Gizmos.DrawLine(new Vector3(Mathf.Floor(x / this.m_gridWidth) * this.m_gridWidth, -1000000, 0),
                                 new Vector3(Mathf.Floor(x / this.m_gridWidth) * this.m_gridWidth, 1000000, 0));
            }
        }
	}
}
