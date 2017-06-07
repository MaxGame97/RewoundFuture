using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    light, medium, heavy
}

public enum MovementState
{
    idle, running, jumping, falling, landing
}

public class PlayerController : MonoBehaviour
{
    public LayerMask m_collisionMask;
    
    //Public run vars
    [UnityEngine.Header("Run variables")]
    [UnityEngine.Tooltip("The speed at which the player reaches maximum speed (Range : 0-1)")]
    public float m_runWindupScale;
    public float m_runDecayScale;
    public float m_dirChangeScale;
    public float m_speed;

    //Public jump vars
    [UnityEngine.Header("Jump variables")]
    public float m_jumpHeight;
    public float m_doubleJumpHeight;
    public float m_jumpTime;
    public float m_jumpDecayScale;

    //General Vars
    public MovementState m_state;

    //Local jump vars
    float m_currentJumpTime;
    float m_jumpTimeElapsed = 0;
    float m_currentMaxJumpHeight;
    float m_currentJumpHeight;

    //Local run vars
    float m_runWindupMult;
    float m_lastXValue;

    //Local jump bools
    bool m_jumpCancelled;
    bool m_canJump;
    bool m_jumpObstructed;
    bool m_doubleJumpTriggered = false;
    bool m_canDoubleJump = false;

    //Local general bools
    bool m_canMove = true;
    bool m_lastFlipDir;

