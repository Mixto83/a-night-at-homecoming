using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchingPerception : Perception
{

    #region variables

    private GameObject watcher;
    private string target;
    private PolygonCollider2D colliderVision;

    #endregion variables

    public WatchingPerception(GameObject watcher, string target, PolygonCollider2D colliderVision)
    {
        this.watcher = watcher;
        this.target = target;
        this.colliderVision = colliderVision;
    }

    public override bool Check()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(target))
        {
            if (colliderVision.bounds.Contains(obj.transform.position))
            {
                return true;
            }
        }

        return false;
    }
}