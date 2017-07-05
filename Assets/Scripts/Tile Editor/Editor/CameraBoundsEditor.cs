using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraBounds))]
public class CameraBoundsEditor : Editor
{
    // OnInspectorGUI handles inspector functions and tools
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CameraBounds t = (CameraBounds)target;

        GUILayout.Space(6f);
        
        if(GUILayout.Button("Generate Rough Camera Bounds"))
        {
            t.GenerateRoughCameraBounds();
            
            SceneView.RepaintAll();

            EditorUtility.SetDirty(t);
        }
    }
}
