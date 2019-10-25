using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessyStudent : Student
{
    //parameters


    //methods
    public MessyStudent(string name, Genders gender, Vector2 position) : base(name, gender, position)
    {
        this.role = Roles.MessyStudent;
    }

    public override void Trouble()
    {
        Debug.Log("[" + name + "] Ni**er");
    }
}