    //Component vars
    public AnimationCurve m_jumpCurve;
    Rigidbody2D m_rigidbody;
    SpriteRenderer m_spriteRenderer;
    Animator m_animator;
    Vector2 m_directionVector;
    PlayerAttack m_attack;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_currentJumpTime = m_jumpTime;
        m_currentMaxJumpHeight = m_jumpHeight;
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_attack = GetComponent<PlayerAttack>();
    }

    void FixedUpdate()
    {
        InputUpdate(ref m_directionVector);

        AnimationUpdate(m_directionVector);
        MoveUpdate(m_directionVector);
        JumpUpdate(m_directionVector);
    }   

    void AnimationUpdate(Vector2 xyVector)
    {
        m_spriteRenderer.flipX = m_lastFlipDir;

        if (xyVector.x > 0)
        {
            m_lastFlipDir = true;
        }
        else if (xyVector.x < 0)
        {
            m_lastFlipDir = false;
        }

        switch (m_state)
        {
            case MovementState.idle:
                m_animator.SetInteger("State", (int)MovementState.idle);
                break;
            case MovementState.running:
                m_animator.SetInteger("State", (int)MovementState.running);
                break;
            case MovementState.jumping:
                m_animator.SetInteger("State", (int)MovementState.jumping);
                if (m_animator.GetInteger("State") == (int)MovementState.jumping)
                {
                    m_state = MovementState.falling;
                }
                break;
            case MovementState.falling:
                m_animator.SetInteger("State", (int)MovementState.falling);
                break;
            case MovementState.landing:
                m_animator.SetInteger("State", (int)MovementState.landing);
                break;
            default:
                break;
        }
    }

    IEnumerator PlayTimedAnimation(string name, float time)
    {
        m_animator.Play(name);

        m_canMove = false;
        yield return new WaitForSeconds(time);
        m_canMove = true;

        m_animator.Play("Idle");
    }
    
    void InputUpdate(ref Vector2 xyVector)
    {
        xyVector = Vector2.zero;

        if (m_canMove) {

            if (Input.GetButtonDown("Fire1"))
            {
                StartCoroutine(PlayTimedAnimation("Attack 1", 0.9f));
                m_attack.Attack(AttackType.light, m_lastFlipDir);
            }

            xyVector.x = Input.GetAxis("Horizontal");
            xyVector.y = Input.GetButton("Vertical") ? 1 : 0;
        }
    }
    /// <summary>
    /// Calculates character transform speed based on windup, decay & direction change
    /// Result of said calculations is then clamped between 0-1 and multiplied with the base movement speed
    /// If player releases movement button the movement should still decay in the last direction the player was moving in,
    /// therefore 
    /// </summary>
    /// <param name="xVector"></param>
    void MoveUpdate(Vector2 xVector)
    {
        xVector.y = 0;

        //Checks if player is pressing a button to move
        if (xVector.x != 0)
        {
            //Checks if player has changed direction
            if (m_lastXValue != xVector.x)
            {
                m_lastXValue = Mathf.Clamp(xVector.x * m_dirChangeScale, -1, 1);   
            }

            //Increments multiplier based on scale
            m_runWindupMult += Mathf.Abs(xVector.x) * m_runWindupScale;
        }
        //Multiplier is decremented based on scale if not moving
        else
        {
            m_runWindupMult -= m_runDecayScale;
        }

        //Clamp
        m_runWindupMult = Mathf.Clamp01(m_runWindupMult);

        //If multiplier has reached zero the player should no longer be moving.
        if (m_runWindupMult == 0)
        {
            m_lastXValue = 0;
        }

        //Translates on X based on mult, base speed and input
        transform.Translate(new Vector2(m_lastXValue, 0) * m_runWindupMult * m_speed * Time.fixedDeltaTime);
    }

    void JumpUpdate(Vector2 yVector)
    {
        //Checks if pressing jump button, jumptime finished and canjump is true
        if (yVector.y > 0 && m_jumpTimeElapsed < m_currentJumpTime && !m_jumpCancelled && m_canJump)
        {
            if (!m_canJump || !CheckOnGround())
            {
                JumpReset(yVector.y);
            }

            JumpTranslate(yVector.y);

            m_state = MovementState.jumping;
        }

        //Triggers when jump is cancelled by releasing jump key mid-jump
        else if (yVector.y == 0 && m_jumpTimeElapsed < m_currentJumpTime && m_jumpTimeElapsed != 0 && !m_jumpCancelled && !m_jumpObstructed)
        {
            JumpDecay();
            m_jumpCancelled = true;
            m_canJump = false;
        }

        else
        {
            //Resets current jumpheight if on ground
            JumpReset(yVector.y);
        }
    }

    void JumpTranslate(float yVector)
    {
        float jumpheight = m_jumpCurve.Evaluate(m_jumpTimeElapsed / m_currentJumpTime) * m_currentMaxJumpHeight;
        float gravityCompensator = m_rigidbody.velocity.y * Time.fixedDeltaTime;

        jumpheight = jumpheight * yVector;

        transform.Translate(Vector3.up * ((jumpheight - m_currentJumpHeight) - gravityCompensator));

        m_currentJumpHeight = jumpheight;
        m_jumpTimeElapsed += Time.fixedDeltaTime;

        if (CheckAirCollision())
        {
            m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, 0);
            m_canJump = false;
            m_jumpObstructed = true;
        }
    }

    /// <summary>
    /// An effective translation of translation velocity to rigidbody velocity.
    /// Adds an upward velocity to the rigidbody equal to the current translation velocity at the exact moment the jump button is released. 
    /// This is multiplied with m_jumpDecayScale.
    /// </summary>
    void JumpDecay()
    {
        float jumpheight = m_jumpCurve.Evaluate(m_jumpTimeElapsed / m_currentJumpTime) * m_currentMaxJumpHeight;

        m_rigidbody.velocity = (new Vector2(m_rigidbody.velocity.x, ((jumpheight - m_currentJumpHeight) / Time.fixedDeltaTime * m_jumpDecayScale)));

    }

    void JumpReset(float yVector) {

        if (!CheckOnGround() && !m_canDoubleJump && yVector == 0)
        {
            if (m_jumpTimeElapsed >= m_currentJumpTime)
            {
                m_canDoubleJump = true;
            }

            else if (m_jumpTimeElapsed <= 0) {
                m_canDoubleJump = true;
            }

            else if (m_jumpCancelled)
            {
                m_canDoubleJump = true;
            }

            else if (m_jumpObstructed)
            {
                m_canDoubleJump = true;
            }
        }

        // Checks if jump has finished and resets velocity
        if (m_jumpTimeElapsed > m_currentJumpTime && m_canJump)
        {
            m_canJump = false;
            m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, 0);
        }

        //Checks if the player is on the ground and not pressing the jump button
        if (CheckOnGround() && yVector == 0)
        {
            if (!m_canJump)
            {
                m_state = MovementState.landing;
            }

            m_doubleJumpTriggered = false;
            m_canDoubleJump = false;
            m_currentMaxJumpHeight = m_jumpHeight;
            m_currentJumpTime = m_jumpTime;

            JumpResetVars();
        }

        if (m_canDoubleJump && yVector == 1 && !m_doubleJumpTriggered)
        {
            m_doubleJumpTriggered = true;
            m_canDoubleJump = false;
            m_currentMaxJumpHeight = m_doubleJumpHeight;
            m_currentJumpTime = m_jumpTime * m_doubleJumpHeight / m_jumpHeight; 

            JumpResetVars();
        }
    }

    void JumpResetVars()
    {
        m_canJump = true;
        m_jumpCancelled = false;
        m_jumpObstructed = false;
        m_jumpTimeElapsed = 0;
        m_currentJumpHeight = 0;
    }

    bool CheckOnGround()
    {
        const float distToFeet = 0.5f;
        const float dist = 0.2f;
        const float distToEdge = 0.4f;

        return (
            Physics2D.Raycast(transform.position + Vector3.down * distToFeet + Vector3.right * distToEdge, Vector2.down, dist, m_collisionMask)
            ||
            Physics2D.Raycast(transform.position + Vector3.down * distToFeet + Vector3.left * distToEdge, Vector2.down, dist, m_collisionMask));
    }

    bool CheckAirCollision()
    {
        const float distToTop = 0.5f;
        const float dist = 0.2f;
        const float distToEdge = 0.4f;

        return (
            Physics2D.Raycast(transform.position + Vector3.up * distToTop + Vector3.right * distToEdge, Vector2.up, dist, m_collisionMask)
            ||
            Physics2D.Raycast(transform.position + Vector3.up * distToTop + Vector3.left * distToEdge, Vector2.up, dist, m_collisionMask));
    }
}
