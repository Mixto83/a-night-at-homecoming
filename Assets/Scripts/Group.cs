using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    private int groupSize;
    private Vector3 meetPos;
    private List<CalmStudent> friends;
    private int consistentID = 0;

    public Group(int groupSize)
    {
        this.groupSize = groupSize;
        this.meetPos = new Vector3(Random.Range(-4, 4), Random.Range(-4, 4));
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
            f.setFriendNumber(consistentID);
            consistentID++;
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

    public Vector3 getMyPos(int id)
    {
        Vector3 baseDir = new Vector3(-1, 0, 0);
        Vector3 localDir = Quaternion.Euler(0, 0, id * 360/groupSize) * baseDir;

        Vector3 offset = localDir - meetPos;

        return meetPos + localDir.normalized * groupSize * 0.3f;
    }

    public void reuniteFriends()
    {
        foreach (CalmStudent c in friends)
        {
            c.Move(meetPos);
        }
    }
}
