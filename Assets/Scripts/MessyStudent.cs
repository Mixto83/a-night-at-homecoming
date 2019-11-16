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

    //methods
    public MessyStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.MessyStudent;

        CreateTroubleSubStateMachine();
        CreatePunishmentSubStateMachine();
        CreateStateMachine();
    }

    private void CreateTroubleSubStateMachine()
    {
        troubleSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception seesStudent = troubleSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception timer = troubleSubFSM.CreatePerception<TimerPerception>(2);
        Perception affinityCheck = troubleSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity());
        Perception fightStudent = troubleSubFSM.CreateOrPerception<OrPerception>(timer, affinityCheck);

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
        troubleSubFSM.CreateTransition("Sees bar unattended", lookingForTroubleState, push, sabotageDrinkState);
        troubleSubFSM.CreateTransition("Sees teacher", lookingForTroubleState, push, botherTeacherState);
        troubleSubFSM.CreateTransition("Sees student", lookingForTroubleState, seesStudent, checkAffinityState); //This perception will return in some way a Student, and assign it to this.targetStudent

        troubleSubFSM.CreateTransition("Didn't get busted", sabotageDrinkState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by organizer", sabotageDrinkState, push, negotiationState);
        troubleSubFSM.CreateTransition("Busted by teacher (drink)", sabotageDrinkState, push, chaseState);

        troubleSubFSM.CreateTransition("Convinced organizer", negotiationState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Didn't convince organizer", negotiationState, push, chaseState);

        troubleSubFSM.CreateTransition("Escaped", chaseState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher", chaseState, push, arguingState);

        troubleSubFSM.CreateTransition("Finished bothering teacher", botherTeacherState, push, chaseState);

        troubleSubFSM.CreateTransition("Affinity is positive", checkAffinityState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Affinity is negative", checkAffinityState, fightStudent, fightState);

        troubleSubFSM.CreateTransition("Fight ends", fightState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher (fight)", fightState, push, arguingState);
    }

    private void CreatePunishmentSubStateMachine()
    {
        punishmentSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = punishmentSubFSM.CreatePerception<PushPerception>(); //temporal

        // States
        State toPunishmentRoom = punishmentSubFSM.CreateEntryState("Being taken to punishment room");
        State punishedState = punishmentSubFSM.CreateState("Punished", Punished);
        State escapeState = punishmentSubFSM.CreateState("Escape", Escape);

        // Transitions
        punishmentSubFSM.CreateTransition("Reached punishment room", toPunishmentRoom, push, punishedState);
        punishmentSubFSM.CreateTransition("Teacher distracted", punishedState, push, escapeState);
        punishmentSubFSM.CreateTransition("Teacher busts student", escapeState, push, punishedState);
    }

    private void CreateStateMachine()
    {
        messyStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = messyStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push1 = messyStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception push2 = messyStudentFSM.CreatePerception<PushPerception>(); //temporal

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

        drinkSubFSM.CreateExitTransition("Not thirsty, tired", drinkingState, push, benchState);
        drinkSubFSM.CreateExitTransition("Not thirsty, not tired (2)", drinkingState, push, troubleState);

        messyStudentFSM.CreateTransition("Not tired, thirsty (1)", benchState, push, drinkingState);
        messyStudentFSM.CreateTransition("Not thirsty, not tired (3)", benchState, push, troubleState);

        troubleSubFSM.CreateExitTransition("Not tired, thirsty (2)", troubleState, push, drinkingState);
        troubleSubFSM.CreateExitTransition("More thirsty than tired (2)", troubleState, push, benchState);
        troubleSubFSM.CreateExitTransition("Finished arguing", troubleState, push, punishmentState);

        punishmentSubFSM.CreateExitTransition("End of punishment", punishmentState, push, startState);
    }

    public override void Update()
    {
        drinkSubFSM.Update();
        troubleSubFSM.Update();
        punishmentSubFSM.Update();
        messyStudentFSM.Update();

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            messyStudentFSM.Fire("Not thirsty, not tired (1)");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            troubleSubFSM.Fire("Sees student");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            troubleSubFSM.Fire("Affinity is negative");
        }
    }

    public override bool isInState(string subFSM, string subState)
    {
        Perception isIn;
        switch (subFSM)
        {
            case "Drink":
                isIn = messyStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, subState);
                break;
            case "Trouble":
                isIn = messyStudentFSM.CreatePerception<IsInStatePerception>(troubleSubFSM, subState);
                break;
            case "Punishment":
                isIn = messyStudentFSM.CreatePerception<IsInStatePerception>(punishmentSubFSM, subState);
                break;
            default:
                isIn = messyStudentFSM.CreatePerception<PushPerception>();
                break;
        }

        return isIn.Check();
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
        Move(new Vector3(0, 0, 0));//Needs to move to dancing floor
        createMessage("Ight imma head out!", Color.red);
    }

    protected void Arguing()
    {
        createMessage("Arguing with teacher", Color.red);
    }
}