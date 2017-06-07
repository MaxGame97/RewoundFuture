using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TimeElement))]
[CanEditMultipleObjects]
class TimeElementEditor : Editor
{
    SerializedProperty m_type;
    SerializedProperty m_bool;
    SerializedProperty m_int;
    SerializedProperty m_float;

    void OnEnable()
    {
        m_type = serializedObject.FindProperty("m_eType");
        m_bool = serializedObject.FindProperty("m_eActive");
        m_int = serializedObject.FindProperty("m_eInt");
        m_float = serializedObject.FindProperty("m_eFloat");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_type);
        EditorGUILayout.Space();
        switch (m_type.enumValueIndex)
        {
            case (int)ElementType.BOOL:
                EditorGUILayout.PropertyField(m_bool);
                break;
            case (int)ElementType.INT:
                EditorGUILayout.PropertyField(m_int);
                break;
            case (int)ElementType.FLOAT:
                EditorGUILayout.PropertyField(m_float);
                break;
            default:
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}