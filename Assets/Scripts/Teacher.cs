using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : Authority
{
    //parameters
    public StateMachineEngine teacherFSM;
    private StateMachineEngine teacherSubFSM;
    public StateMachineEngine patrolSubFSM;
    private StateMachineEngine chaseSubFSM;
    private StateMachineEngine punishmentRoomSubFSM;

    private WatchingPerception watchingTrouble;

    float distractionRandom;

    float distanceToMessy = 4.0f;
    float distanceToPunish = 4.0f;
    float distanceToGym = 4.0f;
    float distanceToPunishTable = 4.0f;

    private Vector3 punishPosition = new Vector3(17.5f, 31.0f, 0.0f);
    private Vector3 gymPosition = new Vector3(18.0f, 7.0f, 0.0f);
    private Vector3 punishTablePosition = new Vector3(17.5f, 36.0f, 0.0f);

    MessyStudent targetMessyStudent;

    //methods
    public Teacher(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.Teacher;
        this.strictness = 1;

        this.distractionRandom = Random.Range(2, 5);
        this.watchingTrouble = new WatchingPerception(this.gameObject, () => watchingTrouble.getTargetCharacter().getRole() != Roles.MessyStudent,
            () => watchingTrouble.getTargetCharacter().isInState("Trouble"));

        CreatePatrolSubStateMachine();
        CreatePunishmentSubStateMachine();
        CreateChaseSubStateMachine();

        animationController = this.gameObject.GetComponentInChildren<Animator>();
        if (gender == Genders.Female) animationController.SetBool("isGirl", true);
    }

    private void CreatePatrolSubStateMachine()
    {
        patrolSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception organizerPush = patrolSubFSM.CreatePerception<PushPerception>();
        Perception messyPush = patrolSubFSM.CreatePerception<PushPerception>();
        Perception talkTimer = patrolSubFSM.CreatePerception<TimerPerception>(2);
        Perception findTrouble = patrolSubFSM.CreatePerception<WatchingPerception>(watchingTrouble);
        Perception patrolTimer = patrolSubFSM.CreatePerception<TimerPerception>(2);
        Perception messyTimer = patrolSubFSM.CreatePerception<TimerPerception>(1);
        Perception messyTimer2 = patrolSubFSM.CreatePerception<TimerPerception>(1);
        // States
        State patrolingState = patrolSubFSM.CreateEntryState("Patroling", TeacherPatrol);
        State waitingForMessyState = patrolSubFSM.CreateState("Waiting For Messy", WaitForMessy);
        State warningMessyState = patrolSubFSM.CreateState("Warning Messy", TriggerMessy);
        State talkingState = patrolSubFSM.CreateState("Talking to Organizer", Talking);
        State readyToChaseState = patrolSubFSM.CreateState("Ready to Chase");
         

        // Transitions
        patrolSubFSM.CreateTransition("Keep patrolling", patrolingState, patrolTimer, patrolingState);
        patrolSubFSM.CreateTransition("Sees organizer call", patrolingState, organizerPush, talkingState);
        patrolSubFSM.CreateTransition("Stops talking to organizer", talkingState, talkTimer, readyToChaseState);
        patrolSubFSM.CreateTransition("Sees trouble", patrolingState, findTrouble, readyToChaseState);
        patrolSubFSM.CreateTransition("Pushed by messy", patrolingState, messyPush, waitingForMessyState);
        patrolSubFSM.CreateTransition("Warn messy", waitingForMessyState, messyTimer, warningMessyState);
        patrolSubFSM.CreateTransition("Pursuit messy", warningMessyState, messyTimer2, readyToChaseState);
    }

    private void CreateChaseSubStateMachine()
    {
        chaseSubFSM = new StateMachineEngine(true);

        // Perceptions
        
        Perception reachedMessy = chaseSubFSM.CreatePerception<ValuePerception>(() => distanceToMessy <= 2.0f);
        Perception timerArgue = chaseSubFSM.CreatePerception<TimerPerception>(2);
        Perception stillChasing = chaseSubFSM.CreatePerception<TimerPerception>(2);
        // States
        State chasingStudentState = chaseSubFSM.CreateEntryState("Chasing Student", ChaseMessyStudent);
        State arguingState = chaseSubFSM.CreateState("Arguing", Arguing);
        State toPunishmentRoomState = chaseSubFSM.CreateState("Taking student to punishment room", ToPunishmentRoom);
        // Transitions
        chaseSubFSM.CreateTransition("Keep chasing", chasingStudentState, stillChasing, chasingStudentState);
        chaseSubFSM.CreateTransition("Caught", chasingStudentState, reachedMessy, arguingState);
        chaseSubFSM.CreateTransition("Finish arguing", arguingState, timerArgue, toPunishmentRoomState);
    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentRoomSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception atTable = punishmentRoomSubFSM.CreatePerception<ValuePerception>(() => distanceToPunishTable <= 1.0f);
        Perception randomTimer = punishmentRoomSubFSM.CreatePerception<TimerPerception>(distractionRandom); //temporal
        Perception randomTimer2 = punishmentRoomSubFSM.CreatePerception<TimerPerception>(distractionRandom);

        // States
        State walkToTableState = punishmentRoomSubFSM.CreateEntryState("Walking to table", MoveToPRTable);
        State watchingState = punishmentRoomSubFSM.CreateState("Watching", Watching);
        State distractedState = punishmentRoomSubFSM.CreateState("Reading newspaper", ReadingNewspaper);

        // Transitions
        punishmentRoomSubFSM.CreateTransition("Start watching", walkToTableState, atTable, watchingState);
        punishmentRoomSubFSM.CreateTransition("TimerRandom1", watchingState, randomTimer, distractedState);
        punishmentRoomSubFSM.CreateTransition("TimerRandom2", distractedState, randomTimer2, watchingState);
    }

    public override void CreateStateMachine()
    {
        teacherFSM = new StateMachineEngine(false);

        //Perceptions
        Perception startPush = teacherFSM.CreatePerception<PushPerception>();

        Perception readyToChase = teacherFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Ready to Chase");

        Perception atGym = teacherFSM.CreatePerception<ValuePerception>(() => distanceToGym <= 0.5f);

        Perception loseStudentPush = teacherFSM.CreatePerception<PushPerception>();

        Perception isGoingToPR = teacherFSM.CreatePerception<IsInStatePerception>(chaseSubFSM, "Taking student to punishment room");
        Perception atPR = teacherFSM.CreatePerception<ValuePerception>(() => distanceToPunish <= 1.5f);
        Perception teacherAtPR = teacherFSM.CreatePerception<ValuePerception>(() => gameState.getPunishRoomAttended());
        Perception PRReady = teacherFSM.CreateAndPerception<AndPerception>(isGoingToPR, atPR);
        Perception notStayAtPR = teacherFSM.CreateAndPerception<AndPerception>(PRReady, teacherAtPR);

        Perception noTeacherAtPR = teacherFSM.CreatePerception<ValuePerception>(() => !gameState.getPunishRoomAttended());
        Perception stayAtPR = teacherFSM.CreateAndPerception<AndPerception>(PRReady, noTeacherAtPR);

        Perception noStudentsAtPR = teacherFSM.CreatePerception<ValuePerception>();//rellenar
        Perception exitTimer = teacherFSM.CreatePerception<TimerPerception>(1);

        Perception isInPatrol = teacherFSM.CreatePerception<IsInStatePerception>(patrolSubFSM, "Patroling");
        Perception doorUnattended = teacherFSM.CreatePerception<ValuePerception>(() => !this.gameState.getDoorAttended());
        Perception goToDoor = teacherFSM.CreateAndPerception<AndPerception>(isInPatrol, doorUnattended);

        Perception outOfDoor = teacherFSM.CreatePerception<IsInStatePerception>(doorSubFSM, "Out Door State");

        Perception thirsty = teacherFSM.CreatePerception<ValuePerception>(() => thirst > thirstThreshold);
        Perception goToDrink = teacherFSM.CreateAndPerception<AndPerception>(isInPatrol, thirsty);

        Perception isInDrink = teacherFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");

        // States
        State startState = teacherFSM.CreateEntryState("Start");
        State chaseState = teacherFSM.CreateSubStateMachine("Chase", chaseSubFSM);
        State punishmentRoomState = teacherFSM.CreateSubStateMachine("Punishment room", punishmentRoomSubFSM);
        State patrolState = teacherFSM.CreateSubStateMachine("Patrol", patrolSubFSM);
        State doorState = teacherFSM.CreateSubStateMachine("Door", doorSubFSM);
        State drinkState = teacherFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State leavePRState = teacherFSM.CreateState("Leave PR", LeavePR);
        State returnToGym = teacherFSM.CreateState("Return To Gym", ToGym);

        //Transitions
        teacherFSM.CreateTransition("Start", startState, startPush, patrolState);
        patrolSubFSM.CreateExitTransition("Sees trouble / Finishes talking to organizer", patrolState, readyToChase, chaseState);
        teacherFSM.CreateTransition("Gets to gym from punishment room", returnToGym, atGym, patrolState);
        chaseSubFSM.CreateExitTransition("Student escaped", chaseState, loseStudentPush, patrolState);
        chaseSubFSM.CreateExitTransition("Teacher at PR, returns to gym", chaseState, notStayAtPR, returnToGym);
        chaseSubFSM.CreateExitTransition("No other teacher at PR, stays", chaseState, stayAtPR, punishmentRoomState);
        punishmentRoomSubFSM.CreateExitTransition("No students left, returns to gym", punishmentRoomState, noStudentsAtPR, leavePRState);
        teacherFSM.CreateTransition("Table left, go back to gym", leavePRState,exitTimer, returnToGym);

        //patrolSubFSM.CreateExitTransition("Door unattended", patrolState, goToDoor, doorState);
        doorSubFSM.CreateExitTransition("Back from door", doorState, outOfDoor, patrolState);
        patrolSubFSM.CreateExitTransition("Need to drink", patrolState, goToDrink, drinkState);
        drinkSubFSM.CreateExitTransition("Already drank, back to Patrol", drinkState, isInDrink, patrolState);

        teacherFSM.Fire("Start");
    }

    public override void Update()
    {
        if (targetMessyStudent != null) distanceToMessy = Vector3.Distance(this.GetGameObject().transform.position, targetMessyStudent.GetGameObject().transform.position);
        distanceToPunish = Vector3.Distance(this.GetGameObject().transform.position, punishPosition);
        distanceToGym = Vector3.Distance(this.GetGameObject().transform.position, gymPosition);
        distanceToPunishTable = Vector3.Distance(this.GetGameObject().transform.position, punishTablePosition);
        doorSubFSM.Update();
        drinkSubFSM.Update();
        patrolSubFSM.Update();
        chaseSubFSM.Update();
        teacherFSM.Update();
        punishmentRoomSubFSM.Update();
        DebugInputs();
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

    public bool isInSubState(StateMachineEngine subFSM, params string[] states)
    {
        try
        {
            foreach (string state in states)
            {
                Perception isIn = teacherFSM.CreatePerception<IsInStatePerception>(subFSM, state);
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
        if (targetMessyStudent != null) { targetMessyStudent.troubleSubFSM.Fire("Busted by teacher"); }
        createMessage(11);
    }

    protected void ToPunishmentRoom()
    {
        Move(punishPosition);
    }

    protected void ToGym()
    {
        clearSprites();
        Move(gymPosition);
    }

    protected void MoveToPRTable()
    {
        Move(punishTablePosition);
    }

    protected void TeacherPatrol()
    {
        MoveToRandomGymPos();
    }

    protected void MoveToRandomGymPos()
    {
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        var index = Random.Range(0, this.gameState.possiblePosGym.Count / 2 - 1) * 2;
        currentOcuppiedPos = this.gameState.possiblePosGym.GetRange(index, 2);
        this.gameState.possiblePosGym.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
    }

    protected void ChaseMessyStudent()
    {
        if (targetMessyStudent != null)
        {
            createMessage(11);
            Move(targetMessyStudent.GetGameObject().transform.position + new Vector3(1.5f, 0.0f, 0.0f));
        }
    }

    protected void WaitForMessy()
    {
        //if (targetMessyStudent != null)  createMessage("WDYW " + targetMessyStudent.getName(), Color.blue);
    }

    public void SetMessyStudent(MessyStudent ms)
    {
        targetMessyStudent = ms;
    }
    
    protected void TriggerMessy()
    {
        Debug.Log("No entro al if");
        if (targetMessyStudent != null)
        {
            Debug.Log("Triggereo al messy");
            targetMessyStudent.troubleSubFSM.Fire("Finished bothering teacher");        
        }
    }

    //Punishment Room State FSM: Teachers
    protected void Watching()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Don't think you're gonna escape...");
        gameState.setPunishRoomAttended(true);
    }

    protected void ReadingNewspaper()
    {
        Debug.Log("[" + name + ", " + getRole() + "] President Tremp did what again?");
        distractionRandom = Random.Range(2, 5);
        //boolean isdistracted
    }

    protected void LeavePR()
    {
        gameState.setPunishRoomAttended(false);
    }

    public override string Description()
    {
        var desc = "NAME: " + getName() + ", ROLE: " + getRole() + ", STATE: " + teacherFSM.GetCurrentState().Name;

        return desc + "\n";
    }

    protected void DebugInputs()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            chaseSubFSM.Fire("Caught");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            teacherFSM.Fire("No students left, returns to gym");
        }
    }
}
