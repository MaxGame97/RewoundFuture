using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

interface ITimeManipulation
{
    float GetTimeScale();
}

public class Old_TimeManager : MonoBehaviour, ITimeManipulation {

    public float m_timeScale = 0;
    public Vector2 m_timeZoneLeft = Vector2.left;
    public Vector2 m_timeZoneRight = Vector2.right;

    Transform m_playerPos;

	void Start () {
		if (FindObjectsOfType<Old_TimeManager>().Length > 1)
        {
            Debug.LogWarning("WARNING: More than one Time Manager in scene");
        }

        if (FindObjectOfType<Old_PlayerController>())
        {
            if (FindObjectsOfType<Old_PlayerController>().Length > 1)
            {
                Debug.LogWarning("AAAAAAAAA MORE THAN ONE PLAYER IN SCENE");
            }
            else
            {
                m_playerPos = FindObjectOfType<Old_PlayerController>().transform;
            }
        }
	}

    void UpdateTimeScale()
    {
        float tDist = m_timeZoneRight.x - m_timeZoneLeft.x;
        float playerDist = m_playerPos.transform.position.x - m_timeZoneLeft.x;

        m_timeScale = playerDist / tDist;
    }

	void Update () {
		
	}

    public float GetTimeScale()
    {
        return m_timeScale;
    }
}

