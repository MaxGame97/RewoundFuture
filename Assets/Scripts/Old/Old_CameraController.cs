﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Old_CameraController : MonoBehaviour {

    Old_PlayerController m_player;
    const float m_zLayer = -10;

	void Start () {
        if (m_player = FindObjectOfType<Old_PlayerController>())
        {
            print("CAMERA, Player found");
        }
        else
        {
            print("CAMERA, Player not found");
        }
	}

    void Update () {
        UpdatePosition();
	}

    void UpdatePosition()
    {
        transform.position = m_player.transform.position;

        transform.position = new Vector3(transform.position.x, transform.position.y , m_zLayer);
    }
}
