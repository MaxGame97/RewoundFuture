using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCamera : MonoBehaviour {
    
	// Use this for initialization
	void Start ()
    {
        // Update the camera's target to this transform
        GameObject.FindObjectOfType<CameraBehaviour>().GetComponent<CameraBehaviour>().UpdateTarget(transform);

        // Destroy this script
        Destroy(this);
	}
}
