using System.Collections;
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

    public drinkStates currentDrink;
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
    protected string Drinking()
    {
        switch (currentDrink)
        {
            case drinkStates.walkToBar:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentDrink + "] Walking to the bar...");
                break;
            case drinkStates.waitQueue:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentDrink + "] Waiting queue...");
                break;
            case drinkStates.drinking:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentDrink + "] Actually drinking!");
                break;
            default:
                break;
        }

        return "" + currentDrink;
    }

    //Overrides

    public virtual bool Start()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
        return false;
    }

    public virtual string FSM()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
        return "NOT DEFINED";
    }

    public virtual string Patrol() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
        return "NOT DEFINED";
    }

    public virtual string Flirt() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
        return "NOT DEFINED";
    }

    public virtual void Trouble() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void Enjoying() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");   
    }
}
