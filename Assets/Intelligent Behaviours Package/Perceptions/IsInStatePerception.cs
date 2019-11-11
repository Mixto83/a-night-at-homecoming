using System.Collections;
using System.Collections.Generic;

public class IsInStatePerception : Perception {

    #region variables

    private State stateToLook;
    private BehaviourEngine engineToLook;

    #endregion variables

    public IsInStatePerception(BehaviourEngine engineToLook, string stateToLookFor, BehaviourEngine behaviourEngine)
    {
        this.engineToLook = engineToLook;
        this.stateToLook = engineToLook.GetState(stateToLookFor);
        base.behaviourEngine = behaviourEngine;
    }

    public override bool Check()
    {
        if(engineToLook.BehaviourMachine.State == stateToLook) {
            return true;
        }

        return false;
    }
}