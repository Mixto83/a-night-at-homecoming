using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;
    private WatchingPerception watching;
    
    protected float distanceToDoor;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    //methods
    public Authority(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.watching = new WatchingPerception(this.gameObject, () => !watching.getTargetCharacter().getGreeted(),
            () => watching.getTargetCharacter().getRole() == Roles.CalmStudent || watching.getTargetCharacter().getRole() == Roles.MessyStudent);
        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToDoor = doorSubFSM.CreatePerception<ValuePerception>(() => distanceToDoor < 0.3f);
        WatchingPerception seeSomeone = doorSubFSM.CreatePerception<WatchingPerception>(watching);
        Perception timer = doorSubFSM.CreatePerception<TimerPerception>(2);
        Perception doorAttended = doorSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getDoorAttended());

        // States
        State walkingState = doorSubFSM.CreateEntryState("Walking to door", () => Walking("Door", new Vector3(-0.75f, 0, 0)));
        State waitingState = doorSubFSM.CreateState("Waiting for someone", ()=> WaitingAt("Door"));
        State welcomeState = doorSubFSM.CreateState("Welcome", Welcome);
        State outState = doorSubFSM.CreateState("Out Door State");

        // Transitions
        doorSubFSM.CreateTransition("Got to the door", walkingState, gotToDoor, waitingState);
        doorSubFSM.CreateTransition("Someone new arrives", waitingState, seeSomeone, welcomeState);
        doorSubFSM.CreateTransition("Welcome finished", welcomeState, timer, waitingState);
        doorSubFSM.CreateTransition("Back out", walkingState, doorAttended, outState);
    }

    //Common behaviours: Organizer Students and Teachers
    protected void WaitingAt(string tag)
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting for someone to come...");

        this.LookAt(GameObject.FindGameObjectWithTag(tag).transform);

        clearTexts(this);
    }

    protected void Walking(string tag, Vector3 offset)
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to " + tag);

        this.Move(GameObject.FindGameObjectWithTag(tag).transform.position + offset);

        clearTexts(this);
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        createMessage("Welcome to the party!", Color.blue);
        watching.getTargetCharacter().setGreeted(true);
    }

    protected void Patrol()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm watching you!");
        createMessage("I'm watching you!", Color.blue);
    }

    protected void ChaseStudent()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Come back here, you little...");
        createMessage("Come back here, you little...", Color.blue);
    }
}
