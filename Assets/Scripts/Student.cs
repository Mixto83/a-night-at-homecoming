using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters
    #region const
    protected const int thirstThreshold = 5;
    protected const int fatigueThreshold = 5;
    protected const int amusementThreshold = 5;
    #endregion

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


    //methods
    protected Student(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.FavFoods = new List<string>();
        this.FavAnimals = new List<string>();
        this.Hobbies = new List<string>();
    }

    protected void InBench()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Sat in the bench");
    }

    protected void Fight()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Take this Billy!");
    }

    protected void Punished()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Punished :(");
    }

    protected void CheckingAffinity()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Checking Affinity");
    }
}
