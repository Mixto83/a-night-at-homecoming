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

    protected Vector3 randomPunishSeat = Vector3.zero;
    protected float distanceToPunishPos = 1.5f;

    //methods
    protected Student(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.FavFoods = new List<string>();
        this.FavAnimals = new List<string>();
        this.Hobbies = new List<string>();
    }

    protected void InBench()
    {
        createMessage(9);
        if(fatigue > 0.0f) fatigue--;
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);

        var index = Random.Range(0, this.gameState.possiblePosBench.Count / 2 - 1) * 2;
        currentOcuppiedBench = this.gameState.possiblePosBench.GetRange(index, 2);
        this.gameState.possiblePosBench.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedBench[0], currentOcuppiedBench[1]));
    }

    //Se va a un sitio del aula de castigo
    protected void Punished()
    {
        LookAt(GameObject.FindGameObjectWithTag("Desk").transform);
    }
    

    //Se mueve a un asiento disponible del aula de castigo
    protected void MoveToRandomPunishmentRoomPos()
    {
        if (currentOcuppiedPunishment != null) this.gameState.possiblePosPunishment.AddRange(currentOcuppiedPunishment);
        var index = Random.Range(0, this.gameState.possiblePosPunishment.Count / 2 - 1) * 2;
        currentOcuppiedPunishment = this.gameState.possiblePosPunishment.GetRange(index, 2);
        this.gameState.possiblePosPunishment.RemoveRange(index, 2);
        randomPunishSeat = new Vector3(currentOcuppiedPunishment[0], currentOcuppiedPunishment[1]);
        Move(randomPunishSeat);
    }

    protected string listToString(List<string> list)
    {
        string res = "";
        var firstElem = true;

        List<string> auxList = new List<string>();

        foreach (string elem in list)
        {
            if(!auxList.Contains(elem)) auxList.Add(elem);
        }

        foreach (string elem in auxList)
        {
            if (!firstElem) res += ", ";

            res += elem;

            firstElem = false;
        }

        return res;
    }
}
