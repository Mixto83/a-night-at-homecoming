using System;
using System.Collections;
using System.Collections.Generic;

public class LoopUntilFailDecoratorNode : TreeNode {

    public LoopUntilFailDecoratorNode(string name, TreeNode child, BehaviourTreeEngine behaviourTree)
    {
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
        if(ReturnNodeValue() == ReturnValues.Succeed) {
            ReturnToParent();
            Child.Reset();
        }
    }

    private void Loop()
    {
        ReturnValue = ReturnValues.Running;
        Child.ReturnValue = ReturnValues.Running;
        new Transition("loop child", StateNode, new PushPerception(behaviourTree), Child.StateNode, behaviourTree)
                .FireTransition();
        behaviourTree.ActiveNode = Child;
    }

    public override ReturnValues ReturnNodeValue()
    {
        if(Child.ReturnValue == ReturnValues.Failed) {
            ReturnValue = ReturnValues.Succeed;
        }
        else {
            Loop();
        }

        return ReturnValue;
    }
}