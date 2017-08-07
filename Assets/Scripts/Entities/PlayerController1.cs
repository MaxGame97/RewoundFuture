using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController1 : MonoBehaviour {
    
    [Header("2D collision settings")]

    [SerializeField] private LayerMask environmentLayer;

    [SerializeField] private float sizeTolerance;
    [SerializeField] private float skinSize;

    [SerializeField][Range(2, 15)] private int horizontalRays = 5;
    [SerializeField][Range(2, 15)] private int verticalRays = 5;

    Vector3 topLeftOffset;

    Vector2 boundsSize;

    Vector2 speedlol;

    State currentState;

    // ---------

    private class GroundedState : State
    {
        PlayerController1 playerInstance;

        public GroundedState(PlayerController1 playerInstance)
        {
            this.playerInstance = playerInstance;
        }

        public override void Enter()
        {
            playerInstance.speedlol.y = -1f;
        }

        public override void FixedUpdate()
        {
            float input = 0f;

            input = Input.GetAxis("Horizontal");

            playerInstance.speedlol.x = input * 5f;

            if(Input.GetAxis("Vertical") > 0f)
                Transition(ref playerInstance.currentState, new JumpingState(playerInstance));
        }
    }

    private class JumpingState : State
    {
        PlayerController1 playerInstance;

        float maxTime;

        public JumpingState(PlayerController1 playerInstance)
        {
            this.playerInstance = playerInstance;
        }

        public override void Enter()
        {
            playerInstance.speedlol.y = 2f;

            maxTime = Time.time + 0.2f;
        }

        public override void FixedUpdate()
        {
            float input = 0f;

            input = Input.GetAxis("Horizontal");

            playerInstance.speedlol.x = input * 5f;

            if (Input.GetAxis("Vertical") <= 0f || maxTime < Time.time)
                Transition(ref playerInstance.currentState, new FallingState(playerInstance));
        }
    }

    private class FallingState : State
    {
        PlayerController1 playerInstance;

        public FallingState(PlayerController1 playerInstance)
        {
            this.playerInstance = playerInstance;
        }

        public override void FixedUpdate()
        {
            float input = 0f;

            input = Input.GetAxis("Horizontal");

            playerInstance.speedlol.x = input * 5f;

            playerInstance.speedlol.y -= 0.2f;
        }
    }



    // Use this for initialization
    void Start ()
    {
        Collider2D colliderInstance = GetComponent<Collider2D>();
        Bounds boundsInstance = colliderInstance.bounds;

        topLeftOffset = new Vector3(-boundsInstance.extents.x, boundsInstance.extents.y, 0f);
        topLeftOffset += (Vector3)colliderInstance.offset;

        boundsSize = GetComponent<Collider2D>().bounds.size;

        speedlol = Vector2.zero;

        currentState = new State();
        currentState.Transition(ref currentState, new GroundedState(this));
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        currentState.FixedUpdate();

        RaycastDirection(speedlol * Time.deltaTime);
    }

    void RaycastDirection(Vector3 direction)
    {
        Vector3 rayOrigin = new Vector3();

        // Horizontal check
        if(direction.x != 0f)
        {
            rayOrigin = transform.position + topLeftOffset;
            rayOrigin.y -= sizeTolerance / 2;
            rayOrigin.x += direction.x < 0f ? skinSize : boundsSize.x - skinSize;

            float directionFactor = direction.x > 0f ? 1f : -1f;

            RaycastHit2D[] raycasts = new RaycastHit2D[horizontalRays];

            RaycastHit2D shortestRaycast;

            for (int i = 0; i < horizontalRays; i++)
            {
                RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, new Vector2(directionFactor, 0f), Mathf.Abs(direction.x) + skinSize, environmentLayer);
                
                raycasts[i] = raycast;

                Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(direction.x + (skinSize * directionFactor), 0f, 0f), Color.white);

                rayOrigin += new Vector3(0f, -(boundsSize.y - sizeTolerance) / (horizontalRays - 1), 0f);
            }

            shortestRaycast = raycasts[0];

            for (int i = 1; i < raycasts.Length; i++)
            {
                if ((raycasts[i].distance < shortestRaycast.distance && raycasts[i]) || !shortestRaycast)
                    shortestRaycast = raycasts[i];
            }

            if (shortestRaycast)
            {
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(-0.25f, 0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(0.25f, 0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(-0.25f, -0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(0.25f, -0.25f), Color.red);

                float m = shortestRaycast.point.x;
                m += boundsSize.x / 2 * -directionFactor;
                m -= GetComponent<Collider2D>().offset.x;

                transform.position = new Vector3(m, transform.position.y, transform.position.z);
            }
            else
            {
                transform.Translate(direction.x, 0f, 0f);
            }
        }

        // Vertical check
        if (direction.y != 0f)
        {
            rayOrigin = transform.position + topLeftOffset;
            rayOrigin.x += sizeTolerance / 2;
            rayOrigin.y += direction.y > 0f ? -skinSize : -boundsSize.y + skinSize;

            float directionFactor = direction.y > 0f ? 1f : -1f;

            RaycastHit2D[] raycasts = new RaycastHit2D[verticalRays];

            RaycastHit2D shortestRaycast;

            for (int i = 0; i < verticalRays; i++)
            {
                RaycastHit2D raycast = Physics2D.Raycast(rayOrigin, new Vector2(0f, directionFactor), Mathf.Abs(direction.y) + skinSize, environmentLayer);

                raycasts[i] = raycast;

                Debug.DrawLine(rayOrigin, rayOrigin + new Vector3(0f, direction.y + (skinSize * directionFactor), 0f), Color.white);

                rayOrigin += new Vector3((boundsSize.x - sizeTolerance) / (verticalRays - 1), 0f, 0f);
            }

            shortestRaycast = raycasts[0];

            for (int i = 1; i < raycasts.Length; i++)
            {
                if ((raycasts[i].distance < shortestRaycast.distance && raycasts[i]) || !shortestRaycast)
                    shortestRaycast = raycasts[i];
            }

            if (shortestRaycast)
            {
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(-0.25f, 0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(0.25f, 0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(-0.25f, -0.25f), Color.red);
                Debug.DrawLine(shortestRaycast.point, shortestRaycast.point + new Vector2(0.25f, -0.25f), Color.red);

                float m = shortestRaycast.point.y;
                m += boundsSize.y / 2 * -directionFactor;
                m -= GetComponent<Collider2D>().offset.y;

                transform.position = new Vector3(transform.position.x, m, transform.position.z);
            }
            else
            {
                transform.Translate(0f, direction.y, 0f);
            }
        }
    }
}
