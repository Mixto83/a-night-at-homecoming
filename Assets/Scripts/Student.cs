using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters
    #region Interests
    public List<string> FavFoods;
    public List<string> FavAnimals;
    public List<string> Hobbies;
    #endregion

    #region Stats
    protected float fatigue;
    protected float amusement;
    protected float thirst;
    protected int affinityTolerance = 5;
    #endregion

    #region HFSM States
    public enum restStates { walkToBenches, satInBench}
    public enum breathStates { walkOutside, stayOutside}
    public enum punishmentStates { waitEndOfPunishment, scapeFromPunishment}

    public restStates currentRest;
    public breathStates currentBreath;
    public punishmentStates currentPunishment;
    #endregion

    //methods
    protected Student(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.currentDrink = drinkStates.walkToBar;
        this.currentRest = restStates.walkToBenches;
        this.currentBreath = breathStates.walkOutside;
        this.currentPunishment = punishmentStates.waitEndOfPunishment;

        this.FavFoods = new List<string>();
        this.FavAnimals = new List<string>();
        this.Hobbies = new List<string>();
    }

    public override void Enjoying()
    {
        //Debug.Log("[" + name + "] I'm having fun!");
    }

    //Resting State FSM: Messy and Calm Students
    protected string Resting()
    {
        switch (currentRest)
        {
            case restStates.walkToBenches:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentRest + "] Walking to benches");
                break;
            case restStates.satInBench:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentRest + "] Sat in the bench");
                break;
            default:
                break;
        }

        return "" + currentRest;
    }

    //Breathing State FSM: Messy and Calm Students
    protected string Breathing()
    {
        switch (currentBreath)
        {
            case breathStates.walkOutside:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentBreath + "] Walking outside...");
                break;
            case breathStates.stayOutside:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentBreath + "] I'm outside!");
                break;
            default:
                break;
        }

        return "" + currentBreath;
    }

    //Punishment State FSM: Messy and Calm Students
    protected string Punishment()
    {
        switch (currentPunishment)
        {
            case punishmentStates.waitEndOfPunishment:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPunishment + "] Waiting for being free...");
                break;
            case punishmentStates.scapeFromPunishment:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPunishment + "] Now is my chance!");
                break;
            default:
                break;
        }

        return "" + currentPunishment;
    }
}
