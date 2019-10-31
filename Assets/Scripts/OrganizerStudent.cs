using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Authority
{
    //parameters
    private int timeInServing = 0;

    #region States

    public enum organizerStates
    {
        start, door, serveDrink, patrol
    }

    public enum serveDrinkStates
    {
        waiting, serve
    }

    public enum patrolStates
    {
        patrolling, chaseStudent, negotiate, callTeacher
    }

    public organizerStates currentState;
    public serveDrinkStates currentServeDrink;
    public patrolStates currentPatrol;
    #endregion

    //methods
    public OrganizerStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.OrganizerStudent;
        this.strictness = 0.5f;
    }

    public override string FSM()
    {
        string extraState = "";

        //Organizer Student's FSM
        switch (currentState)
        {
            case organizerStates.start:
                if (Start())
                    currentState = (organizerStates) targetReached;
                break;
            case organizerStates.door:
                extraState = AtDoor();
                break;
            case organizerStates.serveDrink:
                extraState = ServeDrink();
                break;
            case organizerStates.patrol:
                extraState = Patrol();
                break;
            default:
                break;
        }

        return "" + currentState + " " + extraState;
    }

    //Patrolling State FSM: Organizer Student
    public override string Patrol()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Door state");
        switch (currentPatrol)
        {
            case patrolStates.patrolling:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] I'm watching you...");
                break;
            case patrolStates.chaseStudent:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] You can't get away from me!");
                break;
            case patrolStates.negotiate:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] I'm not sure if I should call a teacher");
                break;
            case patrolStates.callTeacher:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentPatrol + "] Hey, teacher! This kid made a mess");
                break;
            default:
                break;
        }

        return "" + currentPatrol;

    }

    //Chasing State FSM: Teachers
    protected string ServeDrink()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Serve Drink state");
        switch (currentServeDrink)
        {
            case serveDrinkStates.waiting:
                if (Waiting())
                {
                    currentServeDrink = serveDrinkStates.serve;
                }
                break;
            case serveDrinkStates.serve:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentServeDrink + "] Here you go, enjoy your drink!");
                if (timeInServing >= 100)
                {
                    timeInServing = 0;
                    currentServeDrink = serveDrinkStates.waiting;
                }
                else
                {
                    timeInServing++;
                }
                break;
            default:
                break;
        }

        return "" + currentServeDrink;
    }
}