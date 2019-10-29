using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters
    public enum drinkStates { walkToBar, waitQueue, drinking}
    public enum restStates { walkToBenches, satInBench}
    public enum breathStates { walkOutside, stayOutside}
    public enum punishmentStates { waitEndOfPunishment, scapeFromPunishment}

    public drinkStates currentDrink;
    public restStates currentRest;
    public breathStates currentBreath;
    public punishmentStates currentPunishment;

    //methods
    protected Student(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.currentDrink = drinkStates.walkToBar;
        this.currentRest = restStates.walkToBenches;
        this.currentBreath = breathStates.walkOutside;
        this.currentPunishment = punishmentStates.waitEndOfPunishment;
    }

    public override void Enjoying()
    {
        //Debug.Log("[" + name + "] I'm having fun!");
    }

    //Drinking State FSM: Messy and Calm Students
    protected void Drinking()
    {
        switch (currentDrink)
        {
            case drinkStates.walkToBar:
                Debug.Log("[" + name + ", " + currentDrink + "] Walking to the bar...");
                break;
            case drinkStates.waitQueue:
                Debug.Log("[" + name + ", " + currentDrink + "] Waiting queue...");
                break;
            case drinkStates.drinking:
                Debug.Log("[" + name + ", " + currentDrink + "] Actually drinking!");
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
                Debug.Log("[" + name + ", " + currentRest + "] Walking to benches");
                break;
            case restStates.satInBench:
                Debug.Log("[" + name + ", " + currentRest + "] Sat in the bench");
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
                Debug.Log("[" + name + ", " + currentBreath + "] Walking outside...");
                break;
            case breathStates.stayOutside:
                Debug.Log("[" + name + ", " + currentBreath + "] I'm outside!");
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
                Debug.Log("[" + name + ", " + currentPunishment + "] Waiting for being free...");
                break;
            case punishmentStates.scapeFromPunishment:
                Debug.Log("[" + name + ", " + currentPunishment + "] Now is my chance!");
                break;
            default:
                break;
        }
    }
}
