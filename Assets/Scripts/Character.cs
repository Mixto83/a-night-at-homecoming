﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    //parameters
    protected StateMachineEngine drinkSubFSM;

    protected string name;
    protected Roles role;
    protected Genders gender;
    protected Vector2 position;
    protected float movementSpeed = 0.5f;

    //methods
    protected Character(string name, Genders gender, Vector2 position)
    {
        this.name = name;
        this.gender = gender;
        this.position = position;

        CreateDrinkSubStateMachine();
    }

    private void CreateDrinkSubStateMachine()
    {
        drinkSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception push = drinkSubFSM.CreatePerception<PushPerception>(); //temporal

        // States
        State walkingToBarState = drinkSubFSM.CreateState("WalkingToBar", WalkingToBar);
        State waitingQueueState = drinkSubFSM.CreateState("WaitingQueue", WaitingQueue);
        State drinkState = drinkSubFSM.CreateState("Drink", Drinking);

        // Transitions
        drinkSubFSM.CreateTransition("Join queue", walkingToBarState, push, waitingQueueState);
        drinkSubFSM.CreateTransition("Served", waitingQueueState, push, drinkState);
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

    //Common behaviours to be overridden
    public virtual void Update()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void Flirt() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void LookForTrouble() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void Enjoying() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");   
    }

    protected void WalkingToBar()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to bar");
    }

    protected void WaitingQueue()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Witing at queue");
    }

    protected void Drinking()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Drinking!");
    }
}
