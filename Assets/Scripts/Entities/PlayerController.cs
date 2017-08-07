using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{



    // ------------------------
    // --- Inspector values ---
    // ------------------------

    [Header("Platformer values")]
    
    [SerializeField] private float movementSpeed;                   // The max speed that the player can move horizontally at
    [SerializeField] private float jumpingSpeed;                    // The initial jumping speed of the player
    [SerializeField] private float gravity;                         // The amount of acceleration that gravity causes when falling

    [Space(6f)]

    [SerializeField] private float maxSlopeSnapDistance;            // The max distance that the player will snap to slopes, when moving down one
    [SerializeField] [Range(0f, 90f)] private float maxSlopeAngle;  // The max slope angle that the player can walk up
    
    [SerializeField] private bool slopeNormalization = true;        // Specifies whether or not slope movement should be normalized or not (if walking up slopes should be slower)

    [Space(6f)]

    [SerializeField] private float terminalVelocity;                // The max falling speed of the player
    [SerializeField] private float jumpingTime;                     // How long the player can hold in the jump button before starting to fall (better control over jump height)

    [Space(6f)]

    [SerializeField] private float testingDistance;                 // The distance that testing functions (like IsGrounded() and HitsCeiling() uses to check defferent conditions

    [Header("2D collision settings")]
    
    [SerializeField] private LayerMask environmentLayer;            // The LayerMask of normal environment

    [Space(6f)]

    [SerializeField] private float sizeTolerance;                   // How big the size tolerance is (how much the raycasts are offset from the collider edges)
    [SerializeField] private float skinSize;                        // How deep the raycast skin is (how far within the collider the rays start)

    [Space(6f)]

    [SerializeField][Range(2, 15)] private int horizontalRays;      // The amount of horizontal rays to test
    [SerializeField][Range(2, 15)] private int verticalRays;        // The amount of vertical rays to test



    // ----------------------
    // --- Private values ---
    // ----------------------

    private Collider2D playerCollider;      // The player's 2D collider

    private Vector3 topLeftOffset;          // The top left corner offset of the player bounds
    private Vector3 topRightOffset;         // The top right corner offset of the player bounds
    private Vector3 bottomLeftOffset;       // The bottom left corner offset of the player bounds
    private Vector3 bottomRightOffset;      // The bottom right corner offset of the player bounds

    private Vector2 boundsSize;             // The player bounds's size

    private Vector2 currentVelocity;        // The player's current velocity
    private Vector2 deltaVelocity;          // The player's relative movement during one update

    private State currentState;             // The player's current state

    private bool previouslyGrounded;        // Specifies if the player was grounded during the previous iteration
    private bool hasDoubleJumped = false;   // If this is true, the player has performed a double jump
    private bool dropDown = false;          // If this is true, the player wants to drop down any one way platforms




    // -------------------------
    // --- Public properties ---
    // -------------------------



    public Vector2 CurrentVelocity { get { return currentVelocity; } }
    public Vector2 DeltaVelocity { get { return deltaVelocity; } }

    public bool DropDown { get { return dropDown; } }



    // --------------------------
    // --- State declarations ---
    // --------------------------

    private class GroundedState : State
    {
        PlayerController playerInstance;

        float previousVerticalInput;    // The previous vertical input value, used to make jumping more intuitive

        public GroundedState(PlayerController playerInstance)
        {
            this.playerInstance = playerInstance;
        }

        public override void Enter()
        {
            // Set the player's vertical velocity to 0
            playerInstance.currentVelocity.y = 0f;

            // Set the previous vertical input variable
            previousVerticalInput = Input.GetAxis("Vertical");
        }

        public override void FixedUpdate()
        {
            // Get the horizontal axis input
            float horizontalInput = Input.GetAxis("Horizontal");

            // Set the player's horizontal speed based on the horizontal input and the player's max movement speed
            playerInstance.currentVelocity.x = horizontalInput * playerInstance.movementSpeed;
            
            // If the player is no longer grounded
            if (!playerInstance.IsGrounded())
                // Transition to the falling state
                Transition(ref playerInstance.currentState, new FallingState(playerInstance));

            // If the player is not directly under a ceiling
            if (!playerInstance.HitsCeiling())
            {
                // If the vertical axis is above 0 AND above the previous vertical input
                if (Input.GetAxis("Vertical") > 0f && Input.GetAxis("Vertical") > previousVerticalInput)
                    // Transition to the jumping state
                    Transition(ref playerInstance.currentState, new JumpingState(playerInstance));
            }

            // If the vertical axis is below 0
            if (Input.GetAxis("Vertical") < 0f)
                // The player wants to drop down one way platforms
                playerInstance.dropDown = true;
            // If not
            else
                // The player doesn't want to drop down one way platforms
                playerInstance.dropDown = false;

            // Update the previous vertical input variable
            previousVerticalInput = Input.GetAxis("Vertical");
        }
    }

    private class JumpingState : State
    {
        PlayerController playerInstance;

        // The time when the player is forced into the falling state
        float jumpingTime;

        public JumpingState(PlayerController playerInstance)
        {
            this.playerInstance = playerInstance;
        }

        public override void Enter()
        {
            // Set the player's vertical velocity based on the player's jumping speed
            playerInstance.currentVelocity.y = playerInstance.jumpingSpeed;

            // Sets the jumping time based on the current time and the player's max jumping time
            jumpingTime = Time.time + playerInstance.jumpingTime;
        }

        public override void FixedUpdate()
        {
            // Get the horizontal axis input
            float horizontalInput = Input.GetAxis("Horizontal");

            // Set the player's horizontal speed based on the horizontal input and the player's max movement speed
            playerInstance.currentVelocity.x = horizontalInput * playerInstance.movementSpeed;

            // If the player has hit the ceiling
            if (playerInstance.HitsCeiling())
            {
                // Sets the player's vertical velocity to 0
                playerInstance.currentVelocity.y = 0f;

                // Transition to the falling state
                Transition(ref playerInstance.currentState, new FallingState(playerInstance));
            }

            // If the vertical axis is not above 0, or if the jumping time has been reached
            if (Input.GetAxis("Vertical") <= 0f || jumpingTime < Time.time)
                // Transition to the falling state
                Transition(ref playerInstance.currentState, new FallingState(playerInstance));
        }
    }

    private class FallingState : State
    {
        PlayerController playerInstance;

        float previousVerticalInput;    // The previous vertical input value, used to make jumping more intuitive

        public FallingState(PlayerController playerInstance)
        {
            this.playerInstance = playerInstance;

            // Set the previous vertical input variable
            previousVerticalInput = Input.GetAxis("Vertical");
        }

        public override void FixedUpdate()
        {
            // Get the horizontal axis input
            float horizontalInput = Input.GetAxis("Horizontal");

            // Set the player's horizontal speed based on the horizontal input and the player's max movement speed
            playerInstance.currentVelocity.x = horizontalInput * playerInstance.movementSpeed;

            // Decreases the player's vertical velocity based on the gravity value
            playerInstance.currentVelocity.y -= playerInstance.gravity;

            // If the player has hit the ceiling while moving up
            if (playerInstance.HitsCeiling() && playerInstance.currentVelocity.y > 0f)
                // Sets the player's vertical velocity to 0
                playerInstance.currentVelocity.y = 0f;

            // If the player's vertical velocity has exceeded the terminal velocity
            if (playerInstance.currentVelocity.y < -playerInstance.terminalVelocity)
                // Set the player's vertical velocity to the terminal velocity
                playerInstance.currentVelocity.y = -playerInstance.terminalVelocity;

            // If the player has NOT performed a double jump, and if the player is not directly under a ceiling
            if (!playerInstance.hasDoubleJumped && !playerInstance.HitsCeiling())
            {
                // If the vertical axis is above 0 AND above the previous vertical input
                if (Input.GetAxis("Vertical") > 0f && Input.GetAxis("Vertical") > previousVerticalInput)
                {
                    // Disable the player's double jump
                    playerInstance.hasDoubleJumped = true;

                    // Transition to the jumping state
                    Transition(ref playerInstance.currentState, new JumpingState(playerInstance));
                }
            }

            // If the player has reached the ground
            if (playerInstance.IsGrounded())
            {
                // Reset the player's double jump
                playerInstance.hasDoubleJumped = false;
                
                // Transition to the grounded state
                Transition(ref playerInstance.currentState, new GroundedState(playerInstance));
            }

            // The following part needs to be disabled, as the player can currently get stuck inside colliders if let go in mid air
            //
            // If a solution to this is found, it will be implemented instead

            /*
            // If the vertical axis is larger than or equal to 0
            if (Input.GetAxis("Vertical") >= 0f)
                // The player doesn't want to drop down one way platforms
                playerInstance.dropDown = false;
            */

            // Update the previous vertical input variable
            previousVerticalInput = Input.GetAxis("Vertical");
        }
    }



    // Use this for initialization
    void Start ()
    {
        // Set the player to not be destroyed on scene change
        DontDestroyOnLoad(gameObject);
        
        // Get the player's 2D collider
        playerCollider = GetComponent<Collider2D>();

        // Get the player's bounds
        Bounds boundsInstance = playerCollider.bounds;



        // ---------------------------------------
        // --- Calculate bounds corner offsets ---
        // ---------------------------------------

        topLeftOffset = new Vector3(-boundsInstance.extents.x, boundsInstance.extents.y, 0f);
        topLeftOffset += (Vector3)playerCollider.offset;

        topRightOffset = new Vector3(boundsInstance.extents.x, boundsInstance.extents.y, 0f);
        topRightOffset += (Vector3)playerCollider.offset;

        bottomLeftOffset = new Vector3(-boundsInstance.extents.x, -boundsInstance.extents.y, 0f);
        bottomLeftOffset += (Vector3)playerCollider.offset;

        bottomRightOffset = new Vector3(boundsInstance.extents.x, -boundsInstance.extents.y, 0f);
        bottomRightOffset += (Vector3)playerCollider.offset;



        // Get the bounds size
        boundsSize = GetComponent<Collider2D>().bounds.size;

        // Set the current velocity to zero
        currentVelocity = Vector2.zero;

        // Create an empty state as the current state
        currentState = new State();

        // Transition the current state to the falling state (runs entry logic)
        currentState.Transition(ref currentState, new FallingState(this));
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        deltaVelocity = (Vector2)transform.position;

        // Runs the current state's fixed update logic
        currentState.FixedUpdate();

        // Runs horizontal and vertical movement logic
        HorizontalMovement(currentVelocity.x * Time.deltaTime);
        VerticalMovement(currentVelocity.y * Time.deltaTime);

        // Runs the downward slope logic
        DownwardSlope();

        // Update the previously grounded bool
        previouslyGrounded = IsGrounded();
        
        deltaVelocity = (Vector2)transform.position - deltaVelocity;
    }

    // Performs a horizontal raycast check and returns all raycasts
    RaycastHit2D[] HorizontalRaycast(float distance, LayerMask layerMask)
    {
        // Instantiate an array of RaycastHit2D, the size of the amount of raycasts to be performed
        RaycastHit2D[] results = new RaycastHit2D[horizontalRays];

        // Do the following for all x in the results array
        for(int i = 0; i < results.Length; i++)
        {
            // Instantiate the current RaycastHit2D (so it is not uninitialized)
            results[i] = new RaycastHit2D();
        }

        // Do the following unless the distance is 0
        if(distance != 0)
        {
            // Instantiate a new Vector3 that represents the ray origin
            Vector3 rayOrigin = new Vector3();



            // -----------------------------------------
            // --- Preparing the ray origin position ---
            // -----------------------------------------

            // Set the ray origin position to the player's position
            rayOrigin = transform.position;

            // Use the correct position offset according to the direction of the raycast
            rayOrigin += distance < 0f ? bottomLeftOffset : bottomRightOffset;

            // Offsets the horizontal position according to the skin size
            // (This prevents rays from returning an incorrect value, if they are too short)
            rayOrigin.x += distance < 0f ? skinSize : -skinSize;

            // Offset the vertical position according to the raycast size tolerance
            // (This prevents rays from intersecting with paralell colliders, such as floors and ceilings)
            rayOrigin.y += sizeTolerance / 2;



            // Gets the direction based on the assigned distance
            float directionFactor = distance > 0f ? 1f : -1f;

            // Perform all rayscans
            for (int i = 0; i < horizontalRays; i++)
            {
                // Performs a rayscan based on the prepared values
                RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, new Vector2(directionFactor, 0f), Mathf.Abs(distance) + skinSize, layerMask);

                // Add the performed raycast to the results array
                results[i] = raycast;



                // ----------------------------------------------------------------------------------------------------------------------------
                // --- Debug draw rays and contact points, because this functionality isn't optimized, it should be disabled unless testing ---
                // ----------------------------------------------------------------------------------------------------------------------------

                /*
                // If the raycast was successful (hit something)
                if (raycast)
                {
                    // Draws a debug representation of this rayscan
                    Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(raycast.distance * directionFactor, 0f, 0f), Color.white);

                    // Draws a red cross representing this rayscan hit point
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(-0.05f, 0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(0.05f, 0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(-0.05f, -0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(0.05f, -0.05f), Color.red);

                    // Draws a green line representing the hit collider's normal
                    Debug.DrawLine(raycast.point, raycast.point + (raycast.normal * 0.5f), Color.green);
                }
                else
                {
                    // Draws a debug representation of this rayscan
                    Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(distance + (skinSize * directionFactor), 0f, 0f), Color.white);
                }
                */



                // Moves the ray origin position to the next position
                rayOrigin.y += (boundsSize.y - sizeTolerance) / (horizontalRays - 1);
            }
        }

        // Returns the result, this is either an unsuccessful raycast or the shortest successful raycast
        return results;
    }

    // Performs a horizontal raycast check and returns all raycasts
    RaycastHit2D[] VerticalRaycast(float distance, LayerMask layerMask)
    {
        // Instantiate an array of RaycastHit2D, the size of the amount of raycasts to be performed
        RaycastHit2D[] results = new RaycastHit2D[verticalRays];

        // Do the following for all x in the results array
        for (int i = 0; i < results.Length; i++)
        {
            // Instantiate the current RaycastHit2D (so it is not uninitialized)
            results[i] = new RaycastHit2D();
        }

        // Do the following unless the distance is 0
        if (distance != 0)
        {
            // Instantiate a new Vector3 that represents the ray origin
            Vector3 rayOrigin = new Vector3();



            // -----------------------------------------
            // --- Preparing the ray origin position ---
            // -----------------------------------------

            // Set the ray origin position to the player's position
            rayOrigin = transform.position;

            // Use the correct position offset according to the direction of the raycast
            rayOrigin += distance > 0f ? topLeftOffset : bottomLeftOffset;

            // Offset the horizontal position according to the raycast size tolerance
            // (This prevents rays from intersecting with paralell colliders, such as floors and ceilings)
            rayOrigin.x += sizeTolerance / 2;

            // Offsets the vertical position according to the skin size
            // (This prevents rays from returning an incorrect value, if they are too short)
            rayOrigin.y += distance > 0f ? -skinSize : skinSize;



            // Gets the direction based on the assigned distance
            float directionFactor = distance > 0f ? 1f : -1f;

            // Perform all rayscans
            for (int i = 0; i < verticalRays; i++)
            {
                // Performs a rayscan based on the prepared values
                RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, new Vector2(0f, directionFactor), Mathf.Abs(distance) + skinSize, layerMask);

                // Add the performed raycast to the results array
                results[i] = raycast;



                // ----------------------------------------------------------------------------------------------------------------------------
                // --- Debug draw rays and contact points, because this functionality isn't optimized, it should be disabled unless testing ---
                // ----------------------------------------------------------------------------------------------------------------------------

                /*
                // If the raycast was successful (hit something)
                if (raycast)
                {
                    // Draws a debug representation of this rayscan
                    Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(0f, raycast.distance * directionFactor, 0f), Color.white);

                    // Draws a red cross representation of this rayscan hit point
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(-0.05f, 0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(0.05f, 0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(-0.05f, -0.05f), Color.red);
                    Debug.DrawLine(raycast.point, raycast.point + new Vector2(0.05f, -0.05f), Color.red);

                    // Draws a green line representing the hit collider's normal
                    Debug.DrawLine(raycast.point, raycast.point + (raycast.normal * 0.5f), Color.green);
                }
                else
                {
                    // Draws a debug representation of this rayscan
                    Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(0f, distance + (skinSize * directionFactor), 0f), Color.white);
                }
                */



                // Moves the ray origin position to the next position
                rayOrigin.x += (boundsSize.x - sizeTolerance) / (verticalRays - 1);
            }
        }

        // Returns the result, this is either an unsuccessful raycast or the shortest successful raycast
        return results;
    }

    // Evaluates an array of RaycastHit2D and returns the shortest one (if there is one)
    RaycastHit2D EvaluateRaycasts(RaycastHit2D[] raycasts)
    {
        // Creates an empty RaycastHit2D representing the result of the evaluation
        RaycastHit2D result = new RaycastHit2D();

        // Evaluate all raycasts in the RaycastHit2D array
        for (int i = 0; i < raycasts.Length; i++)
        {
            // -----------------------------------------------------------------------------------
            // --- Evaluates the result of this raycast and compares it with the stored result ---
            // -----------------------------------------------------------------------------------

            // If any of the following statements are met:
            //
            // This raycast is successful, and is shorter than the stored result
            // or
            // The stored result was unsuccessful
            //
            // Set the stored result to this raycast

            if ((raycasts[i] && raycasts[i].distance < result.distance) || !result)
                result = raycasts[i];
        }

        // Returns the resulting RaycastHit2D
        return result;
    }

    // Handles horizontal movement
    void HorizontalMovement(float distance)
    {
        // Gets the direction based on the assigned distance
        float directionFactor = distance > 0f ? 1f : -1f;

        // Perform a horizontal raycast test and store the resulting RaycastHit2D array
        RaycastHit2D[] raycasts = HorizontalRaycast(distance, environmentLayer);

        // Attempt to find the shortest raycast by evaluating the performed raycasts
        RaycastHit2D shortestRaycast = EvaluateRaycasts(raycasts);

        // If the shortest raycast is also the foot raycast
        if (shortestRaycast == raycasts[0])
        {
            // Run the upward slope logic
            UpwardSlope(shortestRaycast);



            // --------------------------------------------------------------------------------------------------------------
            // --- Update the previous values as the player may have been moved upwards during the UpwardSlope() function ---
            // --------------------------------------------------------------------------------------------------------------

            // Update the performed raycasts by performing another horizontal raycast test and store the resulting RaycastHit2D array
            raycasts = HorizontalRaycast(distance, environmentLayer);

            // Update the shortest raycast by evaluating the performed raycasts
            shortestRaycast = EvaluateRaycasts(raycasts);
        }

        // If a shortest raycast has been assigned (collision)
        if (shortestRaycast)
        {
            // Instantiate a float representing the x value of the collision point
            float collisionPoint;



            // --------------------------------------------------------------------------------------------
            // --- Calculates the collision point based on the shortest raycast and the player's bounds ---
            // --------------------------------------------------------------------------------------------

            collisionPoint = shortestRaycast.point.x;
            collisionPoint += boundsSize.x / 2 * -directionFactor;
            collisionPoint -= GetComponent<Collider2D>().offset.x;


            
            // Move the player's position to the calculated collision point
            transform.position = new Vector3(collisionPoint, transform.position.y, transform.position.z);
        }
        // If no shortest raycast was assigned (no collision)
        else
        {
            // Translate the player normally based on the current velocity
            transform.Translate(distance, 0f, 0f);
        }
    }

    // Handles vertical movement
    void VerticalMovement(float distance)
    {
        // Gets the direction based on the assigned distance
        float directionFactor = distance > 0f ? 1f : -1f;

        // Perform a vertical raycast test and store the resulting RaycastHit2D array
        RaycastHit2D[] raycasts = VerticalRaycast(distance, environmentLayer);

        // Attempt to find the shortest raycast by evaluating the performed raycasts
        RaycastHit2D shortestRaycast = EvaluateRaycasts(raycasts);

        // If a shortest raycast has been assigned (collision)
        if (shortestRaycast)
        {
            // Instantiate a float representing the y value of the collision point
            float collisionPoint;



            // --------------------------------------------------------------------------------------------
            // --- Calculates the collision point based on the shortest raycast and the player's bounds ---
            // --------------------------------------------------------------------------------------------

            collisionPoint = shortestRaycast.point.y;
            collisionPoint += boundsSize.y / 2 * -directionFactor;
            collisionPoint -= GetComponent<Collider2D>().offset.y;
            
            // Move the player's position to the calculated collision point
            transform.position = new Vector3(transform.position.x, collisionPoint, transform.position.z);
        }
        // If no shortest raycast was assigned (no collision)
        else
        {
            // Translate the player normally based on the current velocity
            transform.Translate(0f, distance, 0f);
        }
    }

    // Handles slopes (upwards)
    void UpwardSlope(RaycastHit2D footRaycast)
    {
        // If the player is jumping or falling
        if (currentVelocity.y != 0f)
            // The player is jumping, abort
            return;

        // Get the slope angle
        float angle = Vector2.Angle(Vector2.up, footRaycast.normal);

        // If the slope angle is 90 degrees or larger than the max slope angle
        if (angle == 90f || angle > maxSlopeAngle)
            // The slope is too steep, abort
            return;

        // Calculate how far upwards the player needs to be corrected in order to move up the slope
        float verticalCorrection = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * currentVelocity.x * Time.deltaTime);

        // If slope normalization is enabled
        if (slopeNormalization)
            // Normalize the vertical correction depending on how steep the slope is (moving up steeper slopes is slower this way)
            verticalCorrection *= Mathf.Cos(angle * Mathf.Deg2Rad);

        // Perform vertical movement to correct the player upwards
        VerticalMovement(verticalCorrection);
    }

    // Handles slopes (downwards)
    void DownwardSlope()
    {
        // If the player is jumping or falling
        if (currentVelocity.y != 0f)
            // The player is not moving down a slope, abort
            return;
        
        // If the player is not grounded now, but used to be previously
        if (!IsGrounded() && previouslyGrounded)
        {
            // Perform a vertical raycast test and store the resulting RaycastHit2D array
            RaycastHit2D[] raycasts = VerticalRaycast(-maxSlopeSnapDistance, environmentLayer);

            // Check all performed raycasts
            for(int i = 0; i < raycasts.Length; i++)
            {
                // If any of them succeeded
                if (raycasts[i])
                {
                    // Perform vertical movement downward until the player has contact with the ground
                    VerticalMovement(-maxSlopeSnapDistance);

                    // Stop checking the performed raycasts
                    break;
                }
            }
        }
    }
    
    // Returns true if the player is on the ground (Keep in mind that this function has no safety check if the player is moving upwards)
    bool IsGrounded()
    {
        // Perform a vertical raycast test and store the resulting RaycastHit2D array
        RaycastHit2D[] raycasts = VerticalRaycast(-testingDistance, environmentLayer);

        // Check all the raycasts
        for (int i = 0; i < raycasts.Length; i++)
        {
            // If any of them were successful, the player is on the ground
            if (raycasts[i])
                // Return true, the player is on the ground
                return true;
        }

        // If the above check fails, return false, the player is NOT on the ground
        return false;
    }

    // Returns true if the player is directly underneath a ceiling collider (Keep in mind that this function has no safety check if the player is moving downwards)
    bool HitsCeiling()
    {
        // Perform a vertical raycast test and store the resulting RaycastHit2D array
        RaycastHit2D[] raycasts = VerticalRaycast(testingDistance, environmentLayer);

        // Check all the raycasts
        for (int i = 0; i < raycasts.Length; i++)
        {
            // If any of them were successful, return true, the player has hit the ceiling
            if (raycasts[i])
                return true;
        }

        // If the above check fails, return false, the player has NOT hit the ceiling
        return false;
    }
}
