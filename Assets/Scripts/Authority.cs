using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;
    private bool isNewBool = true;
    protected Perception pushBack;
    protected Perception pushBack2;
    protected bool shouldGoBool;

    protected bool newSomeone = false;
    protected float distanceToDoor;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    //methods
    public Authority(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToDoor = doorSubFSM.CreatePerception<ValuePerception>(() => distanceToDoor < 0.3f);
        WatchingPerception seeSomeone = doorSubFSM.CreatePerception<WatchingPerception>(new WatchingPerception(this.gameObject, "CalmStudent", this.gameObject.GetComponentInChildren<MeshCollider>()));
        Perception isNew = doorSubFSM.CreatePerception<ValuePerception>(() => isNewBool);
        Perception seeSomoneNew = doorSubFSM.CreateAndPerception<AndPerception>(seeSomeone, isNew);
        Perception timer = doorSubFSM.CreatePerception<TimerPerception>(2);
        pushBack = doorSubFSM.CreatePerception<PushPerception>();
        pushBack2 = doorSubFSM.CreatePerception<PushPerception>();

        // States
        State walkingState = doorSubFSM.CreateEntryState("Walking to door", () => Walking("Door", new Vector3(-1, 0, 0), !gameState.getDoorAttended()));
        State waitingState = doorSubFSM.CreateState("Waiting for someone", ()=> WaitingAt("Door"));
        State welcomeState = doorSubFSM.CreateState("Welcome", Welcome);
        State outState = doorSubFSM.CreateState("Out State");

        // Transitions
        doorSubFSM.CreateTransition("Got to the door", walkingState, gotToDoor, waitingState);
        doorSubFSM.CreateTransition("Someone new arrives", waitingState, seeSomoneNew, welcomeState);
        doorSubFSM.CreateTransition("Welcome finished", welcomeState, timer, waitingState);
        doorSubFSM.CreateTransition("Back out", walkingState, pushBack, outState);
        doorSubFSM.CreateTransition("Back out from wait", waitingState, pushBack, outState);
    }

    //Common behaviours: Organizer Students and Teachers
    protected void WaitingAt(string tag)
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting for someone to come...");

        this.LookAt(GameObject.FindGameObjectWithTag(tag).transform);

        if (!shouldGoBool)
        {
            Debug.Log("Nevermind, someone's already there");
            pushBack2.Fire();
        }

        foreach (Text txt in GameObject.FindObjectsOfType<Text>())
        {
            if(txt.text == "Welcome to the party!" || txt.text == "Have a drink!")
            {
                GameObject.Destroy(txt.gameObject);
            }
        }
    }

    protected void Walking(string tag, Vector3 offset, bool shouldGo)
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to " + tag);

        this.shouldGoBool = shouldGo;

        if (shouldGoBool) {
            this.Move(GameObject.FindGameObjectWithTag(tag).transform.position + offset);
        } else
        {
            Debug.Log("Nevermind, someone's already there");
            pushBack.Fire();
        }
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        createMessage("Welcome to the party!", Color.blue);
        isNewBool = false;
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
