using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

using System.Collections;

public class GridWindow : EditorWindow {
    MapEditor m_grid;

    // Function for initiation of window
    public void init()
    {
        m_grid = (MapEditor)FindObjectOfType(typeof(MapEditor));
    }

    void OnGUI()
    {
        if (m_grid.m_displayGrid)
        {
            m_grid.m_gridWidth = createSlider("Width ", m_grid.m_gridWidth);
    
            m_grid.m_gridHeight = createSlider("Height", m_grid.m_gridHeight);

            m_grid.m_gridColor = EditorGUILayout.ColorField(m_grid.m_gridColor, GUILayout.Width(100));
        }
        SceneView.RepaintAll();
    }

    private float createSlider(string label, float sliderScale)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Grid " + label);
        sliderScale = EditorGUILayout.Slider(sliderScale, 0.5f, 10, null);

        GUILayout.EndHorizontal();

        return sliderScale;
    }

}
#endif