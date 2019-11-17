using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Authority
{
    //parameters
    private StateMachineEngine organizerStudentFSM;
    private StateMachineEngine servingSubFSM;
    private StateMachineEngine patrolSubFSM;

    private WatchingPerception watching;
    private Perception timer;
    private Perception isInStatePatrol;

    float tired = 0;
    float negotiateRandom;
    protected float distanceToBarOrg;

    //methods
    public OrganizerStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.OrganizerStudent;
        this.strictness = 0.5f;

        this.watching = new WatchingPerception(this.gameObject, () => watching.getTargetCharacter().getCanBeServed(),
            () => watching.getTargetCharacter().getRole() != Roles.OrganizerStudent);

        CreateServingSubStateMachine();
        CreatePatrolSubStateMachine();

        this.negotiateRandom = Random.value;
    }

    private void CreateServingSubStateMachine()
    {
        servingSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToBar = servingSubFSM.CreatePerception<ValuePerception>(() => distanceToBarOrg < 0.3f);
        WatchingPerception seeSomeone = servingSubFSM.CreatePerception<WatchingPerception>(watching);
        Perception timer = servingSubFSM.CreatePerception<TimerPerception>(2);
        Perception barAttended = servingSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());

        // States
        State walkingState = servingSubFSM.CreateEntryState("Walking to bar", () => Walking("Bar", new Vector3(0, 0.75f, 0)));
        State waitingState = servingSubFSM.CreateState("Waiting for client", () => WaitingAt("Bar"));
        State serveState = servingSubFSM.CreateState("Serve", ServeDrink);
        State outState = servingSubFSM.CreateState("Out Bar State");

        // Transitions
        servingSubFSM.CreateTransition("Got to bar", walkingState, gotToBar, waitingState);
        servingSubFSM.CreateTransition("New client arrives", waitingState, seeSomeone, serveState);
        servingSubFSM.CreateTransition("Drink served", serveState, timer, waitingState);
        servingSubFSM.CreateTransition("Back out", walkingState, barAttended, outState);
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = patrolSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception seeMessyStudentInTroubleState = patrolSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception lostSightOfStudent = patrolSubFSM.CreatePerception<PushPerception>(); //temporal
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

        timer = patrolSubFSM.CreatePerception<TimerPerception>(5);
        isInStatePatrol = patrolSubFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Patroling");

        // Transitions
        patrolSubFSM.CreateTransition("See trouble", patrollingState, seeMessyStudentInTroubleState, chaseState);

        patrolSubFSM.CreateTransition("Student lost", chaseState, lostSightOfStudent, patrollingState);
        patrolSubFSM.CreateTransition("Messy Student Caught", chaseState, push, negotiateState);

        patrolSubFSM.CreateTransition("Convinced", negotiateState, negotiationEndConvinced, patrollingState);
        patrolSubFSM.CreateTransition("Not convinced", negotiateState, negotiationEndNotConvinced, callTeacherState);

        patrolSubFSM.CreateTransition("Teacher got the call", callTeacherState, push, patrollingState);
    }

    public override void CreateStateMachine()
    {
        organizerStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception tired = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.tired >= 20);
        Perception notTired = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.tired <= 0.1f);
        Perception doorAttended = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.gameState.getDoorAttended());
        Perception doorUnattended = organizerStudentFSM.CreatePerception<ValuePerception>(() => !this.gameState.getDoorAttended());
        Perception barAttended = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());
        Perception barUnattended = organizerStudentFSM.CreatePerception<ValuePerception>(() => !this.gameState.getBarAttended());
        Perception doorNBar = organizerStudentFSM.CreateAndPerception<AndPerception>(doorAttended, barUnattended);
        Perception goPatrol = organizerStudentFSM.CreateAndPerception<AndPerception>(doorAttended, barAttended);
        Perception outOfDoor = organizerStudentFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Out Door State");
        Perception outOfBar = organizerStudentFSM.CreatePerception<IsInStatePerception>(servingSubFSM, "Out Bar State");
        Perception isWaiting = organizerStudentFSM.CreatePerception<IsInStatePerception>(servingSubFSM, "Waiting for client");
        Perception goToDoor = organizerStudentFSM.CreateAndPerception<AndPerception>(doorUnattended, notTired);
        Perception goToDoorNow = organizerStudentFSM.CreateAndPerception<AndPerception>(doorUnattended, isWaiting);
        Perception goToBar = organizerStudentFSM.CreateAndPerception<AndPerception>(doorNBar, notTired);
        Perception goBackFromDoor = organizerStudentFSM.CreateOrPerception<OrPerception>(outOfDoor, tired);

        // States
        State startState = organizerStudentFSM.CreateEntryState("Start", () => Move(initPos));
        State doorState = organizerStudentFSM.CreateSubStateMachine("Door", doorSubFSM);
        State serveDrinkState = organizerStudentFSM.CreateSubStateMachine("Serve Drink", servingSubFSM);
        State patrolState = organizerStudentFSM.CreateSubStateMachine("Patrol", patrolSubFSM);

        // Transitions
        organizerStudentFSM.CreateTransition("Door unattended", startState, goToDoor, doorState);
        organizerStudentFSM.CreateTransition("Door attended, bar unattended", startState, goToBar, serveDrinkState);
        organizerStudentFSM.CreateTransition("Door attended, bar attended", startState, goPatrol, patrolState);
        patrolSubFSM.CreateExitTransition("Bar unattended", patrolState, barUnattended, serveDrinkState);
        servingSubFSM.CreateExitTransition("Door unattended from bar", serveDrinkState, goToDoorNow, doorState);
        doorSubFSM.CreateExitTransition("BackDoor", doorState, goBackFromDoor, startState);
        servingSubFSM.CreateExitTransition("BackBar", serveDrinkState, outOfBar, startState);
    }

    public override void Update()
    {
        doorSubFSM.Update();
        servingSubFSM.Update();
        patrolSubFSM.Update();
        organizerStudentFSM.Update();

        if(isInStatePatrol.Check())
        {
            if (timer.Check())
            {
                timer.Reset();
                Move(new Vector3(Random.Range(-5, 5), Random.Range(-5, 5)));
            }
        }

        if(isInState("Door")) {
            if (tired <= 20) tired += 0.03f;
        } else
        {
            if (tired >= 0.1f) tired -= 0.03f;
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            patrolSubFSM.Fire("See trouble");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            patrolSubFSM.Fire("Student lost");
        }

        distanceToDoor = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Door").transform.position + new Vector3(-0.75f, 0, 0));
        distanceToBarOrg = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0, 0.75f, 0));
    }

    public override bool isInState(string state)
    {
        try
        {
            Perception isIn = organizerStudentFSM.CreatePerception<IsInStatePerception>(organizerStudentFSM, state);
            Perception exclusion1 = organizerStudentFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Walking to door");
            Perception exclusion2 = organizerStudentFSM.CreatePerception<IsInStatePerception>(servingSubFSM, "Walking to bar");

            if (exclusion1.Check() || exclusion2.Check())
            {
                return false;
            }

            return isIn.Check();
        }
        catch (KeyNotFoundException) {
            return false;
        }
    }

    //Behaviours
    protected void ServeDrink()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Serving drink");
        createMessage("Have a drink!", Color.blue);
        watching.getTargetCharacter().setServed();
    }

    protected void Negotiate()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I shouldn't let you go, but...");
        createMessage("I shouldn't let you go, but...", Color.blue);
        negotiateRandom = Random.value;
    }

    protected void CallTeacher()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Sir, get that kid!");
        createMessage("Sir, get that kid!", Color.blue);
    }

    public bool CheckConvinced()
    {
        return negotiateRandom > 0.5f;
    }
}