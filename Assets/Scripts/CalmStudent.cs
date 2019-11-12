﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalmStudent : Student
{
    //parameters
    private StateMachineEngine calmStudentFSM;
    private StateMachineEngine flirtSubFSM;

    #region Stats

    protected Vector2 meetPos;
    protected const int affinityThreshold = 6;
    protected const int DanceAffinityThreshold = 8;
    protected Student targetStudent;

    #endregion

    //methods
    public CalmStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.CalmStudent;

        CreateFlirtSubStateMachine();
        CreateStateMachine();
    }

    private void CreateFlirtSubStateMachine()
    {
        flirtSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = flirtSubFSM.CreatePerception<PushPerception>(); //temporal
        Perception timer = flirtSubFSM.CreatePerception<TimerPerception>(2);
        Perception affinityCheck = flirtSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) > affinityThreshold);
        Perception danceAffinityCheck = flirtSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) > DanceAffinityThreshold);
        Perception danceWithStudent = flirtSubFSM.CreateOrPerception<OrPerception>(timer, affinityCheck);
        Perception goBathroomWithStudent = flirtSubFSM.CreateOrPerception<OrPerception>(timer, danceAffinityCheck);

        // States
        State checkAffinityState = flirtSubFSM.CreateEntryState("CheckingAffinity", CheckingAffinity);
        State dancingState = flirtSubFSM.CreateState("Dancing", Dancing);
        State bathroomState = flirtSubFSM.CreateState("Bathroom", Kissing);

        // Transitions
        flirtSubFSM.CreateTransition("High affinity", checkAffinityState, danceWithStudent, dancingState);
        flirtSubFSM.CreateTransition("High affinity dancing", dancingState, goBathroomWithStudent, bathroomState);
    }

    private void CreateStateMachine()
    {
        calmStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = calmStudentFSM.CreatePerception<PushPerception>(); //temporal
        Perception timer = calmStudentFSM.CreatePerception<TimerPerception>(2);

        // States
        State startState = calmStudentFSM.CreateEntryState("Start");
        State enjoyState = calmStudentFSM.CreateState("Enjoying", Enjoying);
        State benchState = calmStudentFSM.CreateState("Bench", InBench);
        State drinkingState = calmStudentFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State outsideState = calmStudentFSM.CreateState("Outside", Outside);
        State fightingState = calmStudentFSM.CreateState("Fighting", Fight);
        State punishedState = calmStudentFSM.CreateState("Punished", Punished);
        State flirtState = calmStudentFSM.CreateSubStateMachine("Flirting", flirtSubFSM);

        // Transitions
        calmStudentFSM.CreateTransition("Look for friends", startState, push, enjoyState);

        calmStudentFSM.CreateTransition("Like Person", enjoyState, push, flirtState);
        calmStudentFSM.CreateTransition("Provoked by messy student", enjoyState, push, fightingState);
        calmStudentFSM.CreateTransition("Looking for fresh air", enjoyState, push, outsideState);
        calmStudentFSM.CreateTransition("Thirsty", enjoyState, push, drinkingState);
        calmStudentFSM.CreateTransition("Dislike music", enjoyState, push, benchState);

        flirtSubFSM.CreateExitTransition("Low affinity or finish kissing", flirtState, push, enjoyState);

        calmStudentFSM.CreateTransition("Finish fighting", fightingState, timer, enjoyState);
        calmStudentFSM.CreateTransition("Caught fighting", fightingState, push, punishedState);

        calmStudentFSM.CreateTransition("Finish punishment", punishedState, timer, enjoyState);

        calmStudentFSM.CreateTransition("Already taken enough fresh air", outsideState, timer, enjoyState);

        drinkSubFSM.CreateExitTransition("Not thirsty any more", drinkingState, push, enjoyState);

        calmStudentFSM.CreateTransition("Like music", benchState, push, enjoyState);
    }

    public override void Update()
    {
        drinkSubFSM.Update();
        flirtSubFSM.Update();
        calmStudentFSM.Update();

        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            calmStudentFSM.Fire("Look for friends");
        }
    }

    public override bool isInState(string subFSM, string subState)
    {
        Perception isIn;
        switch (subFSM)
        {
            case "Drink":
                isIn = calmStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, subState);
                break;
            case "Flirt":
                isIn = calmStudentFSM.CreatePerception<IsInStatePerception>(flirtSubFSM, subState);
                break;
            default:
                isIn = calmStudentFSM.CreatePerception<PushPerception>();
                break;
        }

        return isIn.Check();
    }

    protected int CheckAffinity(Student targetStudent)
    {
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
        } else
        {
            Debug.Log("[" + name + ", " + getRole() + "] What a virgin!");
            Fight();
        }

        return affinity;
    }

    protected void Dancing()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm the dancing queen!");
    }

    protected void Kissing()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Chuu");
    }

    protected void findFriends()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Finding friends");
    }

    protected void Outside()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Chilling outside");
    }
}
