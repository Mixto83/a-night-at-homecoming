using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchingPerception : Perception
{

    #region variables

    private GameObject watcher;
    private string target;
    private MeshCollider colliderVision;
    private string place;

    private Character targetCharacter;

    #endregion variables

    public WatchingPerception(GameObject watcher, string target, MeshCollider colliderVision, string place)
    {
        this.watcher = watcher;
        this.target = target;
        this.colliderVision = colliderVision;
        this.place = place;
    }

    public override bool Check()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(target))
        {
            if (colliderVision.bounds.Contains(obj.transform.position))
            {
                bool condition;

                targetCharacter = GameObject.FindObjectOfType<GameManager>().GetCharacter(obj);
                if (place == "Door") {
                    condition = !targetCharacter.getGreeted();
                } else if(place == "Bar")
                {
                    condition = !targetCharacter.getServed();
                } else
                {
                    condition = false;
                }
                if (condition)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Character getTargetCharacter()
    {
        return targetCharacter;
    }
}