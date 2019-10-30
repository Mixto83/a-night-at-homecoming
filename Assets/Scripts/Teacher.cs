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

    public override void FSM()
    {
        //Teacher's FSM
        switch (currentState)
        {
            case teacherStates.start:
                Debug.Log("[" + name + ", " + currentState + "] Just Starting!");
                break;
            case teacherStates.patrol:
                Debug.Log("[" + name + ", " + currentState + "] Patrol State");
                Patrol();
                break;
            case teacherStates.drink:
                Debug.Log("[" + name + ", " + currentState + "] Drinking state");
                Drinking();
                break;
            case teacherStates.door:
                Debug.Log("[" + name + ", " + currentState + "] Door state");
                AtDoor();
                break;
            case teacherStates.chase:
                Debug.Log("[" + name + ", " + currentState + "] Chasing state");
                Chasing();
                break;
            case teacherStates.punishment:
                Debug.Log("[" + name + ", " + currentState + "] Punishing state");
                Punishing();
                break;
            default:
                break;
        }
    }

    //Patrolling State FSM: Teachers
    public override void Patrol()
    {
        switch (currentPatrol)
        {
            case patrolStates.patrolling:
                Debug.Log("[" + name + ", " + currentPatrol + "] I'm watching you...");
                break;
            case patrolStates.talking:
                Debug.Log("[" + name + ", " + currentPatrol + "] Who did what??");
                break;
            default:
                break;
        }
        
    }

    //Chasing State FSM: Teachers
    protected void Chasing()
    {
        switch (currentChase)
        {
            case chaseStates.chaseStudent:
                Debug.Log("[" + name + ", " + currentChase + "] You can't get away from me!");
                break;
            case chaseStates.yellAtMessy:
                Debug.Log("[" + name + ", " + currentChase + "] You are gonna face the consecuences");
                break;
            case chaseStates.bringToPunishment:
                Debug.Log("[" + name + ", " + currentChase + "] Come to the punishment room, you punk!");
                break;
            default:
                break;
        }
    }

    //Punishment Room State FSM: Teachers
    protected void Punishing()
    {
        switch (currentPunishment)
        {
            case punishmentStates.looking:
                Debug.Log("[" + name + ", " + currentChase + "] I'm watching you...(punishment room)");
                break;
            case punishmentStates.readingNews:
                Debug.Log("[" + name + ", " + currentChase + "] President Tremp did what again?");
                break;
            default:
                break;
        }
    }
}
