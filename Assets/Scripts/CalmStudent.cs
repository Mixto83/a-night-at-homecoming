using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalmStudent : Student
{
    //parameters


    //methods
    public CalmStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.CalmStudent;
    }

    public override void Flirt()
    {
        Debug.Log("[" + name + "] Howdy!");
    }
}
