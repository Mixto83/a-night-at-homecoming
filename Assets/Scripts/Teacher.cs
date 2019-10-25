using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : Character
{
    //parameters
    protected float strictness;

    //methods
    public Teacher(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.Teacher;
        this.strictness = 1;
    }

    public override void Patrol()
    {
        Debug.Log("[" + name + "] I'm watching you...");
    }
}
