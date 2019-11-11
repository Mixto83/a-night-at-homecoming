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

    //methods
    public OrganizerStudent(string name, Genders gender, Transform obj) : base(name, gender, obj)
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
        Perception push = servingSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception timer = servingSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State waitingState = servingSubFSM.CreateEntryState("Waiting for client", Waiting);
        State serveState = servingSubFSM.CreateState("Serve", ServeDrink);

        // Transitions
        servingSubFSM.CreateTransition("New client arrives", waitingState, push, serveState);
        servingSubFSM.CreateTransition("Drink served", serveState, timer, waitingState);
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = servingSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception convinced = servingSubFSM.CreatePerception<ValuePerception>(() => CheckConvinced());
        Perception notConvinced = servingSubFSM.CreatePerception<ValuePerception>(() => !CheckConvinced());
        Perception negotiationEnd = servingSubFSM.CreatePerception<TimerPerception>(2);

        Perception negotiationEndConvinced = servingSubFSM.CreateAndPerception<AndPerception>(convinced, negotiationEnd);
        Perception negotiationEndNotConvinced = servingSubFSM.CreateAndPerception<AndPerception>(notConvinced, negotiationEnd);

        // States
        State patrollingState = patrolSubFSM.CreateEntryState("Patroling", Patrol);
        State chaseState = patrolSubFSM.CreateState("Chasing", ChaseStudent);
        State negotiateState = patrolSubFSM.CreateState("Negotiate", Negotiate);
        State callTeacherState = patrolSubFSM.CreateState("Call Teacher", CallTeacher);

        // Transitions
        servingSubFSM.CreateTransition("Sees trouble", patrollingState, push, chaseState);

        servingSubFSM.CreateTransition("Student lost", chaseState, push, patrollingState);
        servingSubFSM.CreateTransition("Messy Student Caught", chaseState, push, negotiateState);

        servingSubFSM.CreateTransition("Convinced", negotiateState, negotiationEndConvinced, patrollingState);
        servingSubFSM.CreateTransition("Not convinced", negotiateState, negotiationEndNotConvinced, callTeacherState);

        servingSubFSM.CreateTransition("Teacher got the call", callTeacherState, push, patrollingState);
    }

    private void CreateStateMachine()
    {
        organizerStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push1 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push2 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push3 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push4 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push5 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push6 = organizerStudentFSM.CreatePerception<PushPerception>(); //temporal

        // States
        State startState = organizerStudentFSM.CreateEntryState("Start", () => Move(initPos));
        State doorState = organizerStudentFSM.CreateSubStateMachine("Door", doorSubFSM);
        State serveDrinkState = organizerStudentFSM.CreateSubStateMachine("Serve Drink", servingSubFSM);
        State patrolState = organizerStudentFSM.CreateSubStateMachine("Patrol", patrolSubFSM);

        // Transitions
        organizerStudentFSM.CreateTransition("Door unattended", startState, push1, doorState);
        organizerStudentFSM.CreateTransition("Door attended, bar unattended", startState, push2, serveDrinkState);
        organizerStudentFSM.CreateTransition("Door attended, bar attended", startState, push3, patrolState);
        patrolSubFSM.CreateExitTransition("Bar unattended", patrolState, push5, serveDrinkState);
        servingSubFSM.CreateExitTransition("Door unattended from bar", serveDrinkState, push6, doorState);
        doorSubFSM.CreateExitTransition("Back", doorState, push4, startState);
    }

    public override void Update()
    {
        doorSubFSM.Update();
        servingSubFSM.Update();
        patrolSubFSM.Update();
        organizerStudentFSM.Update();

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

        //Subs
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            organizerStudentFSM.Fire("Back");
        }
    }

    //Behaviours
    protected void ServeDrink()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Serving drink");

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