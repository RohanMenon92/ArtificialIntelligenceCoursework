using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMControlAgent : MonoBehaviour
{
    public string tagToTarget;

    private State currentState = State.Patrol;

    // To check Entry and Exit of state
    private State lastState = State.Idle;


    #region FSMStatesRegion
    private enum State {
        Idle = 0,
        Patrol = 1,
        Investigate = 2,
        Confront = 3,
        Dodge = 4,
        Dead = 5
        //Flee = 0
    };

    void SwitchState(State newState)
    {
        bool switchAllowed = false;

        // Check if switch between states is allowed
        switch (currentState)
        {
            case State.Idle:
                {
                    if(newState == State.Patrol || newState == State.Investigate || newState == State.Dead)
                    {
                        switchAllowed = true;
                    }
                }
                break;
            case State.Patrol:
                {
                    if (newState == State.Idle || newState == State.Investigate || newState == State.Confront)
                    {
                        switchAllowed = true;
                    }
                }
                break;
            case State.Investigate:
                {

                }
                break;
            case State.Confront:
                {

                }
                break;
            case State.Dodge:
                {

                }
                break;
            case State.Dead:
                {

                }
                break;
        }

        if(switchAllowed)
        {
            currentState = newState;
        }
    }

    // Check current and last state and switch when required
    void CheckState(State currState)
    {
        if(currentState != lastState)
        {
            OnExitState(lastState);
            OnEnterState(currentState);
            lastState = currentState;
        }
        OnProcessState(currentState);
    }

    // Check Exit from last state
    void OnExitState(State stateExit)
    {
        switch(stateExit)
        {
            case State.Idle:
                {

                }
                break;
            case State.Patrol:
                {

                }
                break;
            case State.Investigate:
                {

                }
                break;
            case State.Confront:
                {

                }
                break;
            case State.Dodge:
                {

                }
                break;
            case State.Dead:
                {

                }
                break;
        }
    }

    // Check entry to stateEnter
    void OnEnterState(State stateEnter)
    {
        switch (stateEnter)
        {
            case State.Idle:
                {

                }
                break;
            case State.Patrol:
                {

                }
                break;
            case State.Investigate:
                {

                }
                break;
            case State.Confront:
                {

                }
                break;
            case State.Dodge:
                {

                }
                break;
            case State.Dead:
                {

                }
                break;
        }
    }

#endregion

    // Loop for currState
    void OnProcessState(State currState)
    {
        switch (currState)
        {
            case State.Idle:
                {

                }
                break;
            case State.Patrol:
                {

                }
                break;
            case State.Investigate:
                {

                }
                break;
            case State.Confront:
                {

                }
                break;
            case State.Dodge:
                {

                }
                break;
            case State.Dead:
                {

                }
                break;
        }
    }



    // Start is called before the first frame update
    void Start()
    {

        
    }

    void FixedUpdate()
    {
        CheckState(currentState);

    }
}
