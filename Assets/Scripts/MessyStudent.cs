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
        troubleSubFSM.CreateTransition("Busted by teacher", sabotageDrinkState, push, chaseState);

        troubleSubFSM.CreateTransition("Convinced organizer", negotiationState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Didn't convince organizer", negotiationState, push, chaseState);

        troubleSubFSM.CreateTransition("Escaped", chaseState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher", chaseState, push, arguingState);

        troubleSubFSM.CreateTransition("Finished bothering teacher", botherTeacherState, push, chaseState);

        troubleSubFSM.CreateTransition("Affinity is positive", checkAffinityState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Affinity is negative", checkAffinityState, fightStudent, fightState);

        troubleSubFSM.CreateTransition("Fight ends", fightState, push, lookingForTroubleState);
        troubleSubFSM.CreateTransition("Busted by teacher", fightState, push, arguingState);
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

    public override void CreateStateMachine()
    {
        messyStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = messyStudentFSM.CreatePerception<PushPerception>(); //temporal
        timeOut = messyStudentFSM.CreatePerception<TimerPerception>(2);
        isInStateDrink = messyStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");
        Perception exitDrink = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, timeOut); //not used
        Perception exitDrinkTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, push);
        Perception exitDrinkNotTired = messyStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, push);

        // States
        State startState = messyStudentFSM.CreateEntryState("Start");
        State drinkingState = messyStudentFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State benchState = messyStudentFSM.CreateState("Bench", InBench);
        State troubleState = messyStudentFSM.CreateSubStateMachine("Trouble", troubleSubFSM);
        State punishmentState = messyStudentFSM.CreateSubStateMachine("Punishment", punishmentSubFSM);

        // Transitions
        messyStudentFSM.CreateTransition("More thirsty than tired (1)", startState, push, drinkingState);
        messyStudentFSM.CreateTransition("More tired than thirsty", startState, push, benchState);
        messyStudentFSM.CreateTransition("Not thirsty, not tired (1)", startState, push, troubleState);

        drinkSubFSM.CreateExitTransition("Not thirsty, tired", drinkingState, exitDrinkTired, benchState);
        drinkSubFSM.CreateExitTransition("Not thirsty, not tired (2)", drinkingState, exitDrinkNotTired, troubleState);

        messyStudentFSM.CreateTransition("Not tired, thirsty", benchState, push, drinkingState);
        messyStudentFSM.CreateTransition("Not thirsty, not tired (3)", benchState, push, troubleState);

        troubleSubFSM.CreateExitTransition("Not tired, thirsty", troubleState, push, drinkingState);
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

        if (!isInStateDrink.Check())
        {
            timeOut.Reset();
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
        Debug.Log("[" + name + ", " + getRole() + "] Looking for some trouble...");
    }

    protected void SabotageDrink()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Drinking should be fun!");
    }

    protected void BotherTeacher()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Bothering teacher");

    }

    protected bool CheckAffinity()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Is this one chad or virgin?");

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
            Debug.Log("[" + name + ", " + getRole() + "] This dude is a total chad!");
            return false;
        }
        else
        {
            Debug.Log("[" + name + ", " + getRole() + "] What a virgin!");
            return true;
        }
    }

    protected void Negotiate()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Don't sneak!");
    }

    protected void Run()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Run run run!");
    }

    protected void Escape()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Ight imma head out");
    }

    protected void Arguing()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Arguing with teacher");
    }
}