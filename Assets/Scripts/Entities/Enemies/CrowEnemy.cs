using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowEnemy : MonoBehaviour {
    
    [Header("Movement parameters")]
    [SerializeField] private float movementSpeed;       // The movement speed that the crow will have (normally)

    [Header("Vision parameters")]
    [SerializeField] private float viewDistance;        // The crow's view distance

    [Header("Behaviour parameters")]
    [SerializeField] private float attackInterval;      // The interval (in seconds) between attacks
    [SerializeField] private Vector2 patrolArea;        // The area that the crow will be within, around the player

    [Header("Feather attack parameters")]
    [SerializeField] private GameObject featherPrefab;  // The feather prefab
    [SerializeField] private int featherAmount;         // The feather amount
    [SerializeField] private float featherSpread;       // The feather spread (in angles)

    [Header("Swooping attack parameters")]
    [SerializeField] private Vector2 swoopingOffset;    // Used to move the crow to a windup position
    [SerializeField] private float swoopingWindupTime;  // The windup time (in seconds) before the swooping attack
    [SerializeField] private float swoopingSpeed;       // The attack "rotation" speed (angles per second)

    private Transform playerTransform;  // Transform reference to the player

    private LayerMask visionBlock;      // The layer that blocks vision

    private State currentState;         // The crow's current state

    // Behaviour states, these are used to form the crow's attack patterns

    class IdleGroundState : State
    {
        // The crow instance
        CrowEnemy crow;

        public IdleGroundState(CrowEnemy crow)
        {
            this.crow = crow;
        }

        public override void Update()
        {
            // If the crow can see the player
            if (crow.CanSeePlayer())
            {
                // Transition to the alert state
                Transition(ref crow.currentState, new AlertState(crow));
            }
        }
    }

    class IdleAirState : State
    {
        // The crow instance
        CrowEnemy crow;

        // The target position that the crow is moving towards
        Vector3 patrolCenter;
        Vector3 targetPosition;

        public IdleAirState(CrowEnemy crow)
        {
            this.crow = crow;
        }

        public override void Enter()
        {
            patrolCenter = crow.transform.position;

            // Set a new target position
            SetNewTargetPosition();
        }

        public override void Update()
        {
            if (targetPosition.x - crow.transform.position.x < 0f)
                crow.FaceLeft();
            else
                crow.FaceRight();

            crow.MoveTowards(Vector3.Lerp(crow.transform.position, targetPosition, (crow.movementSpeed / 2) * Time.deltaTime));

            // If the crow has reached the target position, set a new one
            if (Vector3.Distance(crow.transform.position, targetPosition) < 0.25f)
                SetNewTargetPosition();

            // If the crow can see the player
            if (crow.CanSeePlayer())
            {
                // Transition to the alert state
                Transition(ref crow.currentState, new AlertState(crow));
            }
        }

        void SetNewTargetPosition()
        {
            Vector3 swag = Random.insideUnitCircle * (crow.patrolArea.y + crow.patrolArea.x);

            targetPosition = patrolCenter + swag;
        }
    }

    class AlertState : State
    {
        // The crow instance
        CrowEnemy crow;

        // Attack time, the crow transitions to an attack state after this
        float attackTime;

        float preferredXPosition;

        public AlertState(CrowEnemy crow)
        {
            this.crow = crow;
        }

        public override void Enter()
        {
            // Set the attack time
            attackTime = Time.time + crow.attackInterval;

            SetNewPreferredXPosition();
        }

        public override void Update()
        {
            // If the crow still can see the player
            if (crow.CanSeePlayer())
            {
                // Turn the crow towards the player
                crow.FacePlayer();

                Vector3 playerPosition = crow.playerTransform.position;
                Vector3 targetPosition = new Vector3();
                
                targetPosition.x = Mathf.Clamp(preferredXPosition, playerPosition.x - (crow.patrolArea.x / 2), playerPosition.x + (crow.patrolArea.x / 2));
                targetPosition.y = playerPosition.y + crow.patrolArea.y;

                crow.MoveTowards(Vector3.Lerp(crow.transform.position, targetPosition, crow.movementSpeed * Time.deltaTime));

                if (Mathf.Abs(playerPosition.x - preferredXPosition) > crow.patrolArea.x / 2)
                {
                    preferredXPosition = crow.transform.position.x;
                }
            }
            // If the crow has lost sight of the player
            else
            {
                // Transition to the idle air state
                Transition(ref crow.currentState, new IdleAirState(crow));
            }

            // If the attack time has expired
            if (attackTime <= Time.time)
            {
                // Create a temporary state representing the attack state
                State attackState;

                // Assign a random attack state to the temporary state
                if (Random.Range(0, 2) == 0)
                    attackState = new FeatherAttackState(crow);
                else
                    attackState = new SwoopingAttackState(crow);

                // Transition to the temporary state
                Transition(ref crow.currentState, attackState);
            }
        }

        void SetNewPreferredXPosition()
        {
            preferredXPosition = crow.transform.position.x + Random.Range(-crow.patrolArea.x / 2, crow.patrolArea.x / 2);
        }
    }

    class FeatherAttackState : State
    {
        // The crow instance
        CrowEnemy crow;

        public FeatherAttackState(CrowEnemy crow)
        {
            this.crow = crow;
        }

        public override void Enter()
        {
            // Loop for each feather to be instantiated
            for (int i = 0; i < crow.featherAmount; i++)
            {
                // Instantiate a feather
                GameObject feather = Instantiate(crow.featherPrefab);

                // Move the feather to the crow
                feather.transform.position = crow.transform.position;

                // Rotate the feather towards the player
                Vector3 direction = (crow.playerTransform.position - feather.transform.position).normalized;    // Get the direction towards the player
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;                                    // Get the angle towards that direction
                feather.transform.eulerAngles = new Vector3(0f, 0f, angle - 90f);                                       // Rotate the feather towards the angle

                // If more than one feather is spawned
                if (crow.featherAmount > 1)
                {
                    // Calculate the spread factor (between -1 and 1)
                    float spreadFactor = -1f + (i / ((float)crow.featherAmount - 1)) * 2f;

                    // Calculate the spread angle
                    float spreadAngle = (crow.featherSpread / 2) * spreadFactor;

                    // Rotate the feather based on the spread angle
                    feather.transform.Rotate(0f, 0f, spreadAngle);
                }
            }

            // Transition to the alert state
            Transition(ref crow.currentState, new AlertState(crow));
        }
    }

    class SwoopingAttackState : State
    {
        // The crow instance
        CrowEnemy crow;

        // Windup time, attack begins after this
        float windupTime;

        // Target position, the crow will move towards this position
        Vector3 targetPosition;

        bool swag;

        // The swooping attack's current angle
        float currentAngle;

        // The angle direction factor
        float angleDirection;

        // The attack size (width and height)
        Vector2 attackSize;

        // The attack's center position
        Vector3 attackCenter;

        public SwoopingAttackState(CrowEnemy crow)
        {
            this.crow = crow;
        }

        public override void Enter()
        {
            // Set the attack windup time
            windupTime = Time.time + crow.swoopingWindupTime;

            // Instantiate the target position
            targetPosition = Vector3.zero;

            // Set the target position's y-value
            targetPosition.y += crow.patrolArea.y + crow.swoopingOffset.y;

            // Set the target position's x-value based on which direction the crow is facing
            if (crow.GetComponent<SpriteRenderer>().flipX)
                targetPosition.x += (crow.patrolArea.x / 2) + crow.swoopingOffset.x;
            else
                targetPosition.x += -(crow.patrolArea.x / 2) - crow.swoopingOffset.x;

            swag = true;
        }

        public override void Update()
        {
            if(windupTime <= Time.time)
            {
                if (swag)
                {
                    // Get the relative position between the crow and the player
                    Vector2 relativePosition = crow.playerTransform.position - crow.transform.position;

                    // Set the attack size to the absolute values of the relative position
                    attackSize = new Vector2();
                    attackSize.x = Mathf.Abs(relativePosition.x);
                    attackSize.y = Mathf.Abs(relativePosition.y);

                    // Set the attack center to be directly above the player, on the y value that the crow is currently on
                    attackCenter = crow.playerTransform.position;
                    attackCenter.y = crow.transform.position.y;

                    // If the crow is to the left of the player
                    if (crow.playerTransform.position.x - crow.transform.position.x >= 0f)
                    {
                        currentAngle = 180f;    // Set the current angle to 180
                        angleDirection = 1f;    // Set the angle direction to 1 (counter-clockwise rotation)
                    }
                    // If the crow is to the right of the player
                    else
                    {
                        currentAngle = 0f;      // Set the current angle to 0
                        angleDirection = -1f;   // Set the angle direction to -1 (clockwise rotation)
                    }

                    swag = false;
                }

                // Perform swooping attack
                //Transition(ref crow.currentState, new AlertState(crow));
                // Increase the angle of the swooping attack
                currentAngle += crow.swoopingSpeed * Time.deltaTime * angleDirection;

                // Get the sine and cosine values from the current attack angle
                float sinFactor = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                float cosFactor = Mathf.Cos(currentAngle * Mathf.Deg2Rad);

                // Calculate the relative target position
                Vector3 relativeTargetPosition = new Vector3(cosFactor * attackSize.x, sinFactor * attackSize.y);

                // Move the crow according to the swooping attack's relative position
                crow.transform.position = attackCenter + relativeTargetPosition;

                if (crow.transform.position.y >= attackCenter.y)
                    // Transition back to the alert state when done
                    Transition(ref crow.currentState, new AlertState(crow));
            }
            else
            {
                // Move to attack position (WHERE IS THIS?)
                crow.MoveTowards(Vector3.Lerp(crow.transform.position, crow.playerTransform.position + targetPosition, crow.movementSpeed * Time.deltaTime));
            }
        }
    }

    // Use this for initialization
    void Start () {
        // Get the player's transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the vision block layer
        visionBlock = LayerMask.GetMask("VisionBlocker");

        // Instantiate the current state and transition to the idle ground state
        currentState = new State();
        currentState.Transition(ref currentState, new IdleGroundState(this));
    }

    // FixedUpdate is called once per phyiscs update
    void Update()
    {
        // Update the current state
        currentState.Update();
    }

    // Moves towards a position, with clamped velocity
    void MoveTowards(Vector3 targetPosition)
    {
        // Get the distance between the current position and the target position
        Vector3 distance = targetPosition - transform.position;

        // Calculate the movement, clamped to the max movement speed
        Vector3 movement = Vector2.ClampMagnitude(distance, movementSpeed * Time.deltaTime);

        // Translate the crow by the calculated movement
        transform.Translate(movement);
    }

    // Turns the crow to face the player
    void FacePlayer()
    {
        if (transform.position.x < playerTransform.position.x)
            FaceRight();
        else
            FaceLeft();
    }

    // Turns the crow to the right
    void FaceRight()
    {
        GetComponent<SpriteRenderer>().flipX = false;
    }

    // Turns the crow to the left
    void FaceLeft()
    {
        GetComponent<SpriteRenderer>().flipX = true;
    }

    // Checks if the enemy can see the player
    bool CanSeePlayer()
    {
        // If the player transform exists
        if (playerTransform != null)
        {
            // Get the distance between the enemy and the player
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // If the player is within the view distance of the enemy
            if (distance < viewDistance)
            {
                // Get the direction towards the player
                Vector2 viewDirection = (playerTransform.position - transform.position).normalized;

                // Perform a raycast against vision blocking colliders towards the player
                RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, viewDirection, distance, visionBlock);

                // If no vision blockers were hit
                if (raycastHit.collider == null)
                {
                    // Return true, the player has been spotted
                    return true;
                }
            }
        }

        // Return false if the either of the above statements were false, the player has not been spotted
        return false;
    }
}
