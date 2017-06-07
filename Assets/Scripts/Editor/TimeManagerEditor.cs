using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TimeManager))]
[CanEditMultipleObjects]
class TimeManagerEditor : Editor
{
    SerializedProperty m_timeZoneLeft;
    SerializedProperty m_timeZoneRight;

    void OnEnable()
    {
        m_timeZoneLeft = serializedObject.FindProperty("m_timeZoneLeft");
        m_timeZoneRight = serializedObject.FindProperty("m_timeZoneRight");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_timeZoneRight);
        EditorGUILayout.PropertyField(m_timeZoneLeft);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        var t = (target as TimeManager);

        EditorGUI.BeginChangeCheck();

        Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.mouseDown);

        Vector3 min = Handles.PositionHandle(t.m_timeZoneLeft, Quaternion.identity);
        Vector3 max = Handles.PositionHandle(t.m_timeZoneRight, Quaternion.identity);

        Handles.color = Color.red;
        Handles.DrawLine(m_timeZoneLeft.vector2Value, m_timeZoneRight.vector2Value);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move point");
            min.y = 0;
            max.y = 0;
            t.m_timeZoneLeft = min;
            t.m_timeZoneRight = max;
        }
    }
}