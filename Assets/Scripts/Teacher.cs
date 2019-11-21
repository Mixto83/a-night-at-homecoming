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

    private WatchingPerception watchingTrouble;

    State patrolState;

    float distractionRandom;

    //methods
    public Teacher(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.Teacher;
        this.strictness = 1;

        this.distractionRandom = Random.Range(2, 5);
        this.watchingTrouble = new WatchingPerception(this.gameObject, () => watchingTrouble.getTargetCharacter().getRole() != Roles.MessyStudent, () => ((MessyStudent)watchingTrouble.getTargetCharacter()).isCausingTrouble());

        CreatePatrolSubStateMachine();
        CreatePunishmentSubStateMachine();
        CreateChaseSubStateMachine();
        CreateSubStateMachine();
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception organizerPush = patrolSubFSM.CreatePerception<PushPerception>();
        Perception talkTimer = patrolSubFSM.CreatePerception<TimerPerception>(2);
        Perception findTrouble = patrolSubFSM.CreatePerception<WatchingPerception>(watchingTrouble);
        Perception patrolTimer = patrolSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State patrolingState = patrolSubFSM.CreateEntryState("Patroling", TeacherPatrol);
        State talkingState = patrolSubFSM.CreateState("Talking to Organizer", Talking);
        State readyToChaseState = patrolSubFSM.CreateState("Ready to Chase");

        // Transitions
        patrolSubFSM.CreateTransition("Keep patrolling", patrolingState, patrolTimer, patrolingState);
        patrolSubFSM.CreateTransition("Sees organizer call", patrolingState, organizerPush, talkingState);
        patrolSubFSM.CreateTransition("Stops talking to organizer", talkingState, talkTimer, readyToChaseState);
        patrolSubFSM.CreateTransition("Sees trouble", patrolingState, findTrouble, readyToChaseState);
    }

    private void CreateChaseSubStateMachine()
    {
        chaseSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception distanceToStudent = chaseSubFSM.CreatePerception<ValuePerception>();//Rellenar
        Perception timerArgue = chaseSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State chasingStudentState = chaseSubFSM.CreateEntryState("Chasing Student", ChaseStudent);
        State arguingState = chaseSubFSM.CreateState("Arguing", Arguing);
        State toPunishmentRoomState = chaseSubFSM.CreateState("Taking student to punishment room", ToPunishmentRoom);

        // Transitions
        chaseSubFSM.CreateTransition("Caught", chasingStudentState, distanceToStudent, arguingState);
        chaseSubFSM.CreateTransition("Finish arguing", chasingStudentState, timerArgue, toPunishmentRoomState);
    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentRoomSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception randomTimer = punishmentRoomSubFSM.CreatePerception<TimerPerception>(distractionRandom); //temporal
        Perception randomTimer2 = punishmentRoomSubFSM.CreatePerception<TimerPerception>(distractionRandom);

        // States
        State watchingState = punishmentRoomSubFSM.CreateEntryState("Watching", Watching);
        State distractedState = punishmentRoomSubFSM.CreateState("Reading newspaper", ReadingNewspaper);

        // Transitions
        punishmentRoomSubFSM.CreateTransition("TimerRandom1", watchingState, randomTimer, distractedState);
        punishmentRoomSubFSM.CreateTransition("TimerRandom2", distractedState, randomTimer2, watchingState);
    }


    private void CreateSubStateMachine()
    {
        teacherSubFSM = new StateMachineEngine(true);

        //Perceptions
        Perception isInPatrol = teacherSubFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Patroling");

        Perception doorAttended = teacherSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getDoorAttended());
        Perception doorUnattended = teacherSubFSM.CreatePerception<ValuePerception>(() => !this.gameState.getDoorAttended());

        Perception goToDoor = teacherSubFSM.CreateAndPerception<AndPerception>(isInPatrol, doorUnattended);

        Perception outOfDoor = teacherSubFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Out Door State");

        Perception thirsty = teacherSubFSM.CreatePerception<ValuePerception>(() => thirst > thirstThreshold);
        Perception goToDrink = teacherSubFSM.CreateAndPerception<AndPerception>(isInPatrol, thirsty);

        Perception isInDrink = teacherSubFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");

        // States
        patrolState = teacherSubFSM.CreateSubStateMachine("Patrol", patrolSubFSM);
        State doorState = teacherSubFSM.CreateSubStateMachine("Door", doorSubFSM);
        State drinkState = teacherSubFSM.CreateSubStateMachine("Drink", drinkSubFSM);

        //Transitions
        patrolSubFSM.CreateExitTransition("Door unattended", patrolState, goToDoor, doorState);
        doorSubFSM.CreateExitTransition("Back from door", doorState, outOfDoor, patrolState);
        patrolSubFSM.CreateExitTransition("Need to drink", patrolState, goToDrink, drinkState);
        drinkSubFSM.CreateExitTransition("Already drank, back to Patrol", drinkState, isInDrink, patrolState);
    }

    public override void CreateStateMachine()
    {
        teacherFSM = new StateMachineEngine(false);

        //Perceptions
        Perception startPush = teacherFSM.CreatePerception<PushPerception>();

        Perception readyToChase = teacherFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Ready to Chase");

        Perception atGym = teacherFSM.CreatePerception<ValuePerception>();//rellenar

        Perception loseStudentPush = teacherFSM.CreatePerception<PushPerception>();

        Perception isGoingToPR = teacherFSM.CreatePerception<IsInStatePerception>(chaseSubFSM, "Taking student to punishment room");
        Perception atPR = teacherFSM.CreatePerception<ValuePerception>();//rellenar
        Perception teacherAtPR = teacherFSM.CreatePerception<ValuePerception>();//rellenar
        Perception PRReady = teacherFSM.CreateAndPerception<AndPerception>(isGoingToPR, atPR);
        Perception notStayAtPR = teacherFSM.CreateAndPerception<AndPerception>(PRReady, teacherAtPR);

        Perception teacherNotAtPR = teacherFSM.CreatePerception<ValuePerception>();//rellenar
        Perception stayAtPR = teacherFSM.CreateAndPerception<AndPerception>(PRReady, teacherNotAtPR);

        Perception noStudentsAtPR = teacherFSM.CreatePerception<ValuePerception>();//rellenar

        // States
        State startState = teacherFSM.CreateEntryState("Start");
        State subFSMState = teacherFSM.CreateSubStateMachine("SubState", teacherSubFSM, patrolState);
        State chaseState = teacherFSM.CreateSubStateMachine("Chase", chaseSubFSM);
        State punishmentRoomState = teacherFSM.CreateSubStateMachine("Punishment room", punishmentRoomSubFSM);
        State returnToGym = teacherFSM.CreateState("Return To Gym", ToGym);

        //Transitions
        teacherFSM.CreateTransition("Start", startState, startPush, subFSMState);
        teacherSubFSM.CreateExitTransition("Sees trouble / Finishes talking to organizer", subFSMState, readyToChase, chaseState);
        teacherFSM.CreateTransition("Gets to gym from punishment room", returnToGym, atGym, subFSMState);
        chaseSubFSM.CreateExitTransition("Student escaped", chaseState, loseStudentPush, subFSMState);
        chaseSubFSM.CreateExitTransition("Teacher at PR, returns to gym", chaseState, notStayAtPR, returnToGym);
        chaseSubFSM.CreateExitTransition("No other teacher at PR, stays", chaseState, stayAtPR, punishmentRoomState);
        punishmentRoomSubFSM.CreateExitTransition("No students left, returns to gym", punishmentRoomState, noStudentsAtPR, returnToGym);

        teacherFSM.Fire("Start");
    }

    public override void Update()
    {
        doorSubFSM.Update();
        patrolSubFSM.Update();
        teacherFSM.Update();
    }

    public override bool isInState(params string[] states)
    {
        try {
            foreach (string state in states)
            {
                Perception isIn = teacherFSM.CreatePerception<IsInStatePerception>(teacherFSM, state);
                if (isIn.Check())
                    return true;
            }

            return false;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
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

    protected void ToGym()
    {

    }

    protected void TeacherPatrol()
    {
        MoveToRandomGymPos();
    }

    protected void MoveToRandomGymPos()
    {
        if (currentOcuppiedPos != null) this.gameState.limitedPossiblePosGym.AddRange(currentOcuppiedPos);
        var index = Random.Range(0, this.gameState.limitedPossiblePosGym.Count / 2 - 1) * 2;
        currentOcuppiedPos = this.gameState.limitedPossiblePosGym.GetRange(index, 2);
        this.gameState.limitedPossiblePosGym.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
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
        //boolean isdistracted
    }

    public override string Description()
    {
        var desc = "NAME: " + getName() + "ROLE: " + getRole() + ", STATE: " + teacherFSM.GetCurrentState().Name;

        return desc + "\n";
    }
}
