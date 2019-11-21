using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Authority
{
    //parameters
    private StateMachineEngine organizerStudentFSM;
    private StateMachineEngine servingSubFSM;
    private StateMachineEngine patrolSubFSM;

    private WatchingPerception watchingBar;
    private Perception timerPatrol;
    private Perception timerChasing;
    private Perception isInStatePatrol;
    private Perception isInStateChasing;

    float tired = 0;
    float tiredTime = 50;
    float negotiateRandom;
    protected float distanceToBarOrg;

    //methods
    public OrganizerStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.OrganizerStudent;
        this.strictness = 0.5f;

        this.watchingBar = new WatchingPerception(this.gameObject, () => watchingBar.getTargetCharacter().getCanBeServed(),
            () => watchingBar.getTargetCharacter().getRole() != Roles.OrganizerStudent);

        CreateServingSubStateMachine();
        CreatePatrolSubStateMachine();

        this.negotiateRandom = Random.value;
    }

    private void CreateServingSubStateMachine()
    {
        servingSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToBar = servingSubFSM.CreatePerception<ValuePerception>(() => distanceToBarOrg < 0.3f);
        WatchingPerception seeSomeone = servingSubFSM.CreatePerception<WatchingPerception>(watchingBar);
        Perception timer = servingSubFSM.CreatePerception<TimerPerception>(2);
        Perception barAttended = servingSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());

        // States
        State walkingState = servingSubFSM.CreateEntryState("Walking to bar", () => Walking("Bar", new Vector3(0.75f, 0, 0)));
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
        Perception push = patrolSubFSM.CreatePerception<PushPerception>();
        WatchingPerception seeMessyStudentInTroubleState = patrolSubFSM.CreatePerception<WatchingPerception>(watchingMessy);
        Perception caughtStudent = patrolSubFSM.CreatePerception<ValuePerception>(() => targetStudent != null ? Vector3.Distance(targetStudent.GetGameObject().transform.position, GetGameObject().transform.position) < 1 : false);
        Perception lostSightOfStudent = patrolSubFSM.CreatePerception<ValuePerception>(() => targetStudent != null ? Vector3.Distance(targetStudent.GetGameObject().transform.position, GetGameObject().transform.position) >= 15 : false);
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

        timerPatrol = patrolSubFSM.CreatePerception<TimerPerception>(5);
        timerChasing = patrolSubFSM.CreatePerception<TimerDecoratorNode>(1);
        isInStatePatrol = patrolSubFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Patroling");
        isInStateChasing = patrolSubFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Chasing");

        // Transitions
        patrolSubFSM.CreateTransition("See trouble", patrollingState, seeMessyStudentInTroubleState, chaseState);

        patrolSubFSM.CreateTransition("Student lost", chaseState, lostSightOfStudent, patrollingState);
        patrolSubFSM.CreateTransition("Messy Student Caught", chaseState, caughtStudent, negotiateState);

        patrolSubFSM.CreateTransition("Convinced", negotiateState, negotiationEndConvinced, patrollingState);
        patrolSubFSM.CreateTransition("Not convinced", negotiateState, negotiationEndNotConvinced, callTeacherState);

        patrolSubFSM.CreateTransition("Teacher got the call", callTeacherState, push, patrollingState); //Hacer push desde Teacher
    }

    public override void CreateStateMachine()
    {
        organizerStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception tired = organizerStudentFSM.CreatePerception<ValuePerception>(() => this.tired >= tiredTime);
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
            if (timerPatrol.Check())
            {
                timerPatrol.Reset();

                if(currentOcuppiedPos != null) this.gameState.limitedPossiblePosGym.AddRange(currentOcuppiedPos);

                var index = Random.Range(0, this.gameState.limitedPossiblePosGym.Count / 2 - 1) * 2;
                currentOcuppiedPos = this.gameState.limitedPossiblePosGym.GetRange(index, 2);
                this.gameState.limitedPossiblePosGym.RemoveRange(index, 2);

                Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
            }
        }

        if (isInStateChasing.Check())
        {
            if (timerChasing.Check())
            {
                if (targetStudent != null)
                {
                    timerChasing.Reset();

                    Vector3 offset = GetGameObject().transform.position - targetStudent.GetGameObject().transform.position;
                    Move(targetStudent.GetGameObject().transform.position - offset.normalized);
                }
            }
        }

        if (isInState("Door")) {
            if (tired <= tiredTime) tired += 0.03f;
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
        distanceToBarOrg = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0.75f, 0, 0));
    }

    public override bool isInState(params string[] states)
    {
        try
        {
            foreach (string state in states)
            {
                Perception isIn = organizerStudentFSM.CreatePerception<IsInStatePerception>(organizerStudentFSM, state);
                Perception exclusion1 = organizerStudentFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Walking to door");
                Perception exclusion2 = organizerStudentFSM.CreatePerception<IsInStatePerception>(servingSubFSM, "Walking to bar");

                if (state == "Door" && (exclusion1.Check() || exclusion2.Check()))
                {
                    break;
                }

                if (isIn.Check())
                    return true;
            }

            return false;
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
        watchingBar.getTargetCharacter().setServed();
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

    public override string Description()
    {
        var desc = "NAME: " + getName() + "ROLE: " + getRole() + ", STATE: " + organizerStudentFSM.GetCurrentState().Name;

        return desc + "\n";
    }
}