using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters
    protected enum studentStates {start, roleBehaviour, rest, drink, breath, punishment}
    protected enum drinkStates { walkToBar, waitQueue, drinking}
    protected enum restStates { walkToBenches, satInBench}
    protected enum breathStates { walkOutside, stayOutside}
    protected enum punishmentStates { waitEndOfPunishment, scapeFromPunishment}

    protected studentStates currentState;
    protected drinkStates currentDrink;
    protected restStates currentRest;
    protected breathStates currentBreath;
    protected punishmentStates currentPunishment;

    //methods
    protected Student(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.currentState = studentStates.start;
        this.currentDrink = drinkStates.walkToBar;
        this.currentRest = restStates.walkToBenches;
        this.currentBreath = breathStates.walkOutside;
        this.currentPunishment = punishmentStates.waitEndOfPunishment;
    }

    public override void Enjoying()
    {
        Debug.Log("[" + name + "] I'm having fun!");

        //Student FSM
        switch (currentState)
        {
            case studentStates.start:
                Debug.Log("[" + name + "] Changing to my behaviour state");
                break;
            case studentStates.breath:
                Debug.Log("[" + name + "] Breathing state");
                break;
            case studentStates.drink:
                Debug.Log("[" + name + "] Drinking state");
                break;
            case studentStates.punishment:
                Debug.Log("[" + name + "] Punishment state");
                break;
            case studentStates.rest:
                Debug.Log("[" + name + "] Resting state");
                break;
            case studentStates.roleBehaviour:
                Debug.Log("[" + name + "] This is my special behaviour");
                break;
            default:
                break;
        }
    }

    //Drinking State FSM: Messy and Calm Students
    protected void Drinking()
    {
        switch (currentDrink)
        {
            case drinkStates.walkToBar:
                Debug.Log("[" + name + "] Walking to the bar...");
                break;
            case drinkStates.waitQueue:
                Debug.Log("[" + name + "] Waiting queue...");
                break;
            case drinkStates.drinking:
                Debug.Log("[" + name + "] Actually drinking!");
                break;
            default:
                break;
        }
    }

    //Resting State FSM: Messy and Calm Students
    protected void Resting()
    {
        switch (currentRest)
        {
            case restStates.walkToBenches:
                Debug.Log("[" + name + "] Walking to benches");
                break;
            case restStates.satInBench:
                Debug.Log("[" + name + "] Sat in the bench");
                break;
            default:
                break;
        }
    }

    //Breathing State FSM: Messy and Calm Students
    protected void Breathing()
    {
        switch (currentBreath)
        {
            case breathStates.walkOutside:
                Debug.Log("[" + name + "] Walking outside...");
                break;
            case breathStates.stayOutside:
                Debug.Log("[" + name + "] I'm outside!");
                break;
            default:
                break;
        }
    }

    //Punishment State FSM: Messy and Calm Students
    protected void Punishment()
    {
        switch (currentPunishment)
        {
            case punishmentStates.waitEndOfPunishment:
                Debug.Log("[" + name + "] Waiting for being free...");
                break;
            case punishmentStates.scapeFromPunishment:
                Debug.Log("[" + name + "] Now it's my chance!");
                break;
            default:
                break;
        }
    }
    
}
