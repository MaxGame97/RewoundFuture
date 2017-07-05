using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    // Enumeratior representing the desired tracking mode
    enum TrackingMode { Update, FixedUpdate }

    // Enumeratior representing the desired camera size mode
    enum CameraSizeMode { Unlocked, Clamped }



    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [SerializeField] private TrackingMode trackingMode = TrackingMode.FixedUpdate;      // The desired tracking mode

    [Space(6f)]

    [SerializeField] private bool clampCameraPosition = true;                           // If true, will clamp the camera position to stay withing the designated camera bounds

    [SerializeField] private CameraSizeMode cameraSizeMode = CameraSizeMode.Clamped;    // The desired camera size mode

    [SerializeField] private float preferredCameraSize = 6f;                            // The preferred camera size (will keep this unless the camera bounds forces it to be smaller)



    // ----------------------
    // --- Private values ---
    // ----------------------

    private Transform target;                                                           // The target transform of the camera

    private Camera cameraInstance;                                                      // The camera instance

    private CameraBounds cameraBounds;                                                  // The camera bounds of the current scene

    private Vector2 orthographicSize;                                                   // The orthographic size of the camera



    // Use this for initialization
    void Start()
    {
        // Get the camera instance
        cameraInstance = GetComponent<Camera>();

        // If no camera has been found, cancel and deactivate this script
        if (cameraInstance == null)
        {
            // Log an error message
            Debug.LogError("Camera behaviour script assigned to object without camera, camera behaviour disabled");

            // Deactivate this script
            enabled = false;

            // Exit this function
            return;
        }

        // Find a camera bounds component, this will be used to clamp the camera's position
        cameraBounds = FindObjectOfType<CameraBounds>();

        // If no camera bounds has been found, log a warning
        if (cameraBounds == null)
            Debug.LogWarning("No camera bounds has been found, camera clamping functionality disabled");

        // If the cameraBounds do not appear to be set, log a warning and disable the camera clamping functionality
        if(cameraBounds.HorizontalBounds == Vector2.zero || cameraBounds.VerticalBounds == Vector2.zero)
        {
            // Set the cameraBounds to null (disable it)
            cameraBounds = null;

            // Log a warning message
            Debug.LogWarning("Camera bounds appear to not be configured, camera clamping functionality disabled");
        }

        // Run the camera clamp size function
        ClampCameraSize();

        // Update the camera's orthographic size
        UpdateOrthographicSize();
    }

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

        // Runs the clamp camera position function
        ClampCameraPosition();
    }

    // Clamps the camera position so it is not outside the camera bounds
    void ClampCameraPosition()
    {
        // If no camera bounds has been set, exit this function
        if (cameraBounds == null)
            return;

        // If the this camera is set to be clamped
        if (clampCameraPosition)
        {
            // Create a vector representing the new position
            Vector3 newPosition = transform.position;

            // If the camera bounds can contain the camera
            if(orthographicSize.x * 2 <= cameraBounds.HorizontalBounds.y - cameraBounds.HorizontalBounds.x)
            {
                // If the camera is outside the left camera bounds
                if (transform.position.x - orthographicSize.x < cameraBounds.HorizontalBounds.x)
                {
                    // Move it inside the camera bounds
                    newPosition.x = cameraBounds.HorizontalBounds.x + orthographicSize.x;
                }
                // Else if the camera is outside the right camera bounds
                else if (transform.position.x + orthographicSize.x > cameraBounds.HorizontalBounds.y)
                {
                    // Move it inside the camera bounds
                    newPosition.x = cameraBounds.HorizontalBounds.y - orthographicSize.x;
                }
            }
            // Else, if the camera bounds is too small for the camera
            else
            {
                // Center the camera to the camera bouncs (horizontally)
                newPosition.x = (cameraBounds.HorizontalBounds.x + cameraBounds.HorizontalBounds.y) / 2;
            }

            // If the camera bounds can contain the camera
            if (orthographicSize.y * 2 <= cameraBounds.VerticalBounds.y - cameraBounds.VerticalBounds.x)
            {
                // If the camera is outside the bottom camera bounds
                if (transform.position.y - orthographicSize.y < cameraBounds.VerticalBounds.x)
                {
                    // Move it inside the camera bounds
                    newPosition.y = cameraBounds.VerticalBounds.x + orthographicSize.y;
                }
                // Else if the camera is outside the top camera bounds
                else if (transform.position.y + orthographicSize.y > cameraBounds.VerticalBounds.y)
                {
                    // Move it inside the camera bounds
                    newPosition.y = cameraBounds.VerticalBounds.y - orthographicSize.y;
                }
            }
            // Else, if the camera bounds is too small for the camera
            else
            {
                // Center the camera to the camera bouncs (horizontally)
                newPosition.y = (cameraBounds.VerticalBounds.x + cameraBounds.VerticalBounds.y) / 2;
            }

            // Update the camera position based on the above code
            transform.position = newPosition;
        }
    }

    // Clamps the camera size if the camera bounds cannot contain it
    void ClampCameraSize()
    {
        // If no camera bounds has been set, exit this function
        if (cameraBounds == null)
            return;
        
        // If the camera size mode is set to be clamped
        if (cameraSizeMode == CameraSizeMode.Clamped)
        {
            // Get the preferred camera height
            float cameraHeight = preferredCameraSize;

            // Get the camera bounds height
            float cameraBoundsHeight = (cameraBounds.VerticalBounds.y - cameraBounds.VerticalBounds.x) / 2;

            // Set the camera size to the lowers of these two
            cameraInstance.orthographicSize = cameraBoundsHeight < cameraHeight ? cameraBoundsHeight : cameraHeight;
        }
    }

    // Update the orthographic camera size
    void UpdateOrthographicSize()
    {
        orthographicSize.y = cameraInstance.orthographicSize;
        orthographicSize.x = orthographicSize.y * cameraInstance.aspect;
    }

    // Public function that updates the target
    public void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
