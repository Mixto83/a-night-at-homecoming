using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : Authority
{
    //parameters
    private StateMachineEngine teacherFSM;
    private StateMachineEngine teacherSubFSM;
    private StateMachineEngine patrolSubFSM;
    private StateMachineEngine chaseSubFSM;
    private StateMachineEngine punishmentRoomSubFSM;

    State patrolState;

    float distractionRandom;

    //methods
    public Teacher(string name, Genders gender, Vector3 position) : base(name, gender, position)
    {
        this.role = Roles.Teacher;
        this.strictness = 1;

        this.distractionRandom = Random.Range(2, 5);
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = patrolSubFSM.CreatePerception<PushPerception>(); //temporal

        // States
        State patrolingState = patrolSubFSM.CreateEntryState("Patroling", Patrol);
        State talkingState = patrolSubFSM.CreateState("Talking to Organizer", Talking);

        // Transitions
        patrolSubFSM.CreateTransition("Sees organizer call", patrolingState, push, talkingState);
    }

    private void CreateChaseSubStateMachine()
    {
        chaseSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = chaseSubFSM.CreatePerception<PushPerception>(); //temporal

        // States
        State chasingStudentState = chaseSubFSM.CreateEntryState("Chasing Student", ChaseStudent);
        State arguingState = chaseSubFSM.CreateState("Arguing", Arguing);
        State toPunishmentRoomState = chaseSubFSM.CreateState("Taking student to punishment room", ToPunishmentRoom);

        // Transitions
        chaseSubFSM.CreateTransition("Caught", chasingStudentState, push, arguingState);
        chaseSubFSM.CreateTransition("Finish arguing", chasingStudentState, push, toPunishmentRoomState);
    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentRoomSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = punishmentRoomSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception randomTimer = punishmentRoomSubFSM.CreatePerception<TimerPerception>(distractionRandom); //temporal

        // States
        State watchingState = punishmentRoomSubFSM.CreateEntryState("Watching", Watching);
        State distractedState = punishmentRoomSubFSM.CreateState("Reading newspaper", ReadingNewspaper);

        // Transitions
        punishmentRoomSubFSM.CreateTransition("TimerRandom1", watchingState, randomTimer, distractedState);
        punishmentRoomSubFSM.CreateTransition("TimerRandom2", distractedState, randomTimer, watchingState);
    }

    private void CreateSubStateMachine()
    {
        teacherSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = teacherSubFSM.CreatePerception<PushPerception>(); //temporal

        // States
        patrolState = teacherSubFSM.CreateSubStateMachine("Patrol", patrolSubFSM);
        State doorState = teacherSubFSM.CreateSubStateMachine("Door", doorSubFSM);
        State drinkState = teacherSubFSM.CreateSubStateMachine("Drink", drinkSubFSM);

        // Transitions
        patrolSubFSM.CreateExitTransition("Door unattended", patrolState, push, doorState);
        patrolSubFSM.CreateExitTransition("Thirsty", patrolState, push, drinkState);

        drinkSubFSM.CreateExitTransition("Not thirsty", drinkState, push, patrolState);
    }

    private void CreateStateMachine()
    {
        teacherFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = teacherFSM.CreatePerception<PushPerception>(); //temporal
        Perception escaped = teacherFSM.CreatePerception<PushPerception>(); //temporal
        Perception reachedPR = teacherFSM.CreatePerception<PushPerception>(); //temporal
        Perception alreadyTecherAtPR = teacherFSM.CreatePerception<PushPerception>(); //temporal
        Perception noTecherAtPR = teacherFSM.CreatePerception<PushPerception>(); //temporal

        Perception haveToStayAtPR = teacherFSM.CreateAndPerception<AndPerception>(reachedPR, noTecherAtPR);
        Perception notHaveToStayAtPR = teacherFSM.CreateAndPerception<AndPerception>(reachedPR, alreadyTecherAtPR);
        Perception endChasingState = teacherFSM.CreateOrPerception<OrPerception>(escaped, notHaveToStayAtPR);

        // States
        State startState = teacherFSM.CreateEntryState("Start");
        State subFSMState = teacherFSM.CreateSubStateMachine("SubState", teacherSubFSM, patrolState);
        State chaseState = teacherFSM.CreateSubStateMachine("Chase", chaseSubFSM);
        State punihsmentRoomState = teacherFSM.CreateSubStateMachine("Punishment room", punishmentRoomSubFSM);

        // Transitions
        teacherFSM.CreateTransition("Start", startState, push, subFSMState);
        teacherSubFSM.CreateExitTransition("Sees trouble / Finishes talking to organizer", subFSMState, push, chaseState);

        chaseSubFSM.CreateExitTransition("Student escaped or reached punishment room. There's already a teacher", chaseState, endChasingState, subFSMState);
        chaseSubFSM.CreateExitTransition("Reached punishment room. There's no techer", chaseState, haveToStayAtPR, punihsmentRoomState);
        punishmentRoomSubFSM.CreateExitTransition("No more students at punishment room", punihsmentRoomState, push, subFSMState);

        teacherFSM.Fire("Start");
    }

    public override void Update()
    {
        doorSubFSM.Update();
        patrolSubFSM.Update();
    }

    protected void Talking()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm watching you...");
    }

    protected void Arguing()
    {
        Debug.Log("[" + name + ", " + getRole() + "] You're going to be punished for this!!");
    }

    protected void ToPunishmentRoom()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Taking student to punishment room");
    }

    //Punishment Room State FSM: Teachers
    protected void Watching()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Don't think you're gonna escape...");
    }

    protected void ReadingNewspaper()
    {
        Debug.Log("[" + name + ", " + getRole() + "] President Tremp did what again?");
        distractionRandom = Random.Range(2, 5);
    }

}
