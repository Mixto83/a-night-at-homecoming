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
