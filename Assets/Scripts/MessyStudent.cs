using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    protected enum messStates
    {
        lookForMess, sabotageDrink, negotiateOrganizer, runAway,
        botherTeacher, checkAffinity, fightStudent
    }
    protected messStates currentMessState;

    //methods
    public MessyStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.MessyStudent;
        this.currentMessState = messStates.lookForMess;
    }

    public override void Trouble()
    {
        Debug.Log("[" + name + "] Ni**er");
        
        //Messy Student's FSM
        switch (currentMessState)
        {
            case messStates.lookForMess:
                Debug.Log("[" + name + "] Looking for some trouble...");
                this.currentMessState = messStates.sabotageDrink;
                break;
            case messStates.sabotageDrink:
                Debug.Log("[" + name + "] Drinking should be fun!");
                this.currentMessState = messStates.botherTeacher;
                break;
            case messStates.botherTeacher:
                Debug.Log("[" + name + "] Hey, teacher! Leave those kids alone!");
                this.currentMessState = messStates.checkAffinity;
                break;
            case messStates.checkAffinity:
                Debug.Log("[" + name + "] Is this one chad or virgin?");
                this.currentMessState = messStates.fightStudent;
                break;
            case messStates.fightStudent:
                Debug.Log("[" + name + "] Take this Billy!");
                this.currentMessState = messStates.negotiateOrganizer;
                break;
            case messStates.negotiateOrganizer:
                Debug.Log("[" + name + "] Don't sneak!");
                this.currentMessState = messStates.runAway;
                break;
            case messStates.runAway:
                Debug.Log("[" + name + "] Run run run!");
                this.currentMessState = messStates.sabotageDrink;
                break;
            default:
                break;
        }
    }

}