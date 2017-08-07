using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour {

    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [SerializeField] private string playerTag = "Player";   // The tag for the player object



    // ----------------------
    // --- Private values ---
    // ----------------------

    private GameObject playerInstance;          // The player instance

    private PlayerController playerController;  // The player's PlayerController instance

    private Collider2D playerCollider;          // The player instance's collider
    private Collider2D platformCollider;        // This object's collider

    private LayerMask startingLayer;            // The starting layer of this object
    private LayerMask environmentLayer;         // The LayerMask of this object's starting layer

    private float maxDistance;



    // Use this for initialization
    void Start () {
        // Find the player instance
        playerInstance = GameObject.FindGameObjectWithTag(playerTag);

        // Get the player's PlayerController instance
        playerController = playerInstance.GetComponent<PlayerController>();

        // Get the player instance's collider
        playerCollider = playerInstance.GetComponent<Collider2D>();

        // Get this object's collider
        platformCollider = GetComponent<Collider2D>();

        // Store the starting layer
        startingLayer = gameObject.layer;

        // Calculate the current LayerMask by bitshifting the value of the current layer
        environmentLayer = 1 << startingLayer.value;

        // Calculate the minimum distance for the one way platform to run its logic, by finding the the player's full collider hypothenuse
        maxDistance = Mathf.Sqrt(Mathf.Pow(playerCollider.bounds.size.x, 2) + Mathf.Pow(playerCollider.bounds.size.y, 2));
	}

    // LateUpdate is called after all other update sequences
    void LateUpdate ()
    {
        // Do the following only if the player is near (for optimization purposes)
        //
        // Doing this is a bit risky, and may cause problems if the player is moving INCREDIBLY fast
        //
        // However, the consequences outweigh the risks here, as the performance will drastically
        // improve when using this method. If a better one is found, it can be used instead.

        if(Vector3.Distance(transform.position, playerInstance.transform.position) <= maxDistance)
        {
            Debug.Log("test " + Time.time);
            // Create a float representing the player's velocity angle
            float angle = 0f;

            // If the player is currently moving
            if (playerController.CurrentVelocity != Vector2.zero)
            {
                // Calculate the player's velocity angle, comparing it to a downwards vector
                angle = Vector2.Angle(Vector2.down, playerController.CurrentVelocity.normalized);
            }

            // If the player's velocity angle is lower than 90 degrees, and if the player is not attempting to drop down any one way platforms
            if (angle <= 90f && !playerController.DropDown)
            {
                // If the player collider and this collider are not intersecting
                if (!Physics2D.IsTouching(platformCollider, playerCollider))
                {
                    // Revert this object's layer to the starting layer
                    gameObject.layer = startingLayer;

                    // If the player's lowest point (feet) is below this object
                    if (playerInstance.transform.position.y - playerCollider.bounds.extents.y + playerCollider.offset.y < transform.position.y)
                    {
                        // Perform a raycast against THIS collider from the PLAYER'S position
                        RaycastHit2D raycast = Physics2D.Raycast(new Vector2(playerInstance.transform.position.x, transform.position.y), new Vector2(transform.position.x - playerInstance.transform.position.x, 0f).normalized, Mathf.Infinity, environmentLayer);

                        // Evaluate the raycast's returned normal, if the angle is larger than or equal to 90 degrees
                        if (Vector2.Angle(Vector2.up, raycast.normal) >= 90f)
                        {
                            // Change this object's layer to the ignore raycasts layer
                            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        }
                    }
                }
            }
            // If the player's velocity angle is larger than or equal to than 90 degrees, or if the player is attempting to drop down any one way platforms
            else
            {
                // Change this object's layer to the ignore raycasts layer
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }
}
