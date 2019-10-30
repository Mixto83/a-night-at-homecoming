﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Character
{
    //parameters
    protected string name;
    protected Roles role;
    protected Genders gender;
    protected Vector2 position;
    protected float movementSpeed = 0.5f;

    #region HFSM States
    public enum drinkStates { walkToBar, waitQueue, drinking }
    public enum doorStates { wait, welcome }

    public drinkStates currentDrink;
    public doorStates currentDoor;
    #endregion

    //methods
    protected Character(string name, Genders gender, Vector2 position)
    {
        this.name = name;
        this.gender = gender;
        this.position = position;
    }

    public string getName()
    {
        return this.name;
    }

    public Roles getRole()
    {
        return this.role;
    }

    public void Move()
    {
        this.position.x += movementSpeed;
        Debug.Log(name + "'s pos is " + position);
    }

    //Drinking State FSM: Messy and Calm Students and Teachers
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

    //Door State FSM: Organizer Students and Teachers
    protected void AtDoor()
    {
        switch (currentDoor)
        {
            case doorStates.wait:
                Debug.Log("[" + name + ", " + currentDoor + "] Waiting at the door...");
                break;
            case doorStates.welcome:
                Debug.Log("[" + name + ", " + currentDoor + "] Welcome to the party!");
                break;
            default:
                break;
        }
    }

    //Overrides

    public virtual void FSM()
    {
        Debug.Log("[" + name + "] Behaviour not defined");
    }
    public virtual void Patrol() {
        Debug.Log("[" + name + "] Behaviour not defined");
    }

    public virtual void Flirt() {
        Debug.Log("[" + name + "] Behaviour not defined");
    }

    public virtual void Trouble() {
        Debug.Log("[" + name + "] Behaviour not defined");
    }

    public virtual void Enjoying() {
        Debug.Log("[" + name + "] Behaviour not defined");
    }
}
