using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Authority : Character
{
    //parameters
    protected int targetReached = 0; //0: None, 1: Door, 2: Bar, 3: Patrol initial position
    protected int timeInWelcome = 0;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    #region HFSM States
    public enum doorStates { wait, welcome }

    public doorStates currentDoor;
    #endregion

    //methods
    public Authority(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {

    }

    //Start State Common behaviour: Organizer Students and Teachers
    public override bool Start()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Just Starting!");

        //Move towards target (door, bar or patrol inital position)

        //Esto es para debug solo, en la version final habrá que mover el personaje
        //hacia el target que le toque, y cuando llegue se pone targetReached a 1, 2, 3 (segun que target sea)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            targetReached = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            targetReached = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            targetReached = 3;
        }

        if (targetReached == 1 || targetReached == 2 || targetReached == 3)
            return true;
        return false;
    }

    //Door State FSM: Organizer Students and Teachers
    protected string AtDoor()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Door state");
        switch (currentDoor)
        {
            case doorStates.wait:
                if(Waiting()) {
                    currentDoor = doorStates.welcome;
                }
                break;
            case doorStates.welcome:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentDoor + "] Welcome to the party!");
                if (timeInWelcome >= 100)
                {
                    timeInWelcome = 0;
                    currentDoor = doorStates.wait;
                } else
                {
                    timeInWelcome++;
                }
                break;
            default:
                break;
        }

        return "" + currentDoor;
    }

    protected bool Waiting()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentDoor + "] Waiting for someone to come...");

        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            return true;
        }

        return false;
    }
}
