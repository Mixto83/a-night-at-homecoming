using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : Authority
{
    //parameters
    #region States

    public enum teacherStates
    {
        start, door, patrol, chase, punishment, drink
    }

    public enum patrolStates
    {
        patrolling, talking
    }

    public enum chaseStates
    {
        chaseStudent, yellAtMessy, bringToPunishment
    }

    public enum punishmentStates
    {
        looking, readingNews
    }

    public teacherStates currentState;
    public patrolStates currentPatrol;
    public chaseStates currentChase;
    public punishmentStates currentPunishment;
    #endregion

    //methods
    public Teacher(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.Teacher;
        this.strictness = 1;
    }

    public override string FSM()
    {
        string extraState = "";

        //Teacher's FSM
        switch (currentState)
        {
            case teacherStates.start:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Just Starting!");
                break;
            case teacherStates.patrol:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Patrol State");
                extraState = Patrol();
                break;
            case teacherStates.drink:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Drinking state");
                extraState = Drinking();
                break;
            case teacherStates.door:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Door state");
                extraState = AtDoor();
                break;
            case teacherStates.chase:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Chasing state");
                extraState = Chasing();
                break;
            case teacherStates.punishment:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Punishing state");
                extraState = Punishing();
                break;
            default:
                break;
        }

        return "" + currentState + "  " + extraState;
    }

    //Patrolling State FSM: Teachers
    public override string Patrol()
    {
        switch (currentPatrol)
        {
            case patrolStates.patrolling:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] I'm watching you...");
                break;
            case patrolStates.talking:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] Who did what??");
                break;
            default:
                break;
        }

        return "" + currentPatrol;
        
    }

    //Chasing State FSM: Teachers
    protected string Chasing()
    {
        switch (currentChase)
        {
            case chaseStates.chaseStudent:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentChase + "] You can't get away from me!");
                break;
            case chaseStates.yellAtMessy:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentChase + "] You are gonna face the consecuences");
                break;
            case chaseStates.bringToPunishment:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentChase + "] Come to the punishment room, you punk!");
                break;
            default:
                break;
        }

        return "" + currentChase;
    }

    //Punishment Room State FSM: Teachers
    protected string Punishing()
    {
        switch (currentPunishment)
        {
            case punishmentStates.looking:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentChase + "] I'm watching you...(punishment room)");
                break;
            case punishmentStates.readingNews:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentChase + "] President Tremp did what again?");
                break;
            default:
                break;
        }

        return "" + currentPunishment;
    }
}
