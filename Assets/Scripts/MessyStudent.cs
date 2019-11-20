using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    private StateMachineEngine messyStudentFSM;
    private StateMachineEngine punishmentSubFSM;
    private StateMachineEngine troubleSubFSM;

    private CalmStudent targetStudent;
    private OrganizerStudent negotiatorStudent;
    private Teacher targetTeacher;

    private WatchingPerception watchingCalmStudent;
    private WatchingPerception watchingTeacher;
    private WatchingPerception watchingOrganizerStudent;//??

    private float distanceToStartingPos = 1.5f;
    private float distanceToBench = 1.5f;
    private float distanceToPunishmentRoom = 1.5f;
    private float distanceToSafePos = 1.5f;

    private Vector3 runVector = new Vector3(-4.5f, 6.0f, 0.0f);
    private Vector3 punishVector = new Vector3(17.5f, 31.0f, 0.0f);

    private List<CalmStudent> chadStudents;
    //methods
    public MessyStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.MessyStudent;

        this.watchingCalmStudent = new WatchingPerception(this.gameObject, () => watchingCalmStudent.getTargetCharacter().getRole() == Roles.CalmStudent, () => !chadStudents.Contains((CalmStudent)watchingCalmStudent.getTargetCharacter()),
            () => ((CalmStudent)watchingCalmStudent.getTargetCharacter()).GetMessyFlag());//
        this.watchingTeacher = new WatchingPerception(this.gameObject, () => watchingTeacher.getTargetCharacter().getRole() == Roles.Teacher);
        this.watchingOrganizerStudent = new WatchingPerception(this.gameObject, () => watchingOrganizerStudent.getTargetCharacter().getRole() == Roles.OrganizerStudent);

        this.chadStudents = new List<CalmStudent>();
        this.thirst = 0.0f;
        this.fatigue = 0.0f;

        CreateTroubleSubStateMachine();
        CreatePunishmentSubStateMachine();
    }

    private void CreateTroubleSubStateMachine()
    {
        troubleSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception searchTimer = troubleSubFSM.CreatePerception<TimerPerception>(2);

        //Ve a un alumno y decide si pelear con el o no
        Perception seesStudent = troubleSubFSM.CreatePerception<WatchingPerception>(watchingCalmStudent);
        Perception fightPositive = troubleSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity());
        Perception fightNegative = troubleSubFSM.CreatePerception<ValuePerception>(() => !CheckAffinity());
        Perception affinityTimer = troubleSubFSM.CreatePerception<TimerPerception>(5);
        Perception fightStudent = troubleSubFSM.CreateAndPerception<AndPerception>(affinityTimer, fightPositive);
        Perception ignoreStudent = troubleSubFSM.CreateAndPerception<AndPerception>(affinityTimer, fightNegative);
        Perception fightEnded = troubleSubFSM.CreatePerception<TimerPerception>(5);
        Perception fightInterrupted = troubleSubFSM.CreatePerception<PushPerception>();//Profesor interrumpe la pelea.

        //Ve la barra y decide sabotearla con o sin exito
        Perception barAttended = troubleSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());//Necesario?
        Perception barUnattended = troubleSubFSM.CreatePerception<ValuePerception>(() => !this.gameState.getBarAttended());
        Perception bustedAtBar = troubleSubFSM.CreatePerception<PushPerception>();//Push desde organizer
        Perception bustedAtBarByTeacher = troubleSubFSM.CreatePerception<PushPerception>();//Push desde teacher
        Perception drinkSabotaged = troubleSubFSM.CreatePerception<TimerPerception>(5);//Exitoso
        Perception failedNegotiation = troubleSubFSM.CreatePerception<PushPerception>(() => !this.negotiatorStudent.CheckConvinced());
        Perception successfulNegotiation = troubleSubFSM.CreatePerception<PushPerception>(() => this.negotiatorStudent.CheckConvinced());

        //Ve profesor y le molesta
        Perception seesTeacher = troubleSubFSM.CreatePerception<WatchingPerception>(watchingTeacher);
        Perception endMocking = troubleSubFSM.CreatePerception<TimerPerception>();
        Perception escapedFromTeacher = troubleSubFSM.CreatePerception<ValuePerception>(() => distanceToSafePos < 0.5);
        Perception caughtByTeacher = troubleSubFSM.CreatePerception<PushPerception>();

        // States
        State lookingForTroubleState = troubleSubFSM.CreateEntryState("Looking for trouble", LookForTrouble);
        State sabotageDrinkState = troubleSubFSM.CreateState("Sabotage drink", SabotageDrink);
        State negotiationState = troubleSubFSM.CreateState("Negotiating", Negotiate);
        State chaseState = troubleSubFSM.CreateState("Being chased", Run);
        State botherTeacherState = troubleSubFSM.CreateState("Bothering teacher", BotherTeacher);
        State arguingState = troubleSubFSM.CreateState("Arguing", Arguing);
        State checkAffinityState = troubleSubFSM.CreateState("CheckingAffinity", CheckingAffinity);
        State fightState = troubleSubFSM.CreateState("Fighting", Fight);

        // Transitions
        troubleSubFSM.CreateTransition("Nothing found", lookingForTroubleState, searchTimer, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Sees bar unattended", lookingForTroubleState, barUnattended, sabotageDrinkState);
        troubleSubFSM.CreateTransition("Sees teacher", lookingForTroubleState, seesTeacher, botherTeacherState);
        troubleSubFSM.CreateTransition("Sees student", lookingForTroubleState, seesStudent, checkAffinityState);

        troubleSubFSM.CreateTransition("Didn't get busted", sabotageDrinkState, drinkSabotaged, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by organizer", sabotageDrinkState, bustedAtBar, negotiationState);
        troubleSubFSM.CreateTransition("Busted by teacher (drink)", sabotageDrinkState, bustedAtBarByTeacher, chaseState);

        troubleSubFSM.CreateTransition("Convinced organizer", negotiationState, successfulNegotiation, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Didn't convince organizer", negotiationState, failedNegotiation, chaseState);

        troubleSubFSM.CreateTransition("Finished bothering teacher", botherTeacherState, endMocking, chaseState);

        troubleSubFSM.CreateTransition("Escaped", chaseState, escapedFromTeacher, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher", chaseState, caughtByTeacher, arguingState);

        troubleSubFSM.CreateTransition("Affinity is positive", checkAffinityState, fightNegative, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Affinity is negative", checkAffinityState, fightStudent, fightState);

        troubleSubFSM.CreateTransition("Fight ends", fightState, fightEnded, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher (fight)", fightState, fightInterrupted, arguingState);
    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = punishmentSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception roomReached = punishmentSubFSM.CreatePerception<PushPerception>(() => distanceToPunishmentRoom < 0.5);
        Perception teacherDistracted = punishmentSubFSM.CreatePerception<ValuePerception>();//TODO: Get distracted from teacher
        Perception bustedByTeacher = punishmentSubFSM.CreatePerception<PushPerception>();//TODO: Get busted by teacher

        // States
        State toPunishmentRoom = punishmentSubFSM.CreateEntryState("Being taken to punishment room", ToPunishment);
        State punishedState = punishmentSubFSM.CreateState("Punished", Punished);
        State escapeState = punishmentSubFSM.CreateState("Escape", Escape);

        // Transitions
        punishmentSubFSM.CreateTransition("Reached punishment room", toPunishmentRoom, roomReached, escapeState);
        punishmentSubFSM.CreateTransition("Teacher distracted", punishedState, teacherDistracted, escapeState);
        punishmentSubFSM.CreateTransition("Teacher busts student", escapeState, bustedByTeacher, punishedState);
    }

    public override void CreateStateMachine()
    {
        messyStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = messyStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push1 = messyStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push2 = messyStudentFSM.CreatePerception<PushPerception>(); //temporal

        //Comprueban si está cansado, sediento, ambas y cuál de las dos más
        Perception tired = messyStudentFSM.CreatePerception<ValuePerception>(() => fatigue > fatigueThreshold);
        Perception notTired = messyStudentFSM.CreatePerception<ValuePerception>(() => fatigue < fatigueThreshold);
        Perception thirsty = messyStudentFSM.CreatePerception<ValuePerception>(() => thirst > thirstThreshold);
        Perception notThirsty = messyStudentFSM.CreatePerception<ValuePerception>(() => thirst < thirstThreshold);
        Perception moreThirsty = messyStudentFSM.CreatePerception<ValuePerception>(() => thirst > fatigue);
        Perception moreTired = messyStudentFSM.CreatePerception<ValuePerception>(() => thirst < fatigue);
        Perception thirstyAndTired = messyStudentFSM.CreateAndPerception<AndPerception>(tired, thirsty);

        //Comprueban estado actual
        timeOut = messyStudentFSM.CreatePerception<TimerPerception>(2);
        isInStateDrink = messyStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");
        Perception enterParty = messyStudentFSM.CreatePerception<ValuePerception>(() => distanceToStartingPos < 0.5);
        Perception inBench = messyStudentFSM.CreatePerception<ValuePerception>(() => distanceToBench < 0.5);
        Perception isInStateTrouble = messyStudentFSM.CreatePerception<IsInStatePerception>(troubleSubFSM, "Looking for trouble");

        //Sale de beber: cansado o sin cansarse
        Perception exitDrinkTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, tired);
        Perception exitDrinkNotTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, notTired);

        //Sale de sentado: sediento o sin sed
        //En principio no son necesarios: deberia valer usando thirsty y not thirsty
        //TODO: Borrar tras comprobar
        Perception exitBenchThirsty = messyStudentFSM.CreateAndPerception<AndPerception>(inBench, thirsty);
        Perception exitBenchNotThirsty = messyStudentFSM.CreateAndPerception<AndPerception>(inBench, notThirsty);

        //Sale de trouble: sediendo, cansado (o una mas que la otra) o castigado
        Perception exitTroubleThirsty = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateTrouble, thirsty);
        Perception exitTroubleTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateTrouble, tired);
        Perception exitTroublePunished = messyStudentFSM.CreatePerception<PushPerception>();//WIP

        //Salida del castigo
        Perception exitPunishment = messyStudentFSM.CreatePerception<PushPerception>();//WIP

        // States
        State startState = messyStudentFSM.CreateEntryState("Start", Start);
        State drinkingState = messyStudentFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State benchState = messyStudentFSM.CreateState("Bench", InBench);
        State troubleState = messyStudentFSM.CreateSubStateMachine("Trouble", troubleSubFSM);
        State punishmentState = messyStudentFSM.CreateSubStateMachine("Punishment", punishmentSubFSM);

        // Transitions
        messyStudentFSM.CreateTransition("Starting trouble", startState, enterParty, troubleState);
        //messyStudentFSM.CreateTransition("Starting trouble", startState, enterParty, punishmentState);//Testing

        messyStudentFSM.CreateTransition("Out of trouble thirsty", troubleState, exitTroubleThirsty, drinkingState);
        messyStudentFSM.CreateTransition("Out of trouble tired", troubleState, exitTroubleTired, benchState);

        drinkSubFSM.CreateExitTransition("Out of drink tired", drinkingState, exitDrinkTired, benchState);
        drinkSubFSM.CreateExitTransition("Out of drink troubling", drinkingState, exitDrinkNotTired, troubleState);

        messyStudentFSM.CreateTransition("Out of bench thirsty", benchState, thirsty, drinkingState);
        messyStudentFSM.CreateTransition("Out of bench troubling", benchState, notThirsty, troubleState);

        troubleSubFSM.CreateExitTransition("Finished arguing", troubleState, exitTroublePunished, punishmentState);
        punishmentSubFSM.CreateExitTransition("End of punishment", punishmentState, exitPunishment, troubleState);
        //troubleSubFSM.CreateExitTransition("Not tired, thirsty (2)", troubleState, exitTroubleThirsty, drinkingState);
        //troubleSubFSM.CreateExitTransition("More thirsty than tired (2)", troubleState, exitTroubleTired, benchState);
    }

    private void Start()
    {
        Move(initPos);
    }

    public override void Update()
    {

        distanceToStartingPos = Vector3.Distance(this.gameObject.transform.position, initPos);
        distanceToSafePos = Vector3.Distance(this.gameObject.transform.position, runVector);
        distanceToPunishmentRoom = Vector3.Distance(this.gameObject.transform.position, punishVector);
        drinkSubFSM.Update();
        troubleSubFSM.Update();
        punishmentSubFSM.Update();
        messyStudentFSM.Update();
        DebugInputs();//Delete later
    }

    public override bool isInState(string state)
    {
        try
        {
            Perception isIn = messyStudentFSM.CreatePerception<IsInStatePerception>(messyStudentFSM, state);
            return isIn.Check();
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private void ToPunishment()
    {
        createMessage("Damn, busted!", Color.red);
        Move(punishVector);
    }

    protected void Punished()
    {
        createMessage("Waiting", Color.red);
        //Move to available seat
    }

    public override void LookForTrouble()
    {
        if (targetStudent != null) { targetStudent.SetMessyFlag(true); }
        MoveToRandomGymPos();
        createMessage("Looking for some trouble...", Color.red);
    }

    protected void SabotageDrink()
    {
        if (currentOcuppiedPos != null) this.gameState.limitedPossiblePosGym.AddRange(currentOcuppiedPos);
        this.Move(GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(1, 0, 0));
        createMessage("Drinking should be fun!", Color.red);
    }

    protected void BotherTeacher()
    {
        if (currentOcuppiedPos != null) this.gameState.limitedPossiblePosGym.AddRange(currentOcuppiedPos);
        createMessage("Bothering teacher", Color.red);
    }

    protected bool CheckAffinity()
    {
        createMessage("Is this one chad or virgin?", Color.red);
        targetStudent = (CalmStudent) watchingCalmStudent.getTargetCharacter();   
        createMessage(targetStudent.getName(), Color.red);
        targetStudent.SetMessyFlag(false);
        targetStudent.createMessage("Is " + name  + " watching me?", Color.green);

        int affinity = 0;
        
        if (Hobbies[0] == targetStudent.Hobbies[0] || Hobbies[0] == targetStudent.Hobbies[1] || Hobbies[0] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[1] == targetStudent.Hobbies[0] || Hobbies[1] == targetStudent.Hobbies[1] || Hobbies[1] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[2] == targetStudent.Hobbies[0] || Hobbies[2] == targetStudent.Hobbies[1] || Hobbies[2] == targetStudent.Hobbies[2])
            affinity++;

        if (FavAnimals[0] == targetStudent.FavAnimals[0] || FavAnimals[0] == targetStudent.FavAnimals[1] || FavAnimals[0] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[1] == targetStudent.FavAnimals[0] || FavAnimals[1] == targetStudent.FavAnimals[1] || FavAnimals[1] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[2] == targetStudent.FavAnimals[0] || FavAnimals[2] == targetStudent.FavAnimals[2] || FavAnimals[2] == targetStudent.FavAnimals[2])
            affinity++;

        if (FavFoods[0] == targetStudent.FavFoods[0] || FavFoods[0] == targetStudent.FavFoods[1] || FavFoods[0] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[1] == targetStudent.FavFoods[0] || FavFoods[1] == targetStudent.FavFoods[1] || FavFoods[1] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[2] == targetStudent.FavFoods[0] || FavFoods[2] == targetStudent.FavFoods[1] || FavFoods[2] == targetStudent.FavFoods[2])
            affinity++;

        //targetStudent.SetMessyFlag(false);
        if (affinity > affinityTolerance)
        {
            createMessage("This dude is a total chad", Color.red);
            chadStudents.Add(targetStudent);
            return false;
        }
        else
        {
            return true;
        }
    }

    protected void Fight()
    {
        createMessage("What a virgin!", Color.red);
    }

    protected void Negotiate()
    {
        createMessage("Don't snitch!", Color.red);
    }

    protected void Run()
    {
        Move(runVector);//Se va a la entrada
        createMessage("Run run run!", Color.red);
    }

    protected void Escape()
    {
        createMessage("Ight imma head out!", Color.red);
        MoveToRandomGymPos();
        messyStudentFSM.Fire("End of punishment");
    }


    protected void Arguing()
    {
        createMessage("Arguing with teacher", Color.red);
    }

    private void MoveToRandomGymPos()
    {
        if (currentOcuppiedPos != null) this.gameState.limitedPossiblePosGym.AddRange(currentOcuppiedPos);
        var index = Random.Range(0, this.gameState.limitedPossiblePosGym.Count / 2 - 1) * 2;
        currentOcuppiedPos = this.gameState.limitedPossiblePosGym.GetRange(index, 2);
        this.gameState.limitedPossiblePosGym.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
    }

    protected void DebugInputs()
    {
        //Testeo de sabotear bebida
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            troubleSubFSM.Fire("Busted by organizer");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            troubleSubFSM.Fire("Busted by teacher");
        }
    }
}