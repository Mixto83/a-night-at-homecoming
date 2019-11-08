using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalmStudent : Student
{
    //parameters
    #region States
    public enum calmStates
    {
        start, enjoy, rest, drink, breath, flirt,
        fightStudent, punishment 
    }

    public enum flirtStates
    {
        checkAffinity, dance, kiss
    }

    public calmStates currentState;
    public flirtStates currentFlirtState;
    #endregion

    #region Stats

    protected Vector2 meetPos;
    protected const int affinityThreshold = 6;
    protected const int DanceAffinityThreshold = 8;
    protected Student targetStudent;

    #endregion

    //methods
    public CalmStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.CalmStudent;
        //this.currentState = calmStates.enjoy;
    }

    public override void Update()
    {
        //Calm Student's FSM
        switch (currentState)
        {
            case calmStates.start:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Just Starting!");
                findFriends();
                currentState = calmStates.enjoy;
                break;
            case calmStates.enjoy:
                Enjoying();
                if (this.amusement < amusementThreshold) currentState = calmStates.rest;
                if (this.thirst < thirstThreshold) currentState = calmStates.drink;
                if (this.fatigue > fatigueThreshold) currentState = calmStates.breath;
                break;
            case calmStates.rest:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Resting state");
                InBench();
                break;
            case calmStates.drink:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Drinking state");
                Drinking();
                break;
            case calmStates.breath:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Breathing state");
                Outside();
                if (this.fatigue < fatigueThreshold) currentState = calmStates.enjoy;
                break;
            case calmStates.flirt:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Flirt state");
                Flirt();
                if (this.CheckAffinity(targetStudent) < affinityThreshold) currentState = calmStates.enjoy;
                if (this.CheckAffinity(targetStudent) < DanceAffinityThreshold) currentState = calmStates.enjoy;//Para baile
                break;
            case calmStates.fightStudent:
                Fight();
                break;
            case calmStates.punishment:
                Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Punishment state");
                Punished();
                break;
            default:
                break;
        }
    }

    //Flirting State FSM: Calm Students
    public override void Flirt()
    {
        switch (currentFlirtState)
        {
            case flirtStates.checkAffinity:
                
                if (this.CheckAffinity(targetStudent) > affinityThreshold)
                {
                    Debug.Log("[" + name + ", " + getRole() + ", " + currentFlirtState + "] I like you!");
                    currentFlirtState = flirtStates.dance;
                }
                break;
            case flirtStates.dance:

                if (this.CheckAffinity(targetStudent) > DanceAffinityThreshold)
                {
                    Debug.Log("[" + name + ", " + getRole() + ", " + currentFlirtState + "] I like you so much!");
                    currentFlirtState = flirtStates.kiss;
                }
                Dancing();
                break;
            case flirtStates.kiss:
                Kissing();
                break;
        }
    }

    protected int CheckAffinity(Student targetStudent)
    {
        int affinity = 0;

        if (Hobbies[0] == targetStudent.Hobbies[0] || Hobbies[0] == targetStudent.Hobbies[1] || Hobbies[0] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[1] == targetStudent.Hobbies[0] || Hobbies[1] == targetStudent.Hobbies[1] || Hobbies[1] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[2] == targetStudent.Hobbies[0] || Hobbies[2] == targetStudent.Hobbies[1] || Hobbies[2] == targetStudent.Hobbies[2])
            affinity++;

        if (FavAnimals[0] == targetStudent.FavAnimals[0] || FavAnimals[0] == targetStudent.FavAnimals[1] || FavAnimals[0] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[1] == targetStudent.FavAnimals[0] || FavAnimals[1] == targetStudent.FavAnimals[1] || FavAnimals[1] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[2] == targetStudent.FavAnimals[0] || FavAnimals[2] == targetStudent.FavAnimals[2] || FavAnimals[2] == targetStudent.FavAnimals[2])
            affinity++;

        if (FavFoods[0] == targetStudent.FavFoods[0] || FavFoods[0] == targetStudent.FavFoods[1] || FavFoods[0] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[1] == targetStudent.FavFoods[0] || FavFoods[1] == targetStudent.FavFoods[1] || FavFoods[1] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[2] == targetStudent.FavFoods[0] || FavFoods[2] == targetStudent.FavFoods[1] || FavFoods[2] == targetStudent.FavFoods[2])
            affinity++;

        if (affinity > affinityTolerance)
        {
            Debug.Log("[" + name + ", " + getRole() + "] This dude is a total chad!");
        } else
        {
            Debug.Log("[" + name + ", " + getRole() + "] What a virgin!");
            Fight();
        }

        return affinity;
    }

    protected void Fight()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Take this Billy!");
    }

    protected void Dancing()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] I'm the dancing queen!");
    }

    protected void Kissing()
    {
        Debug.Log("[" + name + ", " + getRole() + ", " + currentState + "] Chuu");
    }

    protected void findFriends()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Finding friends");
    }

    protected void Outside()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Chilling outside");
    }
}
