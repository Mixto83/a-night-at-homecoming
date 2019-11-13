using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    private int groupSize;
    private Vector3 meetPos;
    private List<CalmStudent> friends;

    public Group(int groupSize)
    {
        this.groupSize = groupSize;
        this.meetPos = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5));
        this.friends = new List<CalmStudent>();
    }

    public int getGroupSize()
    {
        return groupSize;
    }

    public Vector3 getMeetPos()
    {
        return meetPos;
    }

    public bool pushFriend(CalmStudent f)
    {
        if (friends.Count < groupSize)
        {
            friends.Add(f);
            return true;
        } else
        {
            return false;
        }

    }

    public string toString()
    {
        string result = "";
        foreach (CalmStudent c in friends)
        {
            result += ", " + c.getName();
        }
        return result;
    }

    public void reuniteFriends()
    {
        foreach (CalmStudent c in friends)
        {
            c.Move(meetPos);
        }
    }

}
