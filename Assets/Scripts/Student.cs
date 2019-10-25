using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : Character
{
    //parameters

    //methods
    protected Student(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        
    }

    public override void Enjoying()
    {
        Debug.Log("[" + name + "] I'm having fun!");
    }
}
