using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;
    private WatchingPerception watchingDoor;
    protected WatchingPerception watchingTrouble;

    protected Character targetStudent;
    protected Perception isInStateChasing;
    protected Perception timerChasing;

    protected float distanceToDoor;

    #region Stats
    protected float strictness;
    #endregion

    //methods
    public Authority(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.watchingDoor = new WatchingPerception(this.gameObject, () => !watchingDoor.getTargetCharacter().getGreeted(),
            () => watchingDoor.getTargetCharacter().getRole() == Roles.CalmStudent || watchingDoor.getTargetCharacter().getRole() == Roles.MessyStudent);
        this.watchingTrouble = new WatchingPerception(this.gameObject, () => watchingTrouble.getTargetCharacter().getRole() == Roles.MessyStudent,
            () => ((MessyStudent)watchingTrouble.getTargetCharacter()).isCausingTrouble());

        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToDoor = doorSubFSM.CreatePerception<ValuePerception>(() => distanceToDoor < 0.3f);
        WatchingPerception seeSomeone = doorSubFSM.CreatePerception<WatchingPerception>(watchingDoor);
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

        clearSprites();
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        //createMessage("Welcome to the party!", Color.blue);
        watchingDoor.getTargetCharacter().setGreeted(true);
    }

    protected void Patrol()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm watching you!");
        //createMessage("I'm watching you!", Color.blue);
    }

    protected void ChaseStudent()
    {
        createMessage(11);
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        if(targetStudent == null) targetStudent = watchingTrouble.getTargetCharacter();
        Debug.Log("[" + name + ", " + getRole() + "] Come back here, you little...");
    }
}
