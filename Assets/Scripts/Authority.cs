using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Authority : Character
{
    //parameters
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
}
