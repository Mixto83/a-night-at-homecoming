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
    protected Vector2 initPos;
    #endregion


    //methods
    protected Student(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.initPos = new Vector2(Random.Range(-200, 200), Random.Range(-200, 200));

        this.FavFoods = new List<string>();
        this.FavAnimals = new List<string>();
        this.Hobbies = new List<string>();
    }

    public override void Enjoying()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm having fun!");
    }

    protected void InBench()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Sat in the bench");
    }

    protected void Punished()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Punished :(");
    }
}
