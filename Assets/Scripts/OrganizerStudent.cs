using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Authority
{
    //parameters
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

    public override void FSM()
    {
        //Organizer Student's FSM
        switch (currentState)
        {
            case organizerStates.start:
                Debug.Log("[" + name + ", " + currentState + "] Just Starting!");
                break;
            case organizerStates.door:
                Debug.Log("[" + name + ", " + currentState + "] Door state");
                AtDoor();
                break;
            case organizerStates.serveDrink:
                Debug.Log("[" + name + ", " + currentState + "] Serve Drink state");
                ServeDrink();
                break;
            case organizerStates.patrol:
                Debug.Log("[" + name + ", " + currentState + "] Door state");
                Patrol();
                break;
            default:
                break;
        }
    }

    //Patrolling State FSM: Organizer Student
    public override void Patrol()
    {
        switch (currentPatrol)
        {
            case patrolStates.patrolling:
                Debug.Log("[" + name + ", " + currentPatrol + "] I'm watching you...");
                break;
            case patrolStates.chaseStudent:
                Debug.Log("[" + name + ", " + currentPatrol + "] You can't get away from me!");
                break;
            case patrolStates.negotiate:
                Debug.Log("[" + name + ", " + currentPatrol + "] I'm not sure if I should call a teacher");
                break;
            case patrolStates.callTeacher:
                Debug.Log("[" + name + ", " + currentPatrol + "] Hey, teacher! This kid made a mess");
                break;
            default:
                break;
        }

    }

    //Chasing State FSM: Teachers
    protected void ServeDrink()
    {
        switch (currentServeDrink)
        {
            case serveDrinkStates.waiting:
                Debug.Log("[" + name + ", " + currentServeDrink + "] Waiting at the bar...");
                break;
            case serveDrinkStates.serve:
                Debug.Log("[" + name + ", " + currentServeDrink + "] Here you go, enjoy your drink!");
                break;
            default:
                break;
        }
    }
}