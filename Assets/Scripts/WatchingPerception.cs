using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class WatchingPerception : Perception
{

    #region variables

    private GameObject watcher;
    private MeshCollider colliderVision;
    private Func<bool>[] conditions;
    private GameManager gameState;

    private Character targetCharacter;

    #endregion variables

    public WatchingPerception(GameObject watcher, params Func<bool>[] conditions)
    {
        this.watcher = watcher;
        this.colliderVision = watcher.gameObject.GetComponentInChildren<MeshCollider>();
        this.conditions = conditions;
        this.gameState = GameObject.FindObjectOfType<GameManager>();
    }

    public override bool Check()
    {
        foreach (Character character in gameState.GetPeople())
        {
            if (colliderVision.bounds.Contains(character.GetGameObject().transform.position))
            {
                targetCharacter = character;

                foreach (Func<bool> result in conditions)
                {
                    if (!result())
                        goto foo;
                }

                return true;

                foo:
                continue;
            }
        }

        return false;
    }

    public Character getTargetCharacter()
    {
        return targetCharacter;
    }
}