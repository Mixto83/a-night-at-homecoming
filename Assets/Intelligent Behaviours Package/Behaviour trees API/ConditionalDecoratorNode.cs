using System;
using System.Collections;
using System.Collections.Generic;

public class ConditionalDecoratorNode : TreeNode {

    #region variables

    private Perception conditionPerception;

    #endregion variables

    public ConditionalDecoratorNode(string name, TreeNode child, Perception condition, BehaviourTreeEngine behaviourTree)
    {
        this.conditionPerception = condition;
        base.Child = child;
        Child.ParentNode = this;
        base.StateNode = new State(name, ToChild, behaviourTree);
        base.behaviourTree = behaviourTree;
    }

    private void ToChild()
    {
        if(Child.ReturnValue != ReturnValues.Running)
            return;

        ReturnValue = ReturnValues.Running;
        Child.ReturnValue = ReturnValues.Running;
        new Transition("to child", StateNode, new PushPerception(behaviourTree), Child.StateNode, behaviourTree)
            .FireTransition();

        behaviourTree.ActiveNode = Child;
    }

    public override void Update()
    {
        if(Child.ReturnValue != ReturnValues.Running) {
            if(ReturnNodeValue() != ReturnValues.Running) {
                ReturnToParent();
                Child.Reset();
            }
        }
    }

    public override ReturnValues ReturnNodeValue()
    {
        ReturnValue = conditionPerception.Check() ? ReturnValues.Succeed : ReturnValues.Failed;

        return ReturnValue;
    }
}