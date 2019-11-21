﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters
    #region const
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
        createMessage("Boring music...", Color.blue);
        Move(new Vector3(-13.5f, 1f, 0));
    }

    protected void Punished()
    {
        createMessage("Waiting", Color.red);
        //Move to available seat
    }
}
