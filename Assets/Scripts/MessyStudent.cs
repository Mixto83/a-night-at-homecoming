using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    private StateMachineEngine messyStudentFSM;
    private StateMachineEngine punishmentSubFSM;
    private StateMachineEngine troubleSubFSM;

    private Student targetStudent;
    private OrganizerStudent negotiatorStudent;
    private Teacher targetTeacher;

    private WatchingPerception watchingCalmStudent;
    private WatchingPerception watchingTeacher;
    private WatchingPerception watchingBar;
    private WatchingPerception watchingOrganizerStudent;//??

    //methods
    public MessyStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.MessyStudent;
        //this.watching = new WatchingPerception(this.gameObject, "CalmStudent", this.gameObject.GetComponentInChildren<MeshCollider>(), "Anywhere");


        CreateTroubleSubStateMachine();
        CreatePunishmentSubStateMachine();
        CreateStateMachine();
    }

    private void CreateTroubleSubStateMachine()
    {
        troubleSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception timer = troubleSubFSM.CreatePerception<TimerPerception>(2);

        //Perception seesStudent = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        //Perception affinityCheck = troubleSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity());
        WatchingPerception seesStudent = troubleSubFSM.CreatePerception<WatchingPerception>(watchingCalmStudent);
        Perception fightPositive = troubleSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity());
        Perception fightNegative = troubleSubFSM.CreatePerception<ValuePerception>(() => !CheckAffinity());
        Perception fightStudent = troubleSubFSM.CreateAndPerception<AndPerception>(timer, fightPositive);
        Perception ignoreStudent = troubleSubFSM.CreateAndPerception<AndPerception>(timer, fightNegative);
        Perception fightEnded = troubleSubFSM.CreatePerception<TimerPerception>(5);//And?
        Perception fightInterrupted = troubleSubFSM.CreatePerception<ValuePerception>();//Metodo

        Perception barUnattended = troubleSubFSM.CreatePerception<ValuePerception>(() => !this.gameState.getBarAttended());
        Perception barAttended = troubleSubFSM.CreatePerception<ValuePerception>(() => this.gameState.getBarAttended());
        Perception seesBar = troubleSubFSM.CreatePerception<WatchingPerception>(watchingBar);
        Perception goesToBar = troubleSubFSM.CreateAndPerception<AndPerception>(seesBar, barUnattended);
        Perception bustedAtBar = troubleSubFSM.CreatePerception<ValuePerception>();//Metodo
        Perception bustedAtBarByTeacher = troubleSubFSM.CreatePerception<ValuePerception>();//Metodo
        Perception notBustedAtBar = troubleSubFSM.CreatePerception<ValuePerception>();//Metodo
        Perception drinkSabotaged = troubleSubFSM.CreateAndPerception<AndPerception>(timer, notBustedAtBar);
        Perception failedNegotiation = troubleSubFSM.CreatePerception<PushPerception>(() => !this.negotiatorStudent.CheckConvinced());
        Perception successfulNegotiation = troubleSubFSM.CreatePerception<PushPerception>(() => this.negotiatorStudent.CheckConvinced());

        WatchingPerception seesTeacher = troubleSubFSM.CreatePerception<WatchingPerception>(watchingTeacher);//Should change somehow
        Perception endMocking = troubleSubFSM.CreatePerception<PushPerception>();
        Perception escapedFromTeacher = troubleSubFSM.CreatePerception<TimerPerception>(2);
        Perception caughtByTeacher = troubleSubFSM.CreatePerception<ValuePerception>();//Fill


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
        troubleSubFSM.CreateTransition("Sees bar unattended", lookingForTroubleState, barUnattended, sabotageDrinkState);
        troubleSubFSM.CreateTransition("Sees teacher", lookingForTroubleState, seesTeacher, botherTeacherState);
        troubleSubFSM.CreateTransition("Sees student", lookingForTroubleState, seesStudent, checkAffinityState); //This perception will return in some way a Student, and assign it to this.targetStudent

        troubleSubFSM.CreateTransition("Didn't get busted", sabotageDrinkState, drinkSabotaged, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by organizer", sabotageDrinkState, bustedAtBar, negotiationState);
        troubleSubFSM.CreateTransition("Busted by teacher (drink)", sabotageDrinkState, bustedAtBarByTeacher, chaseState);

        troubleSubFSM.CreateTransition("Convinced organizer", negotiationState, successfulNegotiation, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Didn't convince organizer", negotiationState, failedNegotiation, chaseState);

        troubleSubFSM.CreateTransition("Escaped", chaseState, escapedFromTeacher, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher", chaseState, caughtByTeacher, arguingState);

        troubleSubFSM.CreateTransition("Finished bothering teacher", botherTeacherState, endMocking, chaseState);

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
        Perception roomReached = punishmentSubFSM.CreatePerception<PushPerception>();
        Perception teacherDistracted = punishmentSubFSM.CreatePerception<WatchingPerception>();//Introducir metodo que compruebe profesor
        Perception bustedByTeacher = punishmentSubFSM.CreatePerception<PushPerception>();//???

        // States
        State toPunishmentRoom = punishmentSubFSM.CreateEntryState("Being taken to punishment room");
        State punishedState = punishmentSubFSM.CreateState("Punished", Punished);
        State escapeState = punishmentSubFSM.CreateState("Escape", Escape);

        // Transitions
        punishmentSubFSM.CreateTransition("Reached punishment room", toPunishmentRoom, roomReached, punishedState);
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
        
        timeOut = messyStudentFSM.CreatePerception<TimerPerception>(2);
        isInStateDrink = messyStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");
        Perception exitDrink = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, timeOut); //not used
        Perception exitDrinkTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, push);
        Perception exitDrinkNotTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, push);

        Perception exitBenchThirsty = messyStudentFSM.CreatePerception<PushPerception>();
        Perception exitBenchNotThirsty = messyStudentFSM.CreatePerception<PushPerception>();

        Perception exitTroubleThirsty = messyStudentFSM.CreatePerception<PushPerception>();//??
        Perception exitTroubleTired = messyStudentFSM.CreatePerception<PushPerception>();//??
        Perception exitTroublePunished = messyStudentFSM.CreatePerception<PushPerception>();//??
        Perception exitPunishment = messyStudentFSM.CreatePerception<PushPerception>();//??

        // States
        State startState = messyStudentFSM.CreateEntryState("Start");
        State drinkingState = messyStudentFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State benchState = messyStudentFSM.CreateState("Bench", InBench);
        State troubleState = messyStudentFSM.CreateSubStateMachine("Trouble", troubleSubFSM);
        State punishmentState = messyStudentFSM.CreateSubStateMachine("Punishment", punishmentSubFSM);

        // Transitions
        messyStudentFSM.CreateTransition("More thirsty than tired (1)", startState, push, drinkingState);
        messyStudentFSM.CreateTransition("More tired than thirsty", startState, push1, benchState);
        messyStudentFSM.CreateTransition("Not thirsty, not tired (1)", startState, push2, troubleState);

        drinkSubFSM.CreateExitTransition("Not thirsty, tired", drinkingState, exitDrinkTired, benchState);
        drinkSubFSM.CreateExitTransition("Not thirsty, not tired (2)", drinkingState, exitDrinkNotTired, troubleState);

        messyStudentFSM.CreateTransition("Not tired, thirsty (1)", benchState, exitBenchThirsty, drinkingState);
        messyStudentFSM.CreateTransition("Not thirsty, not tired (3)", benchState, exitBenchNotThirsty, troubleState);

        troubleSubFSM.CreateExitTransition("Not tired, thirsty (2)", troubleState, exitTroubleThirsty, drinkingState);
        troubleSubFSM.CreateExitTransition("More thirsty than tired (2)", troubleState, exitTroubleTired, benchState);
        troubleSubFSM.CreateExitTransition("Finished arguing", troubleState, exitTroublePunished, punishmentState);

        punishmentSubFSM.CreateExitTransition("End of punishment", punishmentState, exitPunishment, startState);
    }

    public override void Update()
    {
        drinkSubFSM.Update();
        troubleSubFSM.Update();
        punishmentSubFSM.Update();
        messyStudentFSM.Update();
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SabotageDrink();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            troubleSubFSM.Fire("Sees student");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            troubleSubFSM.Fire("Affinity is negative");

            if (!isInStateDrink.Check())
            {
                timeOut.Reset();
            }
        }
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

    public override void LookForTrouble()
    {     
        Vector3 target = new Vector3(Random.Range(0,8), Random.Range(0, 8), 0);
        Move(target);
        createMessage("Looking for some trouble...", Color.red);
    }

    protected void SabotageDrink()
    {
        this.Move(GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(0, 1, 0));
        createMessage("Drinking should be fun!", Color.red);
    }

    protected void BotherTeacher()
    {
        
        createMessage("Bothering teacher", Color.red);
    }

    protected bool CheckAffinity()
    {
        createMessage("Is this one chad or virgin?", Color.red);

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

        if (affinity > affinityTolerance)
        {
            createMessage("This dude is a total chad", Color.red);
            return false;
        }
        else
        {
            createMessage("What a virgin!", Color.red);
            return true;
        }
    }

    protected void Negotiate()
    {
        createMessage("Don't snitch!", Color.red);
    }

    protected void Run()
    {
        createMessage("Run run run!", Color.red);
    }

    protected void Escape()
    {
        Move(new Vector3(0, 0, 0));//Needs to move out
        createMessage("Ight imma head out!", Color.red);
    }

    protected void Arguing()
    {
        createMessage("Arguing with teacher", Color.red);
    }
}