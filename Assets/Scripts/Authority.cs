using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;

    protected bool newSomeone = false;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    //methods
    public Authority(string name, Genders gender, Vector3 position) : base(name, gender, position)
    {
        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = doorSubFSM.CreatePerception<ValuePerception>(() => newSomeone == true); //temporal
        Perception timer = doorSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State waitingState = doorSubFSM.CreateEntryState("Waiting for someone", Waiting);
        State welcomeState = doorSubFSM.CreateState("Welcome", Welcome);

        // Transitions
        doorSubFSM.CreateTransition("Someone new arrives", waitingState, push, welcomeState);
        doorSubFSM.CreateTransition("Welcome finished", welcomeState, timer, waitingState);
    }

    //Common behaviours: Organizer Students and Teachers
    protected void Waiting()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting for someone to come...");
        this.Move(new Vector3(2, 2, 0));
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        newSomeone = false;
    }

    protected void Patrol()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm watching you!");
    }

    protected void ChaseStudent()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Come back here, you little...");
    }
}
