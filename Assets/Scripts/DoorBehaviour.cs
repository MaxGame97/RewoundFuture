using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBehaviour : MonoBehaviour {

    // Enumerator representing the door's direction
    public enum Direction { Left, Right, Up, Down }



    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [SerializeField] private string playerTag = "Player";   // The tag for the player object

    [Space(6f)]
    
    [SerializeField] private int iD;                        // The ID for this door
    [SerializeField] private Direction doorDirection;       // The direction of this door (may be updated if this is hard to work with)

    [Header("Level to load")]
    
    [SerializeField] private string sceneName;              // The scene to load when the player enters this door
    [SerializeField] private int doorID;                    // The door in the new scene that this door leads to



    // ----------------------
    // --- Private values ---
    // ----------------------

    GameObject playerInstance;                              // The persistent player instance

    float carryoverPosition;                                // Carryover value to correctly position the player after a transition



    // ---------------------
    // --- Public values ---
    // ---------------------

    public int ID { get { return iD; } }



    // Use this for initialization
    void Start()
    {
        // Find the persistent player instance
        playerInstance = GameObject.FindGameObjectWithTag(playerTag);
    }
    
    // OnTriggerExit2D is called when the collider stops being intersected by another collider (on the same collision layer)
    private void OnTriggerExit2D(Collider2D collision)
    {
        // ------------------------------
        // --- Preparatory procedures ---
        // ------------------------------
        
        // Check different conditions based on the door direction
        switch (doorDirection)
        {
            case Direction.Left:
                // If the object is to the right of the trigger, exit this function
                if (collision.transform.position.x > transform.position.x)
                    return;
                break;

            case Direction.Right:
                // If the object is to the left of the trigger, exit this function
                if (collision.transform.position.x < transform.position.x)
                    return;
                break;

            case Direction.Up:
                // If the object is below the trigger, exit this function
                if (collision.transform.position.y < transform.position.y)
                    return;
                break;

            case Direction.Down:
                // If the object is above the trigger, exit this function
                if (collision.transform.position.y > transform.position.y)
                    return;
                break;
        }

        // If the above code didn't exit this function, the player has exited the room and the transition will occur



        // If the player triggered this function
        if (collision.tag == playerTag)
        {
            // Set this GameObject not to be destroyed on scene load
            DontDestroyOnLoad(gameObject);
            // Tell the OnSceneLoad function to start listening for a scene to change
            SceneManager.sceneLoaded += OnSceneLoad;

            // ---------------------------------
            // --- Calculate carryover value ---
            // ---------------------------------

            // If the direction is either left or right
            if (doorDirection == Direction.Left || doorDirection == Direction.Right)
            {
                // Set the carryover position to the player's relative vertical position to the door
                carryoverPosition = playerInstance.transform.position.y - transform.position.y;
            }
            // If the direction is either up or down
            else
            {
                // Set the carryover position to the player's relative horizontal position to the door
                carryoverPosition = playerInstance.transform.position.x - transform.position.x;
            }



            // Load the new scene
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    // OnDrawGizmosSelected is called when the object is selected and gizmos are enabled
    void OnDrawGizmosSelected()
    {
        // ----------------------------------------------------
        // --- Draw a representation of the level load area ---
        // ----------------------------------------------------

        // Set the gizmos color to green (50% transparent)
        Gizmos.color = new Color(0, 1, 0, 0.5F);
        // Draw a cube at the level load object's position, using the local scale as its size
        Gizmos.DrawCube(transform.position, transform.localScale);
    }

    // OnSceneLoad is called when a new scene has finished loading
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // ------------------------------
        // --- Preparatory procedures ---
        // ------------------------------

        // Tell the OnSceneLoad function to stop listening for a scene to change
        SceneManager.sceneLoaded -= OnSceneLoad;

        // Remove the tag on this door
        tag = "Untagged";

        // Find all instances of player objects
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        // Check all found player instances
        for(int i = 0; i < players.Length; i++)
        {
            // If the current player instance is NOT the active player instance
            if(players[i] != playerInstance)
            {
                // Destroy it
                Destroy(players[i]);
            }
        }



        // -----------------------
        // --- Relocate player ---
        // -----------------------

        // Create a transform representing the door transitioning to's GameObject
        GameObject door = new GameObject();

        // Find all instances of door objects
        DoorBehaviour[] doors = GameObject.FindObjectsOfType<DoorBehaviour>();
        
        // Check all doors
        for (int i = 0; i < doors.Length; i++)
        {
            // If one of them matches the desired door ID
            if (doorID == doors[i].GetComponent<DoorBehaviour>().ID)
            {
                // Set the door GameObject to this door
                door = doors[i].gameObject;

                // Break out of the loop
                break;
            }
        }

        // Find the player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Move the player to the door position
        player.transform.position = door.transform.position;

        // Translates the player to the correct position depending on the door direction, based on the carryover position value
        switch (doorDirection)
        {
            case Direction.Left:
                // Translate the player based on the carryover position value
                player.transform.Translate(new Vector3(0f, carryoverPosition, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.y != door.GetComponent<Collider2D>().bounds.size.y)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Right:
                // Translate the player based on the carryover position value
                player.transform.Translate(new Vector3(0f, carryoverPosition, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.y != door.GetComponent<Collider2D>().bounds.size.y)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Up:
                // Translate the player based on the carryover position value
                player.transform.Translate(new Vector3(carryoverPosition, 0f, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.x != door.GetComponent<Collider2D>().bounds.size.x)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Down:
                // Translate the player based on the carryover position value
                player.transform.Translate(new Vector3(carryoverPosition, 0f, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.x != door.GetComponent<Collider2D>().bounds.size.x)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;
        }



        // -----------------------
        // --- Cleanup process ---
        // -----------------------

        // Add a GetCamera component to the player instance
        player.AddComponent<GetCamera>();

        // Destroy this GameObject
        Destroy(gameObject);
    }
}
