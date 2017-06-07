using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    
    // Called when entering the current state
    public virtual void Enter()
    {
    }

    // Called when exiting a state
    public virtual void Exit()
    {
    }

    // Called when transitioning to another state
    public void Transition(ref State activeState, State newState)
    {
        // Runs the old state's exit logic
        activeState.Exit();

        // Sets the active state to the new state
        activeState = newState;

        // Runs the new state's entry logic
        activeState.Enter();
    }

    public virtual void Update()
    {
    }
    
    public virtual void FixedUpdate()
    {
    }
    
    public virtual void LateUpdate()
    {
    }
    
    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
    }
    
    public virtual void OnCollisionExit2D(Collision2D collision)
    {
    }
    
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
    }
    
    public virtual void OnTriggerExit2D(Collider2D collision)
    {
    }
}
