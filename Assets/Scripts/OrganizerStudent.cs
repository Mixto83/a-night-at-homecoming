using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerStudent : Student
{
    //parameters
    protected float strictness;

    //methods
    public OrganizerStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.OrganizerStudent;
        this.strictness = 0.5f;
    }

    public override void Patrol()
    {
        Debug.Log("[" + name + "] Watching you...");
    }
}