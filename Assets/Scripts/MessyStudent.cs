using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters
    public enum messStates
    {
        start, rest, drink, breath, punishment,
        lookForMess, sabotageDrink, negotiateOrganizer, runAway,
        botherTeacher, checkAffinity, fightStudent
    }
    public messStates currentState;

    //methods
    public MessyStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.MessyStudent;
        this.currentState = messStates.lookForMess;
    }

    public override void FSM()
    {
        //Messy Student's FSM
        switch (currentState)
        {
            case messStates.start:
                Debug.Log("[" + name + ", " + currentState + "] Just Starting!");
                Enjoying();
                break;
            case messStates.breath:
                Debug.Log("[" + name + ", " + currentState + "] Breathing state");
                Breathing();
                break;
            case messStates.drink:
                Debug.Log("[" + name + ", " + currentState + "] Drinking state");
                Drinking();
                break;
            case messStates.punishment:
                Debug.Log("[" + name + ", " + currentState + "] Punishment state");
                Punishment();
                break;
            case messStates.rest:
                Debug.Log("[" + name + ", " + currentState + "] Resting state");
                Resting();
                break;
            case messStates.lookForMess:
                Trouble();
                break;
            case messStates.sabotageDrink:
                SabotageDrink();
                break;
            case messStates.botherTeacher:
                BotherTeacher();
                break;
            case messStates.checkAffinity:
                CheckAffinity();
                break;
            case messStates.fightStudent:
                Fight();
                break;
            case messStates.negotiateOrganizer:
                Negotiate();
                break;
            case messStates.runAway:
                Run();
                break;
            default:
                break;
        }
    }

    public override void Trouble()
    {
        Debug.Log("[" + name + ", " + currentState + "] Looking for some trouble...");
    }

    protected void SabotageDrink()
    {
        Debug.Log("[" + name + ", " + currentState + "] Drinking should be fun!");
    }

    protected void BotherTeacher()
    {
        Debug.Log("[" + name + ", " + currentState + "] Hey, teacher! Leave those kids alone!");

    }

    protected void CheckAffinity()
    {
        Debug.Log("[" + name + ", " + currentState + "] Is this one chad or virgin?");
    }

    protected void Fight()
    {
        Debug.Log("[" + name + ", " + currentState + "] Take this Billy!");
    }

    protected void Negotiate()
    {
        Debug.Log("[" + name + ", " + currentState + "] Don't sneak!");
    }

    protected void Run()
    {
        Debug.Log("[" + name + ", " + currentState + "] Run run run!");
    }

}