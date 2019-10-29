using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    public enum messStates
    {
        lookForMess, sabotageDrink, negotiateOrganizer, runAway,
        botherTeacher, checkAffinity, fightStudent
    }
    public messStates currentMessState;

    //methods
    public MessyStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.MessyStudent;
        this.currentMessState = messStates.lookForMess;
    }

    protected override void RoleHFSM()
    {
        //Messy Student's FSM
        switch (currentMessState)
        {
            case messStates.lookForMess:
                Debug.Log("[" + name + ", " + currentMessState + "] Looking for some trouble...");
                break;
            case messStates.sabotageDrink:
                Debug.Log("[" + name + ", " + currentMessState + "] Drinking should be fun!");
                break;
            case messStates.botherTeacher:
                Debug.Log("[" + name + ", " + currentMessState + "] Hey, teacher! Leave those kids alone!");
                break;
            case messStates.checkAffinity:
                Debug.Log("[" + name + ", " + currentMessState + "] Is this one chad or virgin?");
                break;
            case messStates.fightStudent:
                Debug.Log("[" + name + ", " + currentMessState + "] Take this Billy!");
                break;
            case messStates.negotiateOrganizer:
                Debug.Log("[" + name + ", " + currentMessState + "] Don't sneak!");
                break;
            case messStates.runAway:
                Debug.Log("[" + name + ", " + currentMessState + "] Run run run!");
                break;
            default:
                break;
        }
    }

    public override void Trouble()
    {
        //Debug.Log("[" + name + "] Messy HFSM");
    }

}