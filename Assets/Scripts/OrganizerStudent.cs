using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Authority
{
    //parameters
    private StateMachineEngine organizerStudentFSM;
    private StateMachineEngine servingSubFSM;
    private StateMachineEngine patrolSubFSM;

    float negotiateRandom;
    private bool isNewBool = true;
    protected float distanceToBar;

    //methods
    public OrganizerStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.OrganizerStudent;
        this.strictness = 0.5f;

        CreateServingSubStateMachine();
        CreatePatrolSubStateMachine();
        CreateStateMachine();

        this.negotiateRandom = Random.value;
    }

    private void CreateServingSubStateMachine()
    {
        servingSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToBar = servingSubFSM.CreatePerception<ValuePerception>(() => distanceToBar < 0.3f);
        WatchingPerception seeSomeone = servingSubFSM.CreatePerception<WatchingPerception>(new WatchingPerception(this.gameObject, "CalmStudent", this.gameObject.GetComponentInChildren<MeshCollider>()));
        Perception isNew = servingSubFSM.CreatePerception<ValuePerception>(() => isNewBool);
        Perception seeSomoneNew = servingSubFSM.CreateAndPerception<AndPerception>(seeSomeone, isNew);
        Perception timer = servingSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State walkingState = servingSubFSM.CreateEntryState("Walking to bar", () => Walking("Bar", new Vector3(0, 1, 0), !gameState.getBarAttended()));
        State waitingState = servingSubFSM.CreateState("Waiting for client", () => WaitingAt("Bar"));
        State serveState = servingSubFSM.CreateState("Serve", ServeDrink);

        // Transitions
        servingSubFSM.CreateTransition("Got to bar", walkingState, gotToBar, waitingState);
        servingSubFSM.CreateTransition("New client arrives", waitingState, seeSomoneNew, serveState);
        servingSubFSM.CreateTransition("Drink served", serveState, timer, waitingState);
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = patrolSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception convinced = patrolSubFSM.CreatePerception<ValuePerception>(() => CheckConvinced());
        Perception notConvinced = patrolSubFSM.CreatePerception<ValuePerception>(() => !CheckConvinced());
        Perception negotiationEnd = patrolSubFSM.CreatePerception<TimerPerception>(2);

        Perception negotiationEndConvinced = patrolSubFSM.CreateAndPerception<AndPerception>(convinced, negotiationEnd);
        Perception negotiationEndNotConvinced = patrolSubFSM.CreateAndPerception<AndPerception>(notConvinced, negotiationEnd);

        // States
        State patrollingState = patrolSubFSM.CreateEntryState("Patroling", Patrol);
        State chaseState = patrolSubFSM.CreateState("Chasing", ChaseStudent);
        State negotiateState = patrolSubFSM.CreateState("Negotiate", Negotiate);
        State callTeacherState = patrolSubFSM.CreateState("Call Teacher", CallTeacher);

        // Transitions
        patrolSubFSM.CreateTransition("Sees trouble", patrollingState, push, chaseState);

        patrolSubFSM.CreateTransition("Student lost", chaseState, push, patrollingState);
        patrolSubFSM.CreateTransition("Messy Student Caught", chaseState, push, negotiateState);

        patrolSubFSM.CreateTransition("Convinced", negotiateState, negotiationEndConvinced, patrollingState);
        patrolSubFSM.CreateTransition("Not convinced", negotiateState, negotiationEndNotConvinced, callTeacherState);

        patrolSubFSM.CreateTransition("Teacher got the call", callTeacherState, push, patrollingState);
    }

    private void CreateStateMachine()
    {
        organizerStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception rest = organizerStudentFSM.CreatePerception<TimerPerception>(5);
        Perception doorAttended = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.gameState.getDoorAttended());
        Perception doorUnattended = organizerStudentFSM.CreatePerception<ValuePerception>(() => !this.gameState.getDoorAttended());
        Perception barAttended = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());
        Perception barUnattended = organizerStudentFSM.CreatePerception<ValuePerception>(() => !this.gameState.getBarAttended());
        Perception doorNBar = organizerStudentFSM.CreateAndPerception<AndPerception>(doorAttended, barUnattended);
        Perception doorBar = organizerStudentFSM.CreateAndPerception<AndPerception>(doorAttended, barAttended);
        Perception outOfDoor = organizerStudentFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Out State");

        Perception goToDoor = organizerStudentFSM.CreateAndPerception<AndPerception>(doorUnattended, rest);
        Perception goToBar = organizerStudentFSM.CreateAndPerception<AndPerception>(doorNBar, rest);
        OrPerception goBack = organizerStudentFSM.CreateOrPerception<OrPerception>(outOfDoor, rest);

        // States
        State startState = organizerStudentFSM.CreateEntryState("Start", () => Move(initPos));
        State doorState = organizerStudentFSM.CreateSubStateMachine("Door", doorSubFSM);
        State serveDrinkState = organizerStudentFSM.CreateSubStateMachine("Serve Drink", servingSubFSM);
        State patrolState = organizerStudentFSM.CreateSubStateMachine("Patrol", patrolSubFSM);

        // Transitions
        organizerStudentFSM.CreateTransition("Door unattended", startState, goToDoor, doorState);
        organizerStudentFSM.CreateTransition("Door attended, bar unattended", startState, goToBar, serveDrinkState);
        organizerStudentFSM.CreateTransition("Door attended, bar attended", startState, doorBar, patrolState);
        patrolSubFSM.CreateExitTransition("Bar unattended", patrolState, barUnattended, serveDrinkState);
        servingSubFSM.CreateExitTransition("Door unattended from bar", serveDrinkState, goToDoor, doorState);
        doorSubFSM.CreateExitTransition("Back", doorState, goBack, startState);
    }

    public override void Update()
    {
        doorSubFSM.Update();
        servingSubFSM.Update();
        patrolSubFSM.Update();
        organizerStudentFSM.Update();

        distanceToDoor = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Door").transform.position + new Vector3(-1, 0, 0));
        distanceToBar = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0, 1, 0));

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            organizerStudentFSM.Fire("Door unattended");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            organizerStudentFSM.Fire("Door attended, bar unattended");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            organizerStudentFSM.Fire("Door attended, bar attended");
        }
    }

    public override bool isInState(string subFSM, string subState)
    {
        Perception isIn;
        switch (subFSM)
        {
            case "Door":
                isIn = organizerStudentFSM.CreatePerception<IsInStatePerception>(doorSubFSM, subState);
                break;
            case "Bar":
                isIn = organizerStudentFSM.CreatePerception<IsInStatePerception>(servingSubFSM, subState);
                break;
            case "Patrol":
                isIn = organizerStudentFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, subState);
                break;
            default:
                isIn = organizerStudentFSM.CreatePerception<PushPerception>();
                break;
        }

        return isIn.Check();
    }

    //Behaviours
    protected void ServeDrink()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Serving drink");
        createMessage("Have a drink!", Color.blue);
        isNewBool = false;
    }

    protected void Negotiate()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I shouldn't let you go, but...");
        negotiateRandom = Random.value;
    }

    protected void CallTeacher()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Sir, get that kid!");
    }

    private bool CheckConvinced()
    {
        return negotiateRandom > 0.5f;
    }
}