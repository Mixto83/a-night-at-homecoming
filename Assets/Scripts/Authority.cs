using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;
    private bool isNew = true;

    protected bool newSomeone = false;
    protected float distanceToDoor;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    //methods
    public Authority(string name, Genders gender, Transform obj) : base(name, gender, obj)
    {
        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToDoor = doorSubFSM.CreatePerception<ValuePerception>(() => distanceToDoor < 0.3f);
        WatchingPerception seeCalmStudent = doorSubFSM.CreatePerception<WatchingPerception>(new WatchingPerception(this.gameObject, "CalmStudent", this.gameObject.GetComponentInChildren<MeshCollider>()));
        Perception isNewStudent = doorSubFSM.CreatePerception<ValuePerception>(() => isNew);
        Perception seeNewStudent = doorSubFSM.CreateAndPerception<AndPerception>(seeCalmStudent, isNewStudent);
        Perception timer = doorSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State walkingState = doorSubFSM.CreateEntryState("Walking to door", Walking);
        State waitingState = doorSubFSM.CreateState("Waiting for someone", Waiting);
        State welcomeState = doorSubFSM.CreateState("Welcome", Welcome);

        // Transitions
        doorSubFSM.CreateTransition("Got to the door", walkingState, gotToDoor, waitingState);
        doorSubFSM.CreateTransition("Someone new arrives", waitingState, seeNewStudent, welcomeState);
        doorSubFSM.CreateTransition("Welcome finished", welcomeState, timer, waitingState);
    }

    //Common behaviours: Organizer Students and Teachers
    protected void Waiting()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting for someone to come...");

        this.LookAt(GameObject.FindGameObjectWithTag("Door").transform);

        foreach (Text txt in GameObject.FindObjectsOfType<Text>())
        {
            if(txt.text == "Welcome to the party!")
            {
                GameObject.Destroy(txt.gameObject);
            }
        }
    }

    protected void Walking()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to door at: ");

        this.Move(GameObject.FindGameObjectWithTag("Door").transform.position - new Vector3(1, 0, 0));
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        createMessage("Welcome to the party!", Color.blue);
        isNew = false;
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
