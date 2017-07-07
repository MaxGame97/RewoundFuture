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

    GameObject playerInstance;                              // The player instance

    Collider2D collider2d;                                  // This object's collider

    float playerHeight;                                     // The height of the player (extents)
    float height;                                           // The height of this object's collider (extents)



	// Use this for initialization
	void Start () {
        // Find the player instance
        playerInstance = GameObject.FindGameObjectWithTag(playerTag);

        // Get the player's height
        playerHeight = playerInstance.GetComponent<Collider2D>().bounds.extents.y;

        // Get this object's collider
        collider2d = GetComponent<Collider2D>();

        // Get this object's height
        height = collider2d.bounds.extents.y;
	}

    // FixedUpdate is called once per phyiscs update
    void FixedUpdate () {
        // If the player is above this object (the player's bottom relative to this object's top)
        if (transform.position.y + height <= playerInstance.transform.position.y - playerHeight)
            // Enable this object's collider
            collider2d.enabled = true;
        // If the player is below this object
        else
            // Disable this object's collider
            collider2d.enabled = false;
	}
}
