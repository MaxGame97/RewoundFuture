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

    [Header("Fade values")]

    [SerializeField] private GameObject fadeOutPrefab;      // Prefab used for the fade out effect
    [SerializeField] private Texture fadeTexture;           // The texture used to fade out the screen
    [SerializeField] private float fadeSpeed;               // The speed of the fade out effect (in seconds, larger takes longer time)



    // ----------------------
    // --- Private values ---
    // ----------------------

    GameObject playerInstance;                              // The persistent player instance

    float fadeAlpha;                                        // The current alpha for the screen fade effect
    float carryoverPosition;                                // Carryover value to correctly position the player after a transition



    // -------------------------
    // --- Public properties ---
    // -------------------------

    public int ID { get { return iD; } }



    // Use this for initialization
    void Start()
    {
        // Find the persistent player instance
        playerInstance = GameObject.FindGameObjectWithTag(playerTag);

        // Set the fade alpha to 0
        fadeAlpha = 0f;
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
        if (collision.gameObject == playerInstance)
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



            // Set the player instance to be inactive
            playerInstance.SetActive(false);

            // Start the fade and load coroutine
            StartCoroutine(FadeInAndLoad());
        }
    }

    private void OnGUI()
    {
        GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeTexture); 
    }

    // OnDrawGizmosSelected is called when the object is selected and gizmos are enabled
    void OnDrawGizmosSelected()
    {
        // ----------------------------------------------------
        // --- Draw a representation of the level load area ---
        // ----------------------------------------------------

        // Get the size that the cube should be drawn with, from the collider's bounds
        Vector3 cubeSize = GetComponent<Collider2D>().bounds.size;

        // Set the gizmos color to green (50% transparent)
        Gizmos.color = new Color(0, 1, 0, 0.5F);

        // Draw a cube at the level load object's position, using the local scale as its size
        Gizmos.DrawCube(transform.position, cubeSize);
    }

    // Increases the fade alpha and when it reaches 1, loads the scene
    IEnumerator FadeInAndLoad()
    {
        // While the fade alpha is smaller than 1
        while (fadeAlpha < 1f)
        {
            // Increase the fade alpha based on the fade speed
            fadeAlpha += (1 / fadeSpeed) * Time.deltaTime;

            // If the fade alpha is larger than 1
            if (fadeAlpha > 1f)
                // Set it to 1
                fadeAlpha = 1f;

            yield return null;
        }

        // Set the player instance to be active
        playerInstance.SetActive(true);

        // Load the new scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    
    // OnSceneLoad is called when a new scene has finished loading
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // ------------------------------
        // --- Preparatory procedures ---
        // ------------------------------

        // Tell the OnSceneLoad function to stop listening for a scene to change
        SceneManager.sceneLoaded -= OnSceneLoad;

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

        // Create a GameObject that represent the target door
        GameObject door = new GameObject();

        // Find all instances of door objects
        DoorBehaviour[] doors = FindObjectsOfType<DoorBehaviour>();
        
        // Check all doors
        for (int i = 0; i < doors.Length; i++)
        {
            // If one of them matches the desired door ID, as long as it isn't this door
            if (doorID == doors[i].GetComponent<DoorBehaviour>().ID && doors[i] != this)
            {
                // Set the target door to this door
                door = doors[i].gameObject;

                // Break out of the loop
                break;
            }
        }

        // Move the player to the door position
        playerInstance.transform.position = door.transform.position;

        // Translates the player to the correct position depending on the door direction, based on the carryover position value
        switch (doorDirection)
        {
            case Direction.Left:
                // Translate the player based on the carryover position value
                playerInstance.transform.Translate(new Vector3(0f, carryoverPosition, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.y != door.GetComponent<Collider2D>().bounds.size.y)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Right:
                // Translate the player based on the carryover position value
                playerInstance.transform.Translate(new Vector3(0f, carryoverPosition, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.y != door.GetComponent<Collider2D>().bounds.size.y)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Up:
                // Translate the player based on the carryover position value
                playerInstance.transform.Translate(new Vector3(carryoverPosition, 0f, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.x != door.GetComponent<Collider2D>().bounds.size.x)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;

            case Direction.Down:
                // Translate the player based on the carryover position value
                playerInstance.transform.Translate(new Vector3(carryoverPosition, 0f, 0f));

                // Log a warning if the two connected door's sizes aren't matching
                if (GetComponent<Collider2D>().bounds.size.x != door.GetComponent<Collider2D>().bounds.size.x)
                    Debug.LogWarning("Door size mismatch, might cause issues.");

                break;
        }



        // -----------------------
        // --- Cleanup process ---
        // -----------------------

        // Add a GetCamera component to the player instance
        playerInstance.AddComponent<GetCamera>();

        // Instantiates the door fade out prefab
        DoorFadeOut fadeOutInstance = Instantiate(fadeOutPrefab).GetComponent<DoorFadeOut>();

        // Sets the door fade out instance's values to the ones assigned in this script
        fadeOutInstance.FadeSpeed = fadeSpeed;

        // Destroys this GameObject
        Destroy(gameObject);
    }
}
