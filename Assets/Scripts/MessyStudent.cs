﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    #region States
    public enum messStates
    {
        start, rest, drink, breath, punishment,
        lookForMess, sabotageDrink, negotiateOrganizer, runAway,
        botherTeacher, checkAffinity, fightStudent
    }
    public messStates currentState;
    #endregion

    protected int affinityTolerance = 5;

    //methods
    public MessyStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.MessyStudent;
        this.currentState = messStates.lookForMess;
    }

    public override void FSM()
    {
        //Messy Student's FSM
        switch (currentState)
        {
            case messStates.start:
                Debug.Log("[" + name + ", " + currentState + "] Just Starting!");
                Enjoying();
                break;
            case messStates.breath:
                Debug.Log("[" + name + ", " + currentState + "] Breathing state");
                Breathing();
                break;
            case messStates.drink:
                Debug.Log("[" + name + ", " + currentState + "] Drinking state");
                Drinking();
                break;
            case messStates.punishment:
                Debug.Log("[" + name + ", " + currentState + "] Punishment state");
                Punishment();
                break;
            case messStates.rest:
                Debug.Log("[" + name + ", " + currentState + "] Resting state");
                Resting();
                break;
            case messStates.lookForMess:
                Trouble();
                break;
            case messStates.sabotageDrink:
                SabotageDrink();
                break;
            case messStates.botherTeacher:
                BotherTeacher();
                break;
            case messStates.checkAffinity:
                Debug.Log("[" + name + ", " + currentState + "] Is this one chad or virgin?");
                //CheckAffinity();
                break;
            case messStates.fightStudent:
                Fight();
                break;
            case messStates.negotiateOrganizer:
                Negotiate();
                break;
            case messStates.runAway:
                Run();
                break;
            default:
                break;
        }
    }

    public override void Trouble()
    {
        Debug.Log("[" + name + ", " + currentState + "] Looking for some trouble...");
        //Debug.Log("[" + name + "] Hobbies: " + Hobbies[0] + ", " + Hobbies[1] + " and " + Hobbies[2]);
        //Debug.Log("[" + name + "] Fav Foods: " + FavFoods[0] + ", " + FavFoods[1] + " and " + FavFoods[2]);
        //Debug.Log("[" + name + "] Fav Animals: " + FavAnimals[0] + ", " + FavAnimals[1] + " and " + FavAnimals[2]);           
    }

    protected void SabotageDrink()
    {
        Debug.Log("[" + name + ", " + currentState + "] Drinking should be fun!");
    }

    protected void BotherTeacher()
    {
        Debug.Log("[" + name + ", " + currentState + "] Hey, teacher! Leave those kids alone!");

    }

    protected void CheckAffinity(Student targetStudent)
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
            Debug.Log("[" + name + ", " + currentState + "] This dude is a total chad!");
        } else
        {
            Debug.Log("[" + name + ", " + currentState + "] What a virgin!");
            Fight();
        }
    }

    protected void Fight()
    {
        Debug.Log("[" + name + ", " + currentState + "] Take this Billy!");
    }

    protected void Negotiate()
    {
        Debug.Log("[" + name + ", " + currentState + "] Don't sneak!");
    }

    protected void Run()
    {
        Debug.Log("[" + name + ", " + currentState + "] Run run run!");
    }

}