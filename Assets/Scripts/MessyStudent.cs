using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    #region StateMachines
    private StateMachineEngine messyStudentFSM;
    public StateMachineEngine punishmentSubFSM;
    public StateMachineEngine troubleSubFSM;
    #endregion

    #region InteractableCharacters
    private CalmStudent targetStudent;
    private OrganizerStudent negotiatorStudent;
    private Teacher targetTeacher;

    private WatchingPerception watchingCalmStudent;
    private WatchingPerception watchingTeacher;
    private WatchingPerception watchingOrganizerStudent;
    #endregion

    #region DistancesAndPositions
    private float distanceToStartingPos = 1.5f;
    private float distanceToBench = 1.5f;
    private float distanceToPunishmentRoom = 1.5f;
    private float distanceToSafePos = 1.5f;
    private float distanceToEscapePos = 1.5f;
    private float distanceToTargetTeacher = 1.5f;
    private float distanceToTargetStudent = 1.5f;

    private Vector3 runPosition = new Vector3(-2.0f, -8.0f, 0.0f);
    private Vector3 punishPosition = new Vector3(17.5f, 31.0f, 0.0f);
    private Vector3 escapePosition = new Vector3(17.5f, 7.5f, 0.0f);
    private Vector3 targetStudentPosition = Vector3.zero;
    private Vector3 targetTeacherPosition = Vector3.zero;
    #endregion

    #region OtherParameters
    private List<CalmStudent> whiteFlagStudents;//Lista de estudiantes a los que considera chads o que ya ha molestado previamente
    private bool causingTrouble = false;
    #endregion

    //methods
    public MessyStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.MessyStudent;

        this.watchingCalmStudent = new WatchingPerception(this.gameObject, () => watchingCalmStudent.getTargetCharacter().getRole() == Roles.CalmStudent, () => !whiteFlagStudents.Contains((CalmStudent)watchingCalmStudent.getTargetCharacter()),
            () => ((CalmStudent)watchingCalmStudent.getTargetCharacter()).GetMessyFlag(), () => watchingCalmStudent.getTargetCharacter().isInState("Enjoying"));
        this.watchingTeacher = new WatchingPerception(this.gameObject, () => watchingTeacher.getTargetCharacter().getRole() == Roles.Teacher, () => watchingTeacher.getTargetCharacter().isInState("Patrol"),
            () => ((Teacher)watchingTeacher.getTargetCharacter()).isInSubState(((Teacher)watchingTeacher.getTargetCharacter()).patrolSubFSM, "Patroling"),
            () => ((Teacher)watchingTeacher.getTargetCharacter()).GetMessyFlag());
        this.watchingOrganizerStudent = new WatchingPerception(this.gameObject, () => watchingOrganizerStudent.getTargetCharacter().getRole() == Roles.OrganizerStudent);

        this.whiteFlagStudents = new List<CalmStudent>();
        this.thirst = 0.0f;
        this.fatigue = 0.0f;

        CreateTroubleSubStateMachine();
        CreatePunishmentSubStateMachine();

        animationController = this.gameObject.GetComponentInChildren<Animator>();
        if (gender == Genders.Female) animationController.SetBool("isGirl", true);
    }

    #region FSMs Methods
    private void CreateTroubleSubStateMachine()
    {
        troubleSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception searchTimer = troubleSubFSM.CreatePerception<TimerPerception>(2);

        //Ve a un alumno y decide si pelear con el o no
        Perception seesStudent = troubleSubFSM.CreatePerception<WatchingPerception>(watchingCalmStudent);
        Perception studentReached = troubleSubFSM.CreatePerception<ValuePerception>(() => distanceToTargetStudent < 0.5f);
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
        Perception barNear = troubleSubFSM.CreatePerception<ValuePerception>(() => distanceToBar < 5);
        Perception barNoticed = troubleSubFSM.CreateAndPerception<AndPerception>(barUnattended, barNear);
        Perception barSabotaged = troubleSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarSabotaged());
        Perception barNotSabotaged = troubleSubFSM.CreatePerception<ValuePerception>(() => !this.gameState.getBarSabotaged());
        Perception barAvailable = troubleSubFSM.CreateAndPerception<AndPerception>(barNoticed, barNotSabotaged);
        Perception barReached = troubleSubFSM.CreatePerception<ValuePerception>(()=> distanceToBar < 0.8f);

        Perception bustedAtBar = troubleSubFSM.CreatePerception<PushPerception>();//Push desde organizer
        Perception bustedAtBarByTeacher = troubleSubFSM.CreatePerception<PushPerception>();//Push desde teacher
        Perception drinkSabotaged = troubleSubFSM.CreatePerception<TimerPerception>(3);//Exitoso
        Perception startNegotiation = troubleSubFSM.CreatePerception<PushPerception>();//Push desde teacher
        Perception failedNegotiation = troubleSubFSM.CreatePerception<ValuePerception>(() => !this.negotiatorStudent.CheckConvinced());
        Perception successfulNegotiation = troubleSubFSM.CreatePerception<ValuePerception>(() => this.negotiatorStudent.CheckConvinced());

        //Ve profesor y le molesta
        Perception seesTeacher = troubleSubFSM.CreatePerception<WatchingPerception>(watchingTeacher);
        Perception teacherReached = troubleSubFSM.CreatePerception<ValuePerception>(() => distanceToTargetTeacher < 0.5f);
        Perception endMocking = troubleSubFSM.CreatePerception<PushPerception>();
        Perception escapedFromTeacher = troubleSubFSM.CreatePerception<ValuePerception>(() => distanceToSafePos < 0.5);
        Perception caughtByTeacher = troubleSubFSM.CreatePerception<PushPerception>();
        Perception escapeTimer = troubleSubFSM.CreatePerception<TimerPerception>(2);
        Perception arguingTimer = troubleSubFSM.CreatePerception<TimerPerception>(4);

        // States
        State lookingForTroubleState = troubleSubFSM.CreateEntryState("Looking for trouble", LookForTrouble);
        State sabotageDrinkState = troubleSubFSM.CreateState("Sabotage drink", SabotageDrink);
        State negotiationState = troubleSubFSM.CreateState("Negotiating", Negotiate);
        State chaseState = troubleSubFSM.CreateState("Being chased", Run);
        State botherTeacherState = troubleSubFSM.CreateState("Bothering teacher", BotherTeacher);
        State arguingState = troubleSubFSM.CreateState("Arguing", Arguing);
        State checkAffinityState = troubleSubFSM.CreateState("CheckingAffinity", CheckingAffinity);
        State fightState = troubleSubFSM.CreateState("Fighting", FightAsMessy);
        State movingToStudentState = troubleSubFSM.CreateState("MovingToStudent", MoveToStudent);
        State movingToTeacherState = troubleSubFSM.CreateState("MovingToTeacher", MoveToTeacher);
        State movingToBarState = troubleSubFSM.CreateState("MovingToBar", MoveToBar);
        State startingNegotiationState = troubleSubFSM.CreateState("Starting negotiation", StartNegotation);
        State escapedFromTeacherState = troubleSubFSM.CreateState("Escaped From teacher", TriggerTeacher);
        State readyToPunishmentState = troubleSubFSM.CreateState("Ready to punishment");

        // Transitions
        troubleSubFSM.CreateTransition("Nothing found", lookingForTroubleState, searchTimer, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Sees bar unattended", lookingForTroubleState, barAvailable, movingToBarState);
        troubleSubFSM.CreateTransition("Sees teacher", lookingForTroubleState, seesTeacher, movingToTeacherState);
        troubleSubFSM.CreateTransition("Sees student", lookingForTroubleState, seesStudent, movingToStudentState);

        troubleSubFSM.CreateTransition("Didn't get busted", sabotageDrinkState, drinkSabotaged, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by organizer", sabotageDrinkState, bustedAtBar, startingNegotiationState);
        troubleSubFSM.CreateTransition("Negotiation starts", startingNegotiationState, startNegotiation, negotiationState);
        troubleSubFSM.CreateTransition("Busted by teacher (bar)", sabotageDrinkState, bustedAtBarByTeacher, chaseState);

        troubleSubFSM.CreateTransition("Convinced organizer", negotiationState, successfulNegotiation, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Didn't convince organizer", negotiationState, failedNegotiation, chaseState);

        troubleSubFSM.CreateTransition("Finished bothering teacher", botherTeacherState, endMocking, chaseState);

        troubleSubFSM.CreateTransition("Escaped", chaseState, escapedFromTeacher, escapedFromTeacherState);
        troubleSubFSM.CreateTransition("Busted by teacher", chaseState, caughtByTeacher, arguingState);
        troubleSubFSM.CreateTransition("Escape ended", escapedFromTeacherState, escapeTimer, lookingForTroubleState);

        troubleSubFSM.CreateTransition("Affinity is positive", checkAffinityState, fightNegative, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Affinity is negative", checkAffinityState, fightStudent, fightState);

        troubleSubFSM.CreateTransition("Fight ends", fightState, fightEnded, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher (fight)", fightState, fightInterrupted, arguingState);
        troubleSubFSM.CreateTransition("Starting punishment", arguingState, arguingTimer, readyToPunishmentState);

        troubleSubFSM.CreateTransition("Student Reached", movingToStudentState, studentReached, checkAffinityState);
        troubleSubFSM.CreateTransition("Bar Reached", movingToBarState, barReached, sabotageDrinkState);
        troubleSubFSM.CreateTransition("Teacher Reached", movingToTeacherState, teacherReached, botherTeacherState);

    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = punishmentSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception roomReached = punishmentSubFSM.CreatePerception<ValuePerception>(() => distanceToPunishmentRoom < 0.5);
        Perception seatReached = punishmentSubFSM.CreatePerception<ValuePerception>(() => distanceToPunishPos < 0.5);
        Perception teacherDistracted = punishmentSubFSM.CreatePerception<ValuePerception>(() => this.gameState.GetTeacherDistracted());
        Perception bustedByTeacher = punishmentSubFSM.CreatePerception<PushPerception>();
        Perception reachedEscape = punishmentSubFSM.CreatePerception<ValuePerception>(() => distanceToEscapePos < 0.5);
        Perception punishTimer = punishmentSubFSM.CreatePerception<TimerPerception>(35);//Cambiar a algo mucho mas grande

        // States
        State toPunishmentRoomState = punishmentSubFSM.CreateEntryState("Being taken to punishment room", MoveToPunishment);
        State chosingSeatState = punishmentSubFSM.CreateState("Walking to Seat", MoveToRandomPunishmentRoomPos);
        State punishedState = punishmentSubFSM.CreateState("Punished", Punished);
        State escapeState = punishmentSubFSM.CreateState("Escape", Escape);
        State exitState = punishmentSubFSM.CreateState("Exit Punishment", ExitPunishment);


        // Transitions
        punishmentSubFSM.CreateTransition("Reached punishment room", toPunishmentRoomState, roomReached, chosingSeatState);
        punishmentSubFSM.CreateTransition("Reached seat", chosingSeatState, seatReached, punishedState);
        punishmentSubFSM.CreateTransition("Teacher distracted", punishedState, teacherDistracted, escapeState);
        punishmentSubFSM.CreateTransition("Teacher busts student", escapeState, bustedByTeacher, toPunishmentRoomState);
        punishmentSubFSM.CreateTransition("Time out", punishedState, punishTimer, exitState);
        punishmentSubFSM.CreateTransition("Reached escape position", escapeState, reachedEscape, exitState);
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
        Perception isInStateReadyToPunish = messyStudentFSM.CreatePerception<IsInStatePerception>(troubleSubFSM, "Ready to punishment");

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
        Perception exitTroublePunished = messyStudentFSM.CreatePerception<PushPerception>();//Eliminar

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

        troubleSubFSM.CreateExitTransition("Finished arguing", troubleState, isInStateReadyToPunish, punishmentState);
        punishmentSubFSM.CreateExitTransition("End of punishment", punishmentState, exitPunishment, troubleState);
    }
    #endregion

    #region Simulation Methods
    private void Start()
    {
        Move(initPos);
    }

    public override void Update()
    {
        
        distanceToStartingPos = Vector3.Distance(this.gameObject.transform.position, initPos);
        distanceToSafePos = Vector3.Distance(this.gameObject.transform.position, runPosition);
        distanceToEscapePos = Vector3.Distance(this.gameObject.transform.position, escapePosition);
        distanceToPunishmentRoom = Vector3.Distance(this.gameObject.transform.position, punishPosition);
        distanceToPunishPos = Vector3.Distance(this.gameObject.transform.position, randomPunishSeat);
        distanceToBar = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0, -0.75f, 0));
        if (targetStudent != null) distanceToTargetStudent = Vector3.Distance(this.gameObject.transform.position, targetStudentPosition);
        if (targetTeacher != null) distanceToTargetTeacher = Vector3.Distance(this.gameObject.transform.position, targetTeacherPosition);
        drinkSubFSM.Update();
        troubleSubFSM.Update();
        punishmentSubFSM.Update();
        messyStudentFSM.Update();
        DebugInputs();//Delete later
    }
    #endregion

    #region Behaviour Methods
    public override void LookForTrouble()
    {
        causingTrouble = false;
        if (currentOcuppiedBench != null) this.gameState.possiblePosBench.AddRange(currentOcuppiedBench);
        if (targetStudent != null) { targetStudent.SetMessyFlag(true); }
        MoveToRandomGymPos();
        //createMessage("Trouble! Size: " + whiteFlagStudents.Count + ", Thirst: "  + thirst, Color.red);
    }

    //Sabotea la bebida
    protected void SabotageDrink()
    {
        causingTrouble = true;
        thirst += 2;
        gameState.sabotageAvailable.reset();
        gameState.sabotageAvailable.start();
    }

    protected void BotherTeacher()
    {
        //fire persecucion al teacher, probablemente haga falta un estado intermedio
        //if (targetTeacher != null) targetTeacher.teacherSubFSM
        targetTeacher.patrolSubFSM.Fire("Pushed by messy");
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        createMessage(3);
    }

    private void CheckingAffinity()
    {
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        createMessage(3);
        Move(gameObject.transform.position);
        LookAt(targetStudent.GetGameObject().transform);
    }

    //Cuando llega a la posicion del personaje, comprueba la afinidad
    protected bool CheckAffinity()
    {
        //createMessage("Is this one chad or virgin?", Color.red);
        if (!whiteFlagStudents.Contains(targetStudent)) whiteFlagStudents.Add(targetStudent);
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
            //createMessage("This dude is a total chad", Color.red);
            return false;
        }
        else
        {
            return true;
        }
    }

    //Pelea cuando checkAffinity devuelve true
    protected void FightAsMessy()
    {
        fatigue++;
        createMessage(2);
    }

    //Escapar del profesor: Pillado saboteando ponche o tras molestarle
    protected void Run()
    {
        thirst++;
        fatigue++;
        Move(runPosition);//Se va a la entrada
        createMessage(12);
    }

    //Castigo
    //Inicio de castigo
    protected void Arguing()
    {
        createMessage(8);

    }
    //Escape de la sala de castigo
    protected void Escape()
    {
        thirst--;
        fatigue--;
        createMessage(12);
        Move(escapePosition);
    }
    //Fin del estado
    private void ExitPunishment()
    {
        //createMessage("I broke free!", Color.red);
        this.gameState.ChangePeoplePunished(false);
        messyStudentFSM.Fire("End of punishment");
    }

    //Negociacion tras sabotear bebida y ser pillado por organizador
    protected void StartNegotation()
    {
        negotiatorStudent = (OrganizerStudent)watchingOrganizerStudent.getTargetCharacter();
        createMessage(8);
        Move(this.gameObject.transform.position - new Vector3(0, 2, 0));
    }

    protected void Negotiate()
    {
        createMessage(8);
    }

    protected void TriggerTeacher()
    {
        clearSprites();
        if (targetTeacher != null)
        {
            targetTeacher.SetMessyFlag(true);
            targetTeacher.teacherFSM.Fire("Student escaped");
        }
        
    }

    #endregion

    #region Movement Methods
    //Se mueve a una posicion aleatoria del gimnasio
    private void MoveToRandomGymPos()
    {
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        var index = Random.Range(0, this.gameState.possiblePosGym.Count / 2 - 1) * 2;
        currentOcuppiedPos = this.gameState.possiblePosGym.GetRange(index, 2);
        this.gameState.possiblePosGym.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
    }


    //Se va hasta la sala de castigo
    private void MoveToPunishment()
    {
        //createMessage("Damn, busted!", Color.red);
        this.gameState.ChangePeoplePunished(true);
        Move(punishPosition);
    }

    //Se va hasta el estudiante calmado objetivo
    protected void MoveToStudent()
    {
        //createMessage("Found someone!", Color.red);
        targetStudent = (CalmStudent) watchingCalmStudent.getTargetCharacter();
        //createMessage(targetStudent.getName(), Color.red);
        targetStudent.SetMessyFlag(false);
        targetStudent.pushFight.Fire();//Cambiar
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        targetStudentPosition = targetStudent.GetGameObject().transform.position + new Vector3(1.5f, 0, 0);
        Move(targetStudentPosition);
    }

    //Se va hasta el profesor objetivo
    protected void MoveToTeacher()
    {
        //createMessage("Hey oh teach'!", Color.red);
        targetTeacher = (Teacher)watchingTeacher.getTargetCharacter();
        targetTeacher.SetMessyStudent(this);
        targetTeacher.SetMessyFlag(false);
        targetTeacherPosition = targetTeacher.GetGameObject().transform.position + new Vector3(1.5f, 0, 0);

        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        Move(targetTeacherPosition);
    }

    //Se va a la barra
    protected void MoveToBar()
    {
        //createMessage("Bar is free!", Color.red);
        this.gameState.setBarSabotaged(true);
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        this.Move(GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0, -1.5f, 0));
    }

    
    #endregion


public bool isCausingTrouble()
    {
        return causingTrouble;
    }

    public override bool isInState(params string[] states)
    {
        try
        {
            foreach (string state in states)
            {
                Perception isIn = messyStudentFSM.CreatePerception<IsInStatePerception>(messyStudentFSM, state);
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


    protected void DebugInputs()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            troubleSubFSM.Fire("Busted by teacher (fight)");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            troubleSubFSM.Fire("Busted by teacher (bar)");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            troubleSubFSM.Fire("Busted by organizer");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            troubleSubFSM.Fire("Negotiation starts");
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            troubleSubFSM.Fire("Convinced organizer");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            troubleSubFSM.Fire("Didn't convince organizer");
        }
    }

    public override string Description()
    {
        var desc = "NAME: " + getName() + ", ROLE: " + getRole() + ", STATE: " + messyStudentFSM.GetCurrentState().Name;

        return desc + "\n";
    }
}