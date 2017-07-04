using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    // Enumeratior representing the desired tracking mode
    enum TrackingMode { Update, FixedUpdate }



    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [SerializeField] private TrackingMode trackingMode = TrackingMode.FixedUpdate;  // The desired tracking mode


    // ----------------------
    // --- Private values ---
    // ----------------------

    private Transform target;                                                       // The target transform of the camera
    


    // Update is called once per frame
	void Update () {
        // Unless the target is null, and this is the desired tracking mode
		if(target != null && trackingMode == TrackingMode.Update)
        {
            // Track the target
            TrackTarget();
        }
	}

    // FixedUpdate is called once per physics update
    void FixedUpdate()
    {
        // Unless the target is null, and this is the desired tracking mode
        if (target != null && trackingMode == TrackingMode.FixedUpdate)
        {
            // Track the target
            TrackTarget();
        }
    }

    // Tracks the target transform
    void TrackTarget()
    {
        // Get the new position
        Vector3 newPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Move the camera to the new position
        transform.position = newPosition;
    }

    // Public function that updates the target
    public void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